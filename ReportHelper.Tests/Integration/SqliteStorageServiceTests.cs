using Microsoft.Data.Sqlite;
using ReportHelper.Models;
using ReportHelper.Services;

namespace ReportHelper.Tests.Integration
{
    public class SqliteStorageServiceTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly SqliteStorageService _storageService;

        public SqliteStorageServiceTests() 
        {
            _testDbPath = Path.GetTempFileName() + ".db";
            _storageService = new SqliteStorageService(_testDbPath);
        }

        public void Dispose() 
        {
            SqliteConnection.ClearAllPools();

            if (File.Exists(_testDbPath)) 
            {
                File.Delete(_testDbPath);
            }
        }

        [Fact]
        public void InitializeDatabase_CreatesTables()
        {
            // Arrange + Act: database is already initialised in the constructor.

            using var connection = new SqliteConnection($"Data Source={_testDbPath}");
            connection.Open();

            var tableNames = new[] { "ReportRecords", "VictimRecords", "SuspectRecords" };

            foreach (var tableName in tableNames)
            {
                using var command = new SqliteCommand(
                    "SELECT name FROM sqlite_master WHERE type = 'table' AND name = @tableName", connection);
                command.Parameters.AddWithValue("@tableName", tableName);

                var result = command.ExecuteScalar() as string;

                //Assert
                Assert.Equal(tableName, result);
            }
        }

        [Fact]
        public void SaveReportRecord_SavesAndGetsAllReports()
        {
            //Arrange
            var report = new ReportRecord
            {
                ReportNumber = "12345",
                IncidentClassification = "Theft",
                ReportDate = DateTime.Now,
                IncidentDate = DateTime.Now.AddHours(-1),
                OfficerName = "Officer Smith",
                BadgeNumber = "6789",
                NarrativeText = "Suspect stole a bicycle.",
                IsComplete = true,
                CreatedAt = DateTime.Now
            };

            //Act
            _storageService.SaveReport(report);
            var retrievedReports = _storageService.GetAllReports();

            //Assert
            Assert.NotNull(retrievedReports);
            Assert.Equal(report.ReportNumber, retrievedReports[0].ReportNumber);
            Assert.Equal(report.IncidentClassification, retrievedReports[0].IncidentClassification);
            Assert.Equal(report.OfficerName, retrievedReports[0].OfficerName);
        }

        // BL-09 / S1.1 — GetNextReportNumber

        [Fact]
        public void GetNextReportNumber_NoReportsForDate_ReturnsSequenceOne()
        {
            //Arrange
            var today = new DateTime(2026, 6, 23);

            //Act
            var reportNumber = _storageService.GetNextReportNumber(today);

            //Assert
            Assert.Equal("20260623-0001", reportNumber);
        }

        [Fact]
        public void GetNextReportNumber_OneReportAlreadyExistsForDate_ReturnsSequenceTwo()
        {
            //Arrange
            var today = new DateTime(2026, 6, 23);
            _storageService.SaveReport(MakeMinimalReport(reportDate: today));

            //Act
            var reportNumber = _storageService.GetNextReportNumber(today);

            //Assert
            Assert.Equal("20260623-0002", reportNumber);
        }

        [Fact]
        public void GetNextReportNumber_ReportExistsOnDifferentDate_DoesNotAffectTodaysSequence()
        {
            //Arrange — a report saved "yesterday" must not count toward "today"'s sequence.
            var yesterday = new DateTime(2026, 6, 22);
            var today = new DateTime(2026, 6, 23);
            _storageService.SaveReport(MakeMinimalReport(reportDate: yesterday));

            //Act
            var reportNumber = _storageService.GetNextReportNumber(today);

            //Assert — still the first report of today, despite yesterday's report existing.
            Assert.Equal("20260623-0001", reportNumber);
        }

        // Helper: builds a minimally valid ReportRecord for tests that only care about
        // ReportDate/sequencing behaviour, not the full field set.
        private static ReportRecord MakeMinimalReport(DateTime reportDate) => new ReportRecord
        {
            ReportNumber = $"{reportDate:yyyyMMdd}-EXISTING",
            IncidentClassification = "Theft",
            ReportDate = reportDate,
            IncidentDate = reportDate,
            OfficerName = "Officer Smith",
            BadgeNumber = "6789",
            NarrativeText = "Placeholder narrative.",
            IsComplete = true,
            CreatedAt = reportDate
        };
    }
}
