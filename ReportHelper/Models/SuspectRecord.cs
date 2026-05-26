namespace ReportHelper.Models
{
    public class SuspectRecord
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string FullName { get; set; } = string.Empty; // may be "Unknown"
        public string RaceSex { get; set; } = string.Empty; // required — combined field
        public string HeightWeight { get; set; } = string.Empty; // required
        public string HairEyeColor { get; set; } = string.Empty; // required
        public string ClothingDescription { get; set; } = string.Empty; // required
        public string? DateOfBirth { get; set; } // optional
        public string? Address { get; set; } // optional
        public string? DistinguishingMarks { get; set; } // optional
        public string? VehicleDescription { get; set; } // optional
        public string? RelationshipToVictim { get; set; } // optional
        public bool Arrested { get; set; }
        public string? ChargesFiled { get; set; } // only populated if Arrested = true
    }
}
