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
                    IncidentClassification TEXT NOT NULL,
                    ReportDate TEXT NOT NULL,
                    IncidentDate TEXT NOT NULL,
                    OfficerName TEXT NOT NULL,
                    BadgeNumber TEXT NOT NULL,
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
                    (ReportNumber, IncidentClassification, ReportDate, IncidentDate,
                     OfficerName, BadgeNumber, NarrativeText, IsComplete, CreatedAt, CompletedAt)
                VALUES
                    (@ReportNumber, @IncidentClassification, @ReportDate, @IncidentDate,
                     @OfficerName, @BadgeNumber, @NarrativeText, @IsComplete, @CreatedAt, @CompletedAt)",
                connection);

            command.Parameters.AddWithValue("@ReportNumber", report.ReportNumber);
            command.Parameters.AddWithValue("@IncidentClassification", report.IncidentClassification);
            command.Parameters.AddWithValue("@ReportDate", report.ReportDate.ToString("o"));
            command.Parameters.AddWithValue("@IncidentDate", report.IncidentDate.ToString("o"));
            command.Parameters.AddWithValue("@OfficerName", report.OfficerName);
            command.Parameters.AddWithValue("@BadgeNumber", report.BadgeNumber);
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
                SELECT Id, ReportNumber, IncidentClassification, ReportDate, IncidentDate,
                       OfficerName, BadgeNumber, NarrativeText, IsComplete, CreatedAt, CompletedAt
                FROM ReportRecords
                WHERE IsComplete = 1
                ORDER BY CreatedAt DESC",
                connection);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var record = new ReportRecord
                {
                    Id = reader.GetInt32(0),
                    ReportNumber = reader.GetString(1),
                    IncidentClassification = reader.GetString(2),
                    ReportDate = DateTime.Parse(reader.GetString(3)),
                    IncidentDate = DateTime.Parse(reader.GetString(4)),
                    OfficerName = reader.GetString(5),
                    BadgeNumber = reader.GetString(6),
                    NarrativeText = reader.GetString(7),
                    IsComplete = reader.GetInt32(8) == 1,
                    CreatedAt = DateTime.Parse(reader.GetString(9)),
                    CompletedAt = reader.IsDBNull(10) ? null : DateTime.Parse(reader.GetString(10))
                };

                results.Add(record);
            }

            return results;
        }
    }
}

    
