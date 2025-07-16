using DiplomBackend.Data;
using DiplomBackend.DTOs;
using DiplomBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DiplomBackend.Controllers
{
    [ApiController]
    [Route("api/teacher")]
    [Authorize(Roles = "Teacher")]
    public class TeacherController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly int _currentTeacherId;

        public TeacherController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _currentTeacherId = int.Parse(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            Console.WriteLine("construct");
        }

        [HttpGet("exams")]
        public async Task<IActionResult> GetExams()
        {
            try
            {
                int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                var exams = await _context.ExamSchedules
                    .Where(e => e.TeacherId == teacherId)
                    .Include(e => e.Subject)
                    .Include(e => e.Group)
                    .ToListAsync();

                return Ok(exams);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Ошибка в GetExams: {ex}");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        [HttpGet("schedule")]
        public async Task<ActionResult<List<TeacherScheduleDto>>> GetTeacherSchedule(
            [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var query = _context.ExamSchedules
                .Where(e => e.TeacherId == _currentTeacherId)
                .Include(e => e.Subject)
                .Include(e => e.Group)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(e => e.DateTime >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.DateTime <= endDate.Value);

            var schedule = await query
                .OrderBy(e => e.DateTime)
                .Select(e => new TeacherScheduleDto
                {
                    Id = e.Id,
                    DateTime = e.DateTime,
                    RoomNumber = e.RoomNumber,
                    Type = e.Type,
                    SubjectName = e.Subject.Name,
                    GroupNumber = e.Group.GroupNumber
                })
                .ToListAsync();

            return schedule;
        }

        [HttpGet("groups")]
        public async Task<ActionResult<List<GroupDto>>> GetTeacherGroups()
        {
            var groups = await _context.ExamSchedules
                .Where(e => e.TeacherId == _currentTeacherId)
                .Select(e => e.Group)
                .Distinct()
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    GroupNumber = g.GroupNumber,
                    StudentCount = g.Students.Count
                })
                .ToListAsync();

            return groups;
        }

        [HttpGet("grade-sheets")]
        public async Task<ActionResult<GetGradesDto>> GetGradeSheets()
        {
            var sheets = await _context.GradeSheets
                .Where(gs => gs.ExamSchedule.TeacherId == _currentTeacherId)
                .Include(gs => gs.ExamSchedule)
                    .ThenInclude(e => e.Subject)
                .Include(gs => gs.ExamSchedule.Group)
                .Include(gs => gs.GradeEntries)
                    .ThenInclude(ge => ge.Student)
                .Select(gs => new GradeSheetDto
                {
                    Id = gs.Id,
                    CreatedAt = gs.CreatedAt,
                    ExamSchedule = new ExamScheduleDto
                    {
                        Id = gs.ExamSchedule.Id,
                        DateTime = gs.ExamSchedule.DateTime,
                        RoomNumber = gs.ExamSchedule.RoomNumber,
                        Subject = new SubjectDto { Id = gs.ExamSchedule.Subject.Id, Name = gs.ExamSchedule.Subject.Name },
                        Group = new GroupDto { Id = gs.ExamSchedule.Group.Id, GroupNumber = gs.ExamSchedule.Group.GroupNumber }
                    },
                    GradeEntries = gs.GradeEntries.Select(ge => new GradeEntryDto
                    {
                        Id = ge.Id,
                        Grade = ge.Grade,
                        Student = new UserDto
                        {
                            Id = ge.Student.Id,
                            FullName = ge.Student.FullName
                        },
                        ExamScheduleId = gs.ExamSchedule.Id
                    }).ToList()
                })
                .ToListAsync();

            return new GetGradesDto { GradeSheets = sheets };
        }

        [HttpGet("grade-sheets/{id}")]
        public async Task<ActionResult<GradeSheetDetailsDto>> GetGradeSheetDetails(int id)
        {
            var sheet = await _context.GradeSheets
                .Include(gs => gs.ExamSchedule)
                    .ThenInclude(e => e.Subject)
                .Include(gs => gs.ExamSchedule.Group)
                .Include(gs => gs.GradeEntries)
                    .ThenInclude(ge => ge.Student)
                .FirstOrDefaultAsync(gs => gs.Id == id && gs.ExamSchedule.TeacherId == _currentTeacherId);

            if (sheet == null)
                return NotFound();

            var result = new GradeSheetDetailsDto
            {
                Id = sheet.Id,
                ExamSchedule = new ExamScheduleDto
                {
                    Id = sheet.ExamSchedule.Id,
                    DateTime = sheet.ExamSchedule.DateTime,
                    RoomNumber = sheet.ExamSchedule.RoomNumber,
                    Subject = new SubjectDto { Id = sheet.ExamSchedule.Subject.Id, Name = sheet.ExamSchedule.Subject.Name },
                    Group = new GroupDto { Id = sheet.ExamSchedule.Group.Id, GroupNumber = sheet.ExamSchedule.Group.GroupNumber }
                },
                GradeEntries = sheet.GradeEntries.Select(ge => new GradeEntryDto
                {
                    Id = ge.Id,
                    Grade = ge.Grade,
                    Student = new UserDto
                    {
                        Id = ge.Student.Id,
                        FullName = ge.Student.FullName,
                        Username = ge.Student.Username
                    },
                    ExamScheduleId = sheet.ExamSchedule.Id
                }).ToList()
            };

            return result;
        }

        [HttpPut("grade-entries/{id}")]
        public async Task<IActionResult> UpdateGrade(int id, [FromBody] UpdateGradeRequest request)
        {
            var gradeEntry = await _context.GradeEntries
                .Include(ge => ge.GradeSheet)
                    .ThenInclude(gs => gs.ExamSchedule)
                .FirstOrDefaultAsync(ge => ge.Id == id && ge.GradeSheet.ExamSchedule.TeacherId == _currentTeacherId);

            if (gradeEntry == null)
                return NotFound();

            if (request.Grade < 2 || request.Grade > 5)
                return BadRequest("Оценка должна быть от 2 до 5");

            gradeEntry.Grade = request.Grade;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("subjects")]
        public async Task<ActionResult<List<SubjectDto>>> GetTeacherSubjects()
        {
            var subjects = await _context.SubjectTeachers
                .Where(st => st.TeacherId == _currentTeacherId)
                .Select(st => new SubjectDto
                {
                    Id = st.Subject.Id,
                    Name = st.Subject.Name
                })
                .ToListAsync();

            return subjects;
        }
    }
}