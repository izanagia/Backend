// DTOs/ExamScheduleDto.cs
namespace DiplomBackend.DTOs;

public class ExamScheduleDto
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string RoomNumber { get; set; } = null!;
    public SubjectDto Subject { get; set; } = null!;
    public UserDto Teacher { get; set; } = null!;
    public GroupDto Group { get; set; } = null!;
    public string Type { get; set; } = null!;
}