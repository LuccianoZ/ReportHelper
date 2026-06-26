using CommunityToolkit.Mvvm.ComponentModel;
using ReportHelper.Services;
using ReportHelper.ViewModels.Base;

namespace ReportHelper.ViewModels.Sections
{
    // S1 — Report Header. BL-09 delivered the auto-generated Report Number (S1.1).
    // BL-10 adds the remaining 11 S1 fields per A-03 §3 (officer identity, dates/times,
    // classification, status). Validation (RequiredFields wiring, blocking Confirm)
    // is explicitly BL-11's job, not this one — these properties just need to exist,
    // be correctly typed, and default sensibly.
    public partial class ReportHeaderViewModel : SectionViewModelBase
    {
        // S1.1 — Report Number. Auto-generated, intended to be read-only per SC-03:
        // the officer never types this. Note: a true compiler-enforced private setter
        // (via CommunityToolkit.Mvvm's partial-property syntax) requires C# 13, which
        // requires .NET 9 — not available on this project's net8.0-windows target.
        // So this follows the same field-based [ObservableProperty] pattern as every
        // other property in this codebase (SectionTitle, ErrorMessage, etc.), which
        // generates a public setter. "Read-only" here is a contract enforced by
        // convention — only this class's constructor ever sets it — not by the compiler.
        [ObservableProperty]
        private string _reportNumber = string.Empty;

        // ── S1.2 / S1.3 — Report Date & Time ────────────────────────
        // Nullable by design: this ViewModel models a field that STARTS unset.
        // ReportRecord.ReportDate/ReportTime are non-nullable because THAT type
        // represents a complete, saved report — the unwrap from `DateTime?` to
        // `DateTime` happens at the mapping step, after BL-11 validation confirms
        // the value is non-null. Bind these directly to a WPF DatePicker /
        // time-input control's SelectedDate / equivalent (both already accept
        // nullable values natively, so no converter is needed).
        [ObservableProperty]
        private DateTime? _reportDate;

        [ObservableProperty]
        private TimeSpan? _reportTime;

        // ── S1.4 / S1.5 / S1.6 — Officer identity (voice + manual text) ─────
        // Plain string, defaulting to empty (never null) so XAML TextBox bindings
        // never throw on a null Text value — matches SectionTitle/ErrorMessage
        // elsewhere in SectionViewModelBase.
        [ObservableProperty]
        private string _officerName = string.Empty;

        [ObservableProperty]
        private string _badgeNumber = string.Empty;

        [ObservableProperty]
        private string _unitDivision = string.Empty;

        // ── S1.7 — Incident Classification (Select) ───────────────
        // The selected value. Empty string = nothing chosen yet, which is how
        // BL-11 will know this required Select field still blocks Confirm.
        [ObservableProperty]
        private string _incidentClassification = string.Empty;

        // The dropdown's option list. Exposed as a property (not a static/const)
        // so it's bindable to a WPF ComboBox's ItemsSource in XAML. It never
        // changes after construction, but CommunityToolkit.Mvvm's [ObservableProperty]
        // isn't needed here since nothing ever re-assigns it — a plain get-only
        // auto-property is the right tool, not a notifying one.
        public string[] IncidentClassificationOptions { get; } =
        {
            "Assault", "Theft", "Burglary", "Robbery", "Vandalism",
            "Domestic Disturbance", "Traffic Incident", "Disorderly Conduct",
            "Drug Offense", "Fraud", "Missing Person", "Welfare Check", "Other"
        };

        // ── S1.8 / S1.9 — Incident Date & Time (range) ──────────────
        [ObservableProperty]
        private DateTime? _incidentDate;

        // S1.9 start — required. "What time did the incident occur?"
        [ObservableProperty]
        private TimeSpan? _incidentTimeStart;

        // S1.9 end — optional. Stays null for a single-point-in-time incident;
        // only populated when the officer states a range. Mirrors
        // ReportRecord.IncidentTimeEnd, which is nullable for the same reason
        // (and is ALREADY nullable there even in the saved-record sense, since
        // this field is optional on a complete report too, unlike the other
        // Date/Time fields here).
        [ObservableProperty]
        private TimeSpan? _incidentTimeEnd;

        // ── S1.10 — Dispatch Time (optional) ──────────────────
        // Optional on BOTH the ViewModel and ReportRecord — "does not block
        // advance" per A-03/BL-11. Still nullable here for the same reason as
        // every other Date/Time field: starts unset.
        [ObservableProperty]
        private TimeSpan? _dispatchTime;

        // ── S1.11 — Arrival Time (required) ──────────────────
        [ObservableProperty]
        private TimeSpan? _arrivalTime;

        // ── S1.12 — Report Status (Select) ──────────────────
        [ObservableProperty]
        private string _reportStatus = string.Empty;

        // Per A-03's own voice prompt for S1.12: "Is this an initial report or
        // a supplemental report?" — only two valid values, both named explicitly
        // in the source artifact, so no judgment call was needed on this list.
        public string[] ReportStatusOptions { get; } = { "Initial", "Supplemental" };

        public ReportHeaderViewModel(IStorageService storageService, IVoiceInputService? voiceInputService = null)
            : base(voiceInputService)
        {
            SectionTitle = "Report Header";

            // Generated once, immediately, when the officer starts a new report —
            // this is what makes SC-03 true ("when the Report Header section loads,
            // the Report Number field is populated automatically").
            ReportNumber = storageService.GetNextReportNumber(DateTime.Today);
        }

        // ── BL-10 dictation routing ─────────────────────────────────────────
        //
        // Overrides SectionViewModelBase.OnDictationCompleted (a normal
        // protected virtual method, NOT a generator partial-method hook —
        // see SectionViewModelBase.cs for why a generator hook can't live
        // directly on this subclass). Called once per completed dictation,
        // with fieldName carrying whichever property name StartRecording(fieldName)
        // was given when the officer pressed a specific field's Dictate button.
        protected override void OnDictationCompleted(string fieldName, string value)
        {
            switch (fieldName)
            {
                case nameof(OfficerName):
                    OfficerName = value;
                    break;
                case nameof(BadgeNumber):
                    BadgeNumber = value;
                    break;
                case nameof(UnitDivision):
                    UnitDivision = value;
                    break;
                // No default case: if fieldName is empty or names a field this
                // section doesn't have a voice-enabled case for yet, the
                // transcription is silently dropped rather than throwing. This
                // matches DictatedText_WhenSetWithNoActiveDictationField_
                // DoesNotThrowOrRouteAnywhere's expectation — a missing route is
                // a no-op, not a crash.
            }

            // Clear immediately after routing so a stale value can never be
            // re-applied to the wrong field if ActiveDictationField changes
            // again before the next real dictation completes.
            DictatedText = string.Empty;
        }
    }
}
