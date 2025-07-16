// DTOs/ExamScheduleCreateDto.cs
using System;

namespace DiplomBackend.DTOs
{
    public class ExamScheduleCreateDto
    {
        public DateTime DateTime { get; set; }
        public string RoomNumber { get; set; } = null!;
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public int GroupId { get; set; }
        public string Type { get; set; } = null!;
    }
}
