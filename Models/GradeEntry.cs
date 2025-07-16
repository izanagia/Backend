using System.ComponentModel.DataAnnotations.Schema;
using DiplomBackend.Models;
using Microsoft.EntityFrameworkCore;

public class GradeEntry
{
    public int Id { get; set; }

    public int GradeSheetId { get; set; }
    public GradeSheet GradeSheet { get; set; } = null!;

    public int StudentId { get; set; }
    public User Student { get; set; } = null!;

    public int? Grade { get; set; } 
}
