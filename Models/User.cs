namespace DiplomBackend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public int? GroupId { get; set; }
        public Group? Group { get; set; }
        public List<SubjectTeacher> SubjectTeachers { get; set; } = new();


    }
}
