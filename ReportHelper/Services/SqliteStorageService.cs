using Microsoft.Data.Sqlite;
using ReportHelper.Models;

namespace ReportHelper.Services
{
    public class SqliteStorageService : IStorageService
    {
        private readonly string _dbPath;

        public SqliteStorageService(string dbPath)
        {
            _dbPath = dbPath;
            InitializeDatabase();
        }

        public void InitializeDatabase() 
        {
            var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS ReportRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ReportNumber TEXT NOT NULL,
                    ReportDate TEXT NOT NULL,
                    ReportTime TEXT NOT NULL,
                    OfficerName TEXT NOT NULL,
                    BadgeNumber TEXT NOT NULL,
                    UnitDivision TEXT NOT NULL,
                    IncidentClassification TEXT NOT NULL,
                    IncidentDate TEXT NOT NULL,
                    IncidentTimeStart TEXT NOT NULL,
                    IncidentTimeEnd TEXT,
                    DispatchTime TEXT,
                    ArrivalTime TEXT NOT NULL,
                    ReportStatus TEXT NOT NULL,
                    StreetAddress TEXT NOT NULL,
                    CityStateZip TEXT NOT NULL,
                    LocationType TEXT NOT NULL,
                    LocationDetail TEXT,
                    CrossStreets TEXT,
                    JurisdictionBeat TEXT,
                    NarrativeText TEXT NOT NULL,
                    IsComplete INTEGER NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    CompletedAt TEXT
                );
                
                CREATE TABLE IF NOT EXISTS VictimRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ReportId INTEGER NOT NULL,
                    FullName TEXT NOT NULL,
                    DateOfBirth TEXT NOT NULL,
                    HomeAddress TEXT NOT NULL,
                    PhoneNumber TEXT NOT NULL,
                    EmailAddress TEXT,
                    RelationshipToSuspect TEXT,
                    InjuriesSustained TEXT,
                    MedicalTreatment TEXT,
                    FOREIGN KEY (ReportId) REFERENCES ReportRecords(Id)
                );

                CREATE TABLE IF NOT EXISTS SuspectRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ReportId INTEGER NOT NULL,
                    FullName TEXT NOT NULL,
                    RaceSex TEXT NOT NULL,
                    HeightWeight TEXT NOT NULL,
                    HairEyeColor TEXT NOT NULL,
                    ClothingDescription TEXT NOT NULL,
                    DateOfBirth TEXT,
                    Address TEXT,
                    DistinguishingMarks TEXT,
                    VehicleDescription TEXT,
                    RelationshipToVictim TEXT,
                    Arrested INTEGER NOT NULL,
                    ChargesFiled TEXT,
                    FOREIGN KEY (ReportId) REFERENCES ReportRecords(Id)
                );
            ";

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();
            using var command = new SqliteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }
        
        public void SaveDraft(ReportRecord value) 
        { 
            throw new NotImplementedException(); 
        }

        public ReportRecord? LoadDraft()
        {
            throw new NotImplementedException();
        }
        
        public void SaveReport(ReportRecord report)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            using var command = new SqliteCommand(@"
                INSERT INTO ReportRecords
                    (ReportNumber, ReportDate, ReportTime, OfficerName, BadgeNumber, UnitDivision,
                     IncidentClassification, IncidentDate, IncidentTimeStart, IncidentTimeEnd, DispatchTime,
                     ArrivalTime, ReportStatus, StreetAddress, CityStateZip, LocationType, LocationDetail,
                     CrossStreets, JurisdictionBeat, NarrativeText, IsComplete, CreatedAt, CompletedAt)
                VALUES
                    (@ReportNumber, @ReportDate, @ReportTime, @OfficerName, @BadgeNumber, @UnitDivision,
                     @IncidentClassification, @IncidentDate, @IncidentTimeStart, @IncidentTimeEnd, @DispatchTime,
                     @ArrivalTime, @ReportStatus, @StreetAddress, @CityStateZip, @LocationType, @LocationDetail,
                     @CrossStreets, @JurisdictionBeat, @NarrativeText, @IsComplete, @CreatedAt, @CompletedAt)",
                connection);

            command.Parameters.AddWithValue("@ReportNumber", report.ReportNumber);
            command.Parameters.AddWithValue("@ReportDate", report.ReportDate.ToString("o"));
            command.Parameters.AddWithValue("@ReportTime", report.ReportTime.ToString("c"));
            command.Parameters.AddWithValue("@OfficerName", report.OfficerName);
            command.Parameters.AddWithValue("@BadgeNumber", report.BadgeNumber);
            command.Parameters.AddWithValue("@UnitDivision", report.UnitDivision);
            command.Parameters.AddWithValue("@IncidentClassification", report.IncidentClassification);
            command.Parameters.AddWithValue("@IncidentDate", report.IncidentDate.ToString("o"));
            command.Parameters.AddWithValue("@IncidentTimeStart", report.IncidentTimeStart.ToString("c"));
            command.Parameters.AddWithValue("@IncidentTimeEnd", report.IncidentTimeEnd.HasValue ? (object)report.IncidentTimeEnd.Value.ToString("c") : DBNull.Value);
            command.Parameters.AddWithValue("@DispatchTime", report.DispatchTime.HasValue ? (object)report.DispatchTime.Value.ToString("c") : DBNull.Value);
            command.Parameters.AddWithValue("@ArrivalTime", report.ArrivalTime.ToString("c"));
            command.Parameters.AddWithValue("@ReportStatus", report.ReportStatus);
            command.Parameters.AddWithValue("@StreetAddress", report.StreetAddress);
            command.Parameters.AddWithValue("@CityStateZip", report.CityStateZip);
            command.Parameters.AddWithValue("@LocationType", report.LocationType);
            command.Parameters.AddWithValue("@LocationDetail", (object?)report.LocationDetail ?? DBNull.Value);
            command.Parameters.AddWithValue("@CrossStreets", (object?)report.CrossStreets ?? DBNull.Value);
            command.Parameters.AddWithValue("@JurisdictionBeat", (object?)report.JurisdictionBeat ?? DBNull.Value);
            command.Parameters.AddWithValue("@NarrativeText", report.NarrativeText);
            command.Parameters.AddWithValue("@IsComplete", report.IsComplete? 1 : 0);
            command.Parameters.AddWithValue("@CreatedAt", report.CreatedAt.ToString("o"));
            command.Parameters.AddWithValue("@CompletedAt", report.CompletedAt.HasValue? (object)report.CompletedAt.Value.ToString("o") : DBNull.Value);

            command.ExecuteNonQuery();
        }

        public List<ReportRecord> GetAllReports()
        {
            var results = new List<ReportRecord>();

            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            using var command = new SqliteCommand(@"
                SELECT Id, ReportNumber, ReportDate, ReportTime, OfficerName, BadgeNumber, UnitDivision,
                       IncidentClassification, IncidentDate, IncidentTimeStart, IncidentTimeEnd, DispatchTime,
                       ArrivalTime, ReportStatus, StreetAddress, CityStateZip, LocationType, LocationDetail,
                       CrossStreets, JurisdictionBeat, NarrativeText, IsComplete, CreatedAt, CompletedAt
                FROM ReportRecords
                WHERE IsComplete = 1
                ORDER BY CreatedAt DESC",
                connection);

            using var reader = command.ExecuteReader();

            // GetOrdinal looks up a column by name rather than by position. With 24 columns,
            // positional indices (GetString(5), GetString(6)...) are easy to miscount when the
            // SELECT list changes — a silent off-by-one would read the wrong field into the wrong
            // property without throwing an error. GetOrdinal trades a little verbosity for safety.
            int ordId = reader.GetOrdinal("Id");
            int ordReportNumber = reader.GetOrdinal("ReportNumber");
            int ordReportDate = reader.GetOrdinal("ReportDate");
            int ordReportTime = reader.GetOrdinal("ReportTime");
            int ordOfficerName = reader.GetOrdinal("OfficerName");
            int ordBadgeNumber = reader.GetOrdinal("BadgeNumber");
            int ordUnitDivision = reader.GetOrdinal("UnitDivision");
            int ordIncidentClassification = reader.GetOrdinal("IncidentClassification");
            int ordIncidentDate = reader.GetOrdinal("IncidentDate");
            int ordIncidentTimeStart = reader.GetOrdinal("IncidentTimeStart");
            int ordIncidentTimeEnd = reader.GetOrdinal("IncidentTimeEnd");
            int ordDispatchTime = reader.GetOrdinal("DispatchTime");
            int ordArrivalTime = reader.GetOrdinal("ArrivalTime");
            int ordReportStatus = reader.GetOrdinal("ReportStatus");
            int ordStreetAddress = reader.GetOrdinal("StreetAddress");
            int ordCityStateZip = reader.GetOrdinal("CityStateZip");
            int ordLocationType = reader.GetOrdinal("LocationType");
            int ordLocationDetail = reader.GetOrdinal("LocationDetail");
            int ordCrossStreets = reader.GetOrdinal("CrossStreets");
            int ordJurisdictionBeat = reader.GetOrdinal("JurisdictionBeat");
            int ordNarrativeText = reader.GetOrdinal("NarrativeText");
            int ordIsComplete = reader.GetOrdinal("IsComplete");
            int ordCreatedAt = reader.GetOrdinal("CreatedAt");
            int ordCompletedAt = reader.GetOrdinal("CompletedAt");

            while (reader.Read())
            {
                var record = new ReportRecord
                {
                    Id = reader.GetInt32(ordId),
                    ReportNumber = reader.GetString(ordReportNumber),
                    ReportDate = DateTime.Parse(reader.GetString(ordReportDate)),
                    ReportTime = TimeSpan.Parse(reader.GetString(ordReportTime)),
                    OfficerName = reader.GetString(ordOfficerName),
                    BadgeNumber = reader.GetString(ordBadgeNumber),
                    UnitDivision = reader.GetString(ordUnitDivision),
                    IncidentClassification = reader.GetString(ordIncidentClassification),
                    IncidentDate = DateTime.Parse(reader.GetString(ordIncidentDate)),
                    IncidentTimeStart = TimeSpan.Parse(reader.GetString(ordIncidentTimeStart)),
                    IncidentTimeEnd = reader.IsDBNull(ordIncidentTimeEnd) ? null : TimeSpan.Parse(reader.GetString(ordIncidentTimeEnd)),
                    DispatchTime = reader.IsDBNull(ordDispatchTime) ? null : TimeSpan.Parse(reader.GetString(ordDispatchTime)),
                    ArrivalTime = TimeSpan.Parse(reader.GetString(ordArrivalTime)),
                    ReportStatus = reader.GetString(ordReportStatus),
                    StreetAddress = reader.GetString(ordStreetAddress),
                    CityStateZip = reader.GetString(ordCityStateZip),
                    LocationType = reader.GetString(ordLocationType),
                    LocationDetail = reader.IsDBNull(ordLocationDetail) ? null : reader.GetString(ordLocationDetail),
                    CrossStreets = reader.IsDBNull(ordCrossStreets) ? null : reader.GetString(ordCrossStreets),
                    JurisdictionBeat = reader.IsDBNull(ordJurisdictionBeat) ? null : reader.GetString(ordJurisdictionBeat),
                    NarrativeText = reader.GetString(ordNarrativeText),
                    IsComplete = reader.GetInt32(ordIsComplete) == 1,
                    CreatedAt = DateTime.Parse(reader.GetString(ordCreatedAt)),
                    CompletedAt = reader.IsDBNull(ordCompletedAt) ? null : DateTime.Parse(reader.GetString(ordCompletedAt))
                };

                results.Add(record);
            }

            return results;
        }

        // S1.1 — generates the next report number for a given date, e.g. "20260623-0001".
        // Counts how many reports (draft or complete) already exist for that calendar date,
        // then returns the next 4-digit sequence number for that date.
        // SQLite's date() function strips the time portion of ReportDate, so this correctly
        // matches "any report whose ReportDate falls on this calendar day" regardless of the
        // time-of-day component stored alongside it.
        public string GetNextReportNumber(DateTime date)
        {
            using var connection = new SqliteConnection($"Data Source={_dbPath}");
            connection.Open();

            using var command = new SqliteCommand(@"
                SELECT COUNT(*) FROM ReportRecords
                WHERE date(ReportDate) = date(@date)",
                connection);

            command.Parameters.AddWithValue("@date", date.ToString("o"));

            var existingCount = Convert.ToInt32(command.ExecuteScalar());
            var nextSequence = existingCount + 1;

            return $"{date:yyyyMMdd}-{nextSequence:D4}";
        }
    }
}

    
