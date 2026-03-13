using System;
using System.Collections.Generic;

namespace Apllication.DTOs.Dashboard
{
    public class ProjectStatsDto
    {
        public int ProjectId { get; set; }
        public double TotalEstimatedHours { get; set; }
        public double TotalActualHours { get; set; }
        public double CompletionPercentage { get; set; }
        public List<MemberWorkHoursDto> MemberStats { get; set; } = new();
        public List<DailyWorkHoursDto> TimeSeriesStats { get; set; } = new();
    }

    public class MemberWorkHoursDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public double TotalHours { get; set; }
    }

    public class DailyWorkHoursDto
    {
        public DateTime Date { get; set; }
        public double TotalHours { get; set; }
    }
}
