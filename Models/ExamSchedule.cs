namespace DiplomBackend.Models
{
    public class ExamSchedule
    {
        public int Id { get; set; }

        public DateTime DateTime { get; set; }
        public string RoomNumber { get; set; } = null!;

        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public int TeacherId { get; set; }
        public User Teacher { get; set; } = null!;

        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;

        public string Type { get; set; } = null!; 
    }
}