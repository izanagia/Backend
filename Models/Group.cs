using DiplomBackend.Models;

public class Group
{
    public int Id { get; set; }
    public string GroupNumber { get; set; } = string.Empty;

    public List<User> Students { get; set; } = new();
}
