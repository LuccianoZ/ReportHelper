using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ReportHelper.ViewModels.Sections;

namespace ReportHelper.Views.Sections
{
    public partial class ReportHeaderView : UserControl
    {
        public ReportHeaderView()
        {
            InitializeComponent();
        }

        // ── BL-10 — push-to-talk wiring for the 3 voice-enabled fields ──────
        //
        // Mirrors the press/release pattern SectionShellView.xaml.cs already
        // uses for its hold-to-cancel button: WPF's Button.Click only fires on
        // a full click, not on press-and-hold, so push-to-talk needs the lower
        // level PreviewMouseLeftButtonDown / PreviewMouseLeftButtonUp events
        // instead. Each field has its own MouseDown handler (so the correct
        // field name reaches StartRecording), but all three share one MouseUp
        // handler — releasing always means "stop and transcribe," regardless of
        // which field's button was held, so there's nothing field-specific left
        // to do once recording stops.
        //
        // CaptureMouse() (on down) / ReleaseMouseCapture() (on up) ensures the
        // officer still gets a MouseUp event even if their cursor drifts off
        // the button before they release — without this, a recording could get
        // stuck "on" if the press-release happened slightly off-target.

        private void OfficerNameDictate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).CaptureMouse();
            if (DataContext is ReportHeaderViewModel vm)
                vm.StartRecording(nameof(ReportHeaderViewModel.OfficerName));
        }

        private void BadgeNumberDictate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).CaptureMouse();
            if (DataContext is ReportHeaderViewModel vm)
                vm.StartRecording(nameof(ReportHeaderViewModel.BadgeNumber));
        }

        private void UnitDivisionDictate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).CaptureMouse();
            if (DataContext is ReportHeaderViewModel vm)
                vm.StartRecording(nameof(ReportHeaderViewModel.UnitDivision));
        }

        // Shared release handler for all 3 Dictate buttons. async void is the
        // correct (if unusual-looking) signature here: WPF event handlers must
        // return void, but the work inside — StopAndTranscribeAsync — is
        // asynchronous (it awaits the Whisper.NET transcription call).
        private async void Dictate_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ((UIElement)sender).ReleaseMouseCapture();

            if (DataContext is ReportHeaderViewModel vm)
                await vm.StopAndTranscribeAsync();
        }
    }
}
