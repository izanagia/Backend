namespace DiplomBackend.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public List<SubjectTeacher> SubjectTeachers { get; set; } = new();
    }
}
