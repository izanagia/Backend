// DTOs/GradeSheetDetailsDto.cs (исправленный)
namespace DiplomBackend.DTOs;

public class GradeSheetDetailsDto
{
    public int Id { get; set; }
    public ExamScheduleDto ExamSchedule { get; set; } = null!;
    public List<GradeEntryDto> GradeEntries { get; set; } = new();
}