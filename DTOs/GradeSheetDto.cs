// DTOs/GradeSheetDto.cs
namespace DiplomBackend.DTOs;

public class GradeSheetDto
{
    public int Id { get; set; }
    public ExamScheduleDto ExamSchedule { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<GradeEntryDto> GradeEntries { get; set; } = new();

}