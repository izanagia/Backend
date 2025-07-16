// DTOs/GroupDto.cs
namespace DiplomBackend.DTOs;

public class GroupDto
{
    public int Id { get; set; }
    public string GroupNumber { get; set; } = string.Empty;
    public List<StudentDto> Students { get; set; } = new();
    public int StudentCount { get; set; }
}