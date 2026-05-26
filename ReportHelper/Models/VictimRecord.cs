namespace ReportHelper.Models
{
    public class VictimRecord
    {
        public int Id { get; set; }
        public int ReportId { get; set; } // foreign key → ReportRecord.Id
        public string FullName { get; set; } = string.Empty;
        public string DateOfBirth { get; set; } = string.Empty;
        public string HomeAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? EmailAddress { get; set; } // optional
        public string? RelationshipToSuspect { get; set; } // optional
        public string? InjuriesSustained { get; set; } // optional
        public string? MedicalTreatment { get; set; } // optional
    }
}
