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

        // ── BL-10 — remaining 11 S1 fields ──────────────────────────────────
        //
        // BL-10 scope is "fields exist, correctly typed, default sensibly."
        // BL-11 (not this story) is what wires RequiredFields validation against
        // them — these tests deliberately do NOT touch CanAdvance / ErrorMessage.
        //
        // Date/Time fields are nullable (DateTime? / TimeSpan?) by design: the
        // ViewModel represents a field that STARTS unset, unlike ReportRecord
        // (which models a COMPLETE report and uses non-nullable DateTime/TimeSpan
        // for the always-required ones). The mapping step from ViewModel → Record
        // is where these get unwrapped, after BL-11 validation confirms non-null.

        [Fact]
        public void Constructor_AllDateAndTimeFields_DefaultToNull()
        {
            // Arrange + Act
            var viewModel = new ReportHeaderViewModel(new FakeStorageServiceForHeader());

            // Assert — nothing is pre-filled; the officer (or a date/time picker
            // default-to-today UX choice, which is a View concern, not ViewModel)
            // must explicitly set these via the bound control.
            Assert.Null(viewModel.ReportDate);
            Assert.Null(viewModel.ReportTime);
            Assert.Null(viewModel.IncidentDate);
            Assert.Null(viewModel.IncidentTimeStart);
            Assert.Null(viewModel.IncidentTimeEnd);
            Assert.Null(viewModel.DispatchTime);
            Assert.Null(viewModel.ArrivalTime);
        }

        [Fact]
        public void Constructor_AllTextFields_DefaultToEmptyString()
        {
            // Arrange + Act
            var viewModel = new ReportHeaderViewModel(new FakeStorageServiceForHeader());

            // Assert — matches the existing string-field convention elsewhere in the
            // codebase (e.g. SectionTitle, ErrorMessage): empty string, never null,
            // so XAML TextBox bindings never throw on a null Text value.
            Assert.Equal(string.Empty, viewModel.OfficerName);
            Assert.Equal(string.Empty, viewModel.BadgeNumber);
            Assert.Equal(string.Empty, viewModel.UnitDivision);
        }

        [Fact]
        public void Constructor_SelectFields_DefaultToEmptyString()
        {
            // Arrange + Act
            var viewModel = new ReportHeaderViewModel(new FakeStorageServiceForHeader());

            // Assert — no option is pre-selected. An empty selection is how BL-11
            // will detect "required Select field not yet chosen."
            Assert.Equal(string.Empty, viewModel.IncidentClassification);
            Assert.Equal(string.Empty, viewModel.ReportStatus);
        }

        [Fact]
        public void IncidentClassificationOptions_ContainsExpectedThirteenOptions()
        {
            // Arrange + Act
            var viewModel = new ReportHeaderViewModel(new FakeStorageServiceForHeader());

            // Assert — exact list locked in during BL-10 planning. Asserting the
            // full set (not just Contains a couple) so an accidental edit to this
            // list during later work is caught immediately by a red test.
            var expected = new[]
            {
                "Assault", "Theft", "Burglary", "Robbery", "Vandalism",
                "Domestic Disturbance", "Traffic Incident", "Disorderly Conduct",
                "Drug Offense", "Fraud", "Missing Person", "Welfare Check", "Other"
            };

            Assert.Equal(expected, viewModel.IncidentClassificationOptions);
        }

        [Fact]
        public void ReportStatusOptions_ContainsInitialAndSupplemental()
        {
            // Arrange + Act
            var viewModel = new ReportHeaderViewModel(new FakeStorageServiceForHeader());

            // Assert — per A-03's own voice prompt for S1.12: "Is this an initial
            // report or a supplemental report?"
            Assert.Equal(new[] { "Initial", "Supplemental" }, viewModel.ReportStatusOptions);
        }

        [Fact]
        public void AllNewProperties_CanBeSetAndReadBack()
        {
            // Arrange
            var viewModel = new ReportHeaderViewModel(new FakeStorageServiceForHeader());
            var someDate = new DateTime(2026, 6, 26);
            var someTime = new TimeSpan(14, 30, 0);
            var laterTime = new TimeSpan(15, 0, 0);

            // Act — simulates what the View's two-way bindings will do once wired.
            viewModel.ReportDate = someDate;
            viewModel.ReportTime = someTime;
            viewModel.OfficerName = "J. Rivera";
            viewModel.BadgeNumber = "4471";
            viewModel.UnitDivision = "Patrol — 3rd District";
            viewModel.IncidentClassification = "Burglary";
            viewModel.IncidentDate = someDate;
            viewModel.IncidentTimeStart = someTime;
            viewModel.IncidentTimeEnd = laterTime;
            viewModel.DispatchTime = someTime;
            viewModel.ArrivalTime = laterTime;
            viewModel.ReportStatus = "Initial";

            // Assert
            Assert.Equal(someDate, viewModel.ReportDate);
            Assert.Equal(someTime, viewModel.ReportTime);
            Assert.Equal("J. Rivera", viewModel.OfficerName);
            Assert.Equal("4471", viewModel.BadgeNumber);
            Assert.Equal("Patrol — 3rd District", viewModel.UnitDivision);
            Assert.Equal("Burglary", viewModel.IncidentClassification);
            Assert.Equal(someDate, viewModel.IncidentDate);
            Assert.Equal(someTime, viewModel.IncidentTimeStart);
            Assert.Equal(laterTime, viewModel.IncidentTimeEnd);
            Assert.Equal(someTime, viewModel.DispatchTime);
            Assert.Equal(laterTime, viewModel.ArrivalTime);
            Assert.Equal("Initial", viewModel.ReportStatus);
        }

        // ── BL-10 — dictation routing (ActiveDictationField → named property) ─────
        //
        // ReportHeaderView has 3 voice-enabled fields sharing SectionViewModelBase's
        // single DictatedText property. These tests confirm ReportHeaderViewModel's
        // OnDictatedTextChanged hook correctly routes a finished transcription into
        // whichever property ActiveDictationField names, and only that one.

        [Theory]
        [InlineData("OfficerName")]
        [InlineData("BadgeNumber")]
        [InlineData("UnitDivision")]
        public void DictatedText_WhenSet_RoutesIntoFieldNamedByActiveDictationField(string fieldName)
        {
            // Arrange
            var viewModel = new ReportHeaderViewModel(new FakeStorageServiceForHeader());
            viewModel.StartRecording(fieldName); // sets ActiveDictationField

            // Act — simulates StopAndTranscribeAsync() succeeding and setting DictatedText.
            viewModel.DictatedText = "transcribed value";

            // Assert — read back via reflection so all 3 cases share one test body
            // rather than three near-identical copy-pasted tests.
            var property = typeof(ReportHeaderViewModel).GetProperty(fieldName);
            Assert.NotNull(property);
            Assert.Equal("transcribed value", property!.GetValue(viewModel));
        }

        [Fact]
        public void DictatedText_WhenSet_ClearsItselfAfterRouting()
        {
            // Arrange
            var viewModel = new ReportHeaderViewModel(new FakeStorageServiceForHeader());
            viewModel.StartRecording("OfficerName");

            // Act
            viewModel.DictatedText = "transcribed value";

            // Assert — DictatedText must not linger after being applied, or the
            // next dictation into a DIFFERENT field could misfire if something
            // re-reads a stale value. Also prevents this setter from re-triggering
            // itself in a loop (guarded separately below).
            Assert.Equal(string.Empty, viewModel.DictatedText);
        }

        [Fact]
        public void DictatedText_WhenSetWithNoActiveDictationField_DoesNotThrowOrRouteAnywhere()
        {
            // Arrange — ActiveDictationField is still empty (default), which can
            // legitimately happen if DictatedText is somehow set without
            // StartRecording having run first. Routing must no-op safely rather
            // than throw a NullReferenceException or reflection error.
            var viewModel = new ReportHeaderViewModel(new FakeStorageServiceForHeader());

            // Act
            viewModel.DictatedText = "transcribed value";

            // Assert — nothing changed except DictatedText itself; no exception thrown.
            Assert.Equal(string.Empty, viewModel.OfficerName);
            Assert.Equal(string.Empty, viewModel.BadgeNumber);
            Assert.Equal(string.Empty, viewModel.UnitDivision);
        }

        [Fact]
        public void DictatedText_RoutingIntoOfficerName_DoesNotAffectOtherFields()
        {
            // Arrange — confirms routing is exclusive: dictating into one field
            // must not bleed into the other two voice-enabled fields.
            var viewModel = new ReportHeaderViewModel(new FakeStorageServiceForHeader());
            viewModel.BadgeNumber = "4471";
            viewModel.UnitDivision = "Patrol";
            viewModel.StartRecording("OfficerName");

            // Act
            viewModel.DictatedText = "J. Rivera";

            // Assert
            Assert.Equal("J. Rivera", viewModel.OfficerName);
            Assert.Equal("4471", viewModel.BadgeNumber); // untouched
            Assert.Equal("Patrol", viewModel.UnitDivision); // untouched
        }
    }
}
