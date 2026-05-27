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
    }
}
