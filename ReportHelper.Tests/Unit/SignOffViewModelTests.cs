using Microsoft.Data.Sqlite;
using ReportHelper.Models;
using ReportHelper.Services;
using ReportHelper.ViewModels.Sections;

namespace ReportHelper.Tests.Unit
{
    // A fake IStorageService that always throws SqliteException on SaveReport.
    // This is a test stub — it exists only to simulate a storage failure in a
    // controlled way, without needing a real database or a broken disk.
    public class FailingStorageService : IStorageService
    {
        public void SaveReport(ReportRecord report) =>
            throw new SqliteException("Disk full", 13);

        public List<ReportRecord> GetAllReports() => new List<ReportRecord>();
        public void SaveDraft(ReportRecord draft) { }
        public ReportRecord? LoadDraft() => null;
    }

    public class SignOffViewModelTests
    {
        [Fact]
        public void TrySaveReport_WhenSaveFails_SetsErrorStateAndKeepsButtonEnabled()
        {
            //Arrange
            var failingService = new FailingStorageService();
            var report = new ReportRecord
            {
                ReportNumber = "TEST-001",
                IsComplete = true,
                CreatedAt = DateTime.UtcNow
            };
            var viewModel = new SignOffViewModel(failingService, report);

            //Act
            viewModel.TrySaveReport();

            //Assert
            Assert.True(viewModel.SaveFailed);
            Assert.True(viewModel.SaveButtonEnabled);
            Assert.NotNull(viewModel.ErrorMessage);
            Assert.Equal("Report could not be saved. Please check available disk space and try again.", viewModel.ErrorMessage);
        }
    }
}
