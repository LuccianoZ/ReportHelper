namespace ReportHelper.Models
{
    public class ReportRecord
    {
        public int Id { get; set; } //primary key
        public string ReportNumber { get; set; } = string.Empty; //e.g. "20260525-0001"
        public string IncidentClassification { get; set; } = string.Empty; // S1.7      e.g. "Theft", "Assault", etc. 
        public DateTime ReportDate { get; set; } // S1.2
        public DateTime IncidentDate { get; set; } // S1.8
        public string OfficerName { get; set; } = string.Empty; // S1.4
        public string BadgeNumber { get; set; } = string.Empty; // S1.5
        public string NarrativeText { get; set; } = string.Empty; // S7.1       the full narrative
        public bool IsComplete { get; set; } // false = draft, true = finalized report
        public DateTime CreatedAt { get; set; } // when the record was first created
        public DateTime? CompletedAt { get; set; } // null until sign off

    }
}
