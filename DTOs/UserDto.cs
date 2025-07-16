// DTOs/UserDto.cs
namespace DiplomBackend.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;

    public string Username { get; set; } = null!;
    public string Role { get; set; } = null!;
    public GroupDto? Group { get; set; }
}