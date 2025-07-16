// DTOs/GradeEntryDto.cs
namespace DiplomBackend.DTOs;

public class GradeEntryDto
{
    public int Id { get; set; }
    public int? Grade { get; set; }
    public UserDto Student { get; set; } = null!;
    public int ExamScheduleId { get; set; }
}