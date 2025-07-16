using System.Text;
using DiplomBackend.Data;
using DiplomBackend.DTOs;
using DiplomBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography; 
using System.Text;

namespace DiplomBackend.Controllers
{
    public class RegisterRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
    }
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (_context.Users.Any(u => u.Username == request.Username))
                return BadRequest("Пользователь уже существует.");

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = ComputeHash(request.Password),
                FullName = request.FullName,
                Role = request.Role
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok("Пользователь зарегистрирован.");
        }
  
        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            if (await _context.Groups.AnyAsync(g => g.GroupNumber == request.GroupNumber))
                return BadRequest("Группа с таким номером уже существует");

            var group = new Group { GroupNumber = request.GroupNumber };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            return Ok(group);
        }

        public class CreateGroupRequest
        {
            public string GroupNumber { get; set; }
        }

  
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _context.Users
                .Where(u => u.Role == "Student")
                .ToListAsync();

            return Ok(students);
        }

     
        [HttpPost("groups/{groupId}/add-student/{studentId}")]
        public async Task<IActionResult> AddStudentToGroup(int groupId, int studentId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            var student = await _context.Users.FindAsync(studentId);

            if (group == null || student == null)
                return NotFound("Группа или студент не найдены");

            if (student.Role != "Student")
                return BadRequest("Можно добавлять только студентов");

            student.GroupId = group.Id;
            await _context.SaveChangesAsync();

            return Ok("Студент добавлен в группу");
        }

        
        [HttpGet("groups")]
        public ActionResult<List<GroupDto>> GetGroups()
        {
            var groups = _context.Groups
                .Select(g => new GroupDto
                {
                    Id = g.Id,
                    GroupNumber = g.GroupNumber,
                    Students = g.Students.Select(s => new StudentDto
                    {
                        Id = s.Id,
                        UserName = s.Username,
                        FullName = s.FullName
                    }).ToList()
                })
                .ToList();

            return Ok(groups);
        }

        [HttpPost("subjects")]
        public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectRequest request)
        {
            var subject = new Subject { Name = request.Name };
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();
            return Ok(subject);
        }

        [HttpPost("subjects/assign-teacher")]
        public async Task<IActionResult> AssignTeacher([FromBody] AssignTeacherRequest request)
        {
            var subject = await _context.Subjects.FindAsync(request.SubjectId);
            var teacher = await _context.Users.FindAsync(request.TeacherId);

            if (subject == null || teacher == null || teacher.Role != "Teacher")
            {
                return BadRequest("Неверный предмет или преподаватель");
            }

            var existing = await _context.SubjectTeachers.FindAsync(request.SubjectId, request.TeacherId);
            if (existing != null)
            {
                return BadRequest("Преподаватель уже назначен");
            }

            var assignment = new SubjectTeacher
            {
                SubjectId = request.SubjectId,
                TeacherId = request.TeacherId
            };

            _context.SubjectTeachers.Add(assignment);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("subjects")]
        public IActionResult GetSubjects()
        {
            var subjects = _context.Subjects
                .Include(s => s.SubjectTeachers)
                .ThenInclude(st => st.Teacher)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    Teachers = s.SubjectTeachers.Select(st => new
                    {
                        st.Teacher.Id,
                        st.Teacher.FullName,
                        st.Teacher.Username
                    }).ToList()
                }).ToList();

            return Ok(subjects);
        }

        
        [HttpGet("teachers")]
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = await _context.Users
                .Where(u => u.Role == "Teacher")
                .ToListAsync();

            return Ok(teachers);
        }


  
        [HttpPost("exams")]
        public async Task<IActionResult> CreateExam([FromBody] ExamScheduleCreateDto examDto)
        {
            var teacher = await _context.Users.FindAsync(examDto.TeacherId);
            var subject = await _context.Subjects.FindAsync(examDto.SubjectId);
            var group = await _context.Groups.FindAsync(examDto.GroupId);

            if (teacher == null) return BadRequest("Преподаватель не найден.");
            if (subject == null) return BadRequest("Дисциплина не найдена.");
            if (group == null) return BadRequest("Группа не найдена.");

            var exam = new ExamSchedule
            {
                DateTime = examDto.DateTime,
                RoomNumber = examDto.RoomNumber,
                Type = examDto.Type,
                SubjectId = examDto.SubjectId,
                TeacherId = examDto.TeacherId,
                GroupId = examDto.GroupId
            };

            _context.ExamSchedules.Add(exam);
            await _context.SaveChangesAsync();

            return Ok(exam);
        }

        [HttpGet("exams")]
        public async Task<IActionResult> GetAll()
        {
            var exams = await _context.ExamSchedules
                .Include(e => e.Subject)
                .Include(e => e.Teacher)
                .Include(e => e.Group)
                .ToListAsync();

            return Ok(exams);
        }

       
        [HttpDelete("exams/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _context.ExamSchedules.FindAsync(id);
            if (exam == null) return NotFound();

            _context.ExamSchedules.Remove(exam);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("sheets")]
        public async Task<IActionResult> GetAllSheets()
        {
            var sheets = await _context.GradeSheets
                .Include(s => s.ExamSchedule)
                    .ThenInclude(e => e.Subject)
                .Include(s => s.ExamSchedule.Teacher)
                .Include(s => s.ExamSchedule.Group)
                .Include(s => s.GradeEntries)
                    .ThenInclude(e => e.Student)
                        .ThenInclude(s => s.Group)
                .Select(s => new GradeSheetDto
                {
                    Id = s.Id,
                    CreatedAt = s.CreatedAt,
                    ExamSchedule = new ExamScheduleDto
                    {
                        Id = s.ExamSchedule.Id,
                        DateTime = s.ExamSchedule.DateTime,
                        RoomNumber = s.ExamSchedule.RoomNumber,
                        Type = s.ExamSchedule.Type,
                        Subject = new SubjectDto
                        {
                            Id = s.ExamSchedule.Subject.Id,
                            Name = s.ExamSchedule.Subject.Name
                        },
                        Teacher = new UserDto
                        {
                            Id = s.ExamSchedule.Teacher.Id,
                            FullName = s.ExamSchedule.Teacher.FullName,
                            Role = s.ExamSchedule.Teacher.Role
                        },
                        Group = new GroupDto
                        {
                            Id = s.ExamSchedule.Group.Id,
                            GroupNumber = s.ExamSchedule.Group.GroupNumber
                        }
                    },
                    GradeEntries = s.GradeEntries.Select(e => new GradeEntryDto
                    {
                        Id = e.Id,
                        Grade = e.Grade,
                        Student = new UserDto
                        {
                            Id = e.Student.Id,
                            FullName = e.Student.FullName,
                            Role = e.Student.Role,
                            Group = e.Student.Group != null ? new GroupDto
                            {
                                Id = e.Student.Group.Id,
                                GroupNumber = e.Student.Group.GroupNumber
                            } : null
                        }
                    }).ToList()
                })
                .ToListAsync();

            return Ok(sheets);
        }

        [HttpPost("sheets")]
        public async Task<IActionResult> Create([FromBody] CreateGradeSheetFromScheduleRequest request)
        {
            try
            {
                var schedule = await _context.ExamSchedules
                    .Include(e => e.Group)
                        .ThenInclude(g => g.Students)
                    .FirstOrDefaultAsync(e => e.Id == request.ExamScheduleId);

                if (schedule == null)
                    return NotFound($"Экзамен с ID {request.ExamScheduleId} не найден");

                if (schedule.Group == null)
                    return BadRequest("Для экзамена не указана группа");

                if (!schedule.Group.Students.Any())
                    return BadRequest($"В группе {schedule.Group.GroupNumber} нет студентов");

                if (await _context.GradeSheets.AnyAsync(g => g.ExamScheduleId == request.ExamScheduleId))
                    return BadRequest($"Ведомость для экзамена {request.ExamScheduleId} уже существует");

                var sheet = new GradeSheet
                {
                    ExamScheduleId = schedule.Id,
                    GradeEntries = schedule.Group.Students.Select(s => new GradeEntry
                    {
                        StudentId = s.Id,
                        Grade = null
                    }).ToList()
                };

                _context.GradeSheets.Add(sheet);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Id = sheet.Id,
                    ExamScheduleId = sheet.ExamScheduleId,
                    CreatedAt = sheet.CreatedAt,
                    EntryCount = sheet.GradeEntries.Count
                });
            }
            catch (Exception ex)
            {
              
                Console.WriteLine($"Ошибка при создании ведомости: {ex}");
                return StatusCode(500, "Внутренняя ошибка сервера при создании ведомости");
            }
        }

        public class CreateGradeSheetFromScheduleRequest
        {
            public int ExamScheduleId { get; set; }
        }
        private static string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }
    }

}

