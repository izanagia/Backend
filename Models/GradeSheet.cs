using DiplomBackend.Models;

public class GradeSheet
{
    public int Id { get; set; }

    public int ExamScheduleId { get; set; }
    public ExamSchedule ExamSchedule { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<GradeEntry> GradeEntries { get; set; } = new();

}
