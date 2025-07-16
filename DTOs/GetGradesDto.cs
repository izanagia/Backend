using DiplomBackend.DTOs;

public class GetGradesDto
{
    public List<GradeSheetDto> GradeSheets { get; set; } = new List<GradeSheetDto>();
}