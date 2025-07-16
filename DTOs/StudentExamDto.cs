public class StudentExamDto
{
    public int Id { get; set; }
    public DateTime DateTime { get; set; }
    public string RoomNumber { get; set; }
    public string Subject { get; set; }
    public string Group { get; set; }
    public string Teacher { get; set; }
    public string Type { get; set; } // "Exam", "Test" и т.д.
}