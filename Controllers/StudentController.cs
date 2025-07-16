using System.Security.Claims;
using DiplomBackend.Data;
using DiplomBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/student")]
[Authorize(Roles = "Student")]
public class StudentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly int student_id;

    public StudentController(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        student_id = int.Parse(httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }

    [HttpGet("exams")]
    public async Task<ActionResult<List<StudentExamDto>>> GetExams()
    {
        var exams = await _context.ExamSchedules
            .Where(e => e.Group.Students.Any(s => s.Id == student_id))
            .Include(e => e.Subject)
            .Include(e => e.Group)
            .Include(e => e.Teacher)
            .Select(e => new StudentExamDto
            {
                Id = e.Id,
                DateTime = e.DateTime,
                RoomNumber = e.RoomNumber,
                Subject = e.Subject.Name,
                Group = e.Group.GroupNumber,
                Teacher = e.Teacher.FullName,
                Type = e.Type
            })
            .OrderBy(e => e.DateTime)
            .ToListAsync();

        return exams;
    }

    [HttpGet("grades")]
    public async Task<ActionResult<List<StudentGradeDto>>> GetGrades()
    {
        var grades = await _context.GradeEntries
            .Where(ge => ge.StudentId == student_id)
            .Include(ge => ge.GradeSheet)
                .ThenInclude(gs => gs.ExamSchedule)
                    .ThenInclude(e => e.Subject)
            .Include(ge => ge.GradeSheet.ExamSchedule.Teacher)
            .Select(ge => new StudentGradeDto
            {
                Id = ge.Id,
                Grade = ge.Grade,
                GradeSheetId = ge.GradeSheetId,
                 
            })
            
            .ToListAsync();

        return grades;
    }

    
}
