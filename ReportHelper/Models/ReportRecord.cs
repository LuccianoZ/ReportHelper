namespace ReportHelper.Models
{
    public class ReportRecord
    {
        public int Id { get; set; } //primary key

        // ── SECTION 1 — REPORT HEADER (A-03) ──────────────────────────────
        public string ReportNumber { get; set; } = string.Empty; // S1.1   auto-generated, e.g. "20260525-0001"
        public DateTime ReportDate { get; set; } // S1.2          date the report is being written
        public TimeSpan ReportTime { get; set; } // S1.3          time the report is being written
        public string OfficerName { get; set; } = string.Empty; // S1.4
        public string BadgeNumber { get; set; } = string.Empty; // S1.5
        public string UnitDivision { get; set; } = string.Empty; // S1.6   unit / division assignment
        public string IncidentClassification { get; set; } = string.Empty; // S1.7   e.g. "Theft", "Assault", etc.
        public DateTime IncidentDate { get; set; } // S1.8         date the incident occurred

        // S1.9 — "What time did the incident occur? If a range, state both start and end."
        // Modeled as a real range: End is optional and stays null for single-point incidents.
        public TimeSpan IncidentTimeStart { get; set; } // S1.9
        public TimeSpan? IncidentTimeEnd { get; set; } // S1.9   null if not a range

        public TimeSpan? DispatchTime { get; set; } // S1.10  optional — does not block advance
        public TimeSpan ArrivalTime { get; set; } // S1.11        time officer arrived on scene
        public string ReportStatus { get; set; } = string.Empty; // S1.12  "Initial" or "Supplemental"

        // ── SECTION 2 — INCIDENT LOCATION (A-03) ──────────────────────────
        public string StreetAddress { get; set; } = string.Empty; // S2.1
        public string CityStateZip { get; set; } = string.Empty; // S2.2
        public string LocationType { get; set; } = string.Empty; // S2.3   residence, business, street, etc.
        public string? LocationDetail { get; set; } // S2.4  optional — apt/floor/room/landmark
        public string? CrossStreets { get; set; } // S2.5    optional
        public string? JurisdictionBeat { get; set; } // S2.6  optional

        // ── SECTION 3 — VICTIM / COMPLAINANT (A-03) ───────────────────────
        // Repeating section. VictimRecord already models all 8 S3 fields correctly;
        // this collection is the link between a report and its victim entries.
        // Note: SqliteStorageService does not yet read/write this collection —
        // that wiring lands in BL-15 alongside the S3 screen itself.
        public List<VictimRecord> Victims { get; set; } = new();

        // ── SECTION 7 — NARRATIVE (A-03) ──────────────────────────────────
        public string NarrativeText { get; set; } = string.Empty; // S7.1   the full narrative

        // ── RECORD METADATA ────────────────────────────────────────────────
        public bool IsComplete { get; set; } // false = draft, true = finalized report
        public DateTime CreatedAt { get; set; } // when the record was first created
        public DateTime? CompletedAt { get; set; } // null until sign off
    }
}
