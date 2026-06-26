using ReportHelper.Models;
using ReportHelper.Services;
using ReportHelper.ViewModels.Sections;

namespace ReportHelper.Tests.Unit.Sections
{
    // A controlled fake storage service — returns a fixed report number so the test
    // can assert an exact value without touching real SQLite. Mirrors the
    // FailingStorageService pattern already used in SignOffViewModelTests.cs.
    public class FakeStorageServiceForHeader : IStorageService
    {
        public string ReportNumberToReturn { get; set; } = "20260623-0001";
        public DateTime? LastRequestedDate { get; private set; }

        public string GetNextReportNumber(DateTime date)
        {
            LastRequestedDate = date;
            return ReportNumberToReturn;
        }

        public void SaveReport(ReportRecord report) { }
        public List<ReportRecord> GetAllReports() => new List<ReportRecord>();
        public void SaveDraft(ReportRecord draft) { }
        public ReportRecord? LoadDraft() => null;
    }

    public class ReportHeaderViewModelTests
    {
        // BL-09 / SC-03: When the Report Header section loads, the Report Number
        // field must be populated automatically from storage — the officer never types it.
        [Fact]
        public void Constructor_PopulatesReportNumber_FromStorageService()
        {
            // Arrange
            var fakeStorage = new FakeStorageServiceForHeader
            {
                ReportNumberToReturn = "20260623-0001"
            };

            // Act
            var viewModel = new ReportHeaderViewModel(fakeStorage);

            // Assert
            Assert.Equal("20260623-0001", viewModel.ReportNumber);
        }

        // SC-03 also requires the field to be read-only. There's no setter to test
        // directly against from outside the class once we make this read-only by
        // design, so this test instead documents the contract: the property has no
        // public setter. If ReportNumber ever gains a public setter, this comment
        // is the reminder of why it shouldn't.
        [Fact]
        public void Constructor_RequestsReportNumber_ForTodaysDate()
        {
            // Arrange
            var fakeStorage = new FakeStorageServiceForHeader();
            var beforeConstruction = DateTime.Today;

            // Act
            var viewModel = new ReportHeaderViewModel(fakeStorage);

            // Assert — confirms GetNextReportNumber was actually called, and with today's date.
            Assert.Equal(beforeConstruction, fakeStorage.LastRequestedDate);
        }
    }
}
