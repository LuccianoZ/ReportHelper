using CommunityToolkit.Mvvm.ComponentModel;
using ReportHelper.Services;
using ReportHelper.ViewModels.Base;

namespace ReportHelper.ViewModels.Sections
{
    // S1 — Report Header. BL-09 scope: only the auto-generated Report Number (S1.1).
    // The remaining 11 S1 fields (officer name, badge, dates/times, classification, etc.)
    // are BL-10's job — they are intentionally not modeled here yet.
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

        public ReportHeaderViewModel(IStorageService storageService, IVoiceInputService? voiceInputService = null)
            : base(voiceInputService)
        {
            SectionTitle = "Report Header";

            // Generated once, immediately, when the officer starts a new report —
            // this is what makes SC-03 true ("when the Report Header section loads,
            // the Report Number field is populated automatically").
            ReportNumber = storageService.GetNextReportNumber(DateTime.Today);
        }
    }
}
