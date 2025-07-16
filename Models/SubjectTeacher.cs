namespace DiplomBackend.Models
{
    public class SubjectTeacher
    {
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;

        public int TeacherId { get; set; }
        public User Teacher { get; set; } = null!;
    }
}
