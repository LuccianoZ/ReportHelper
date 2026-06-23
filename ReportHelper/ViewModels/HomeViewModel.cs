using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ReportHelper.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        // Callback provided by MainViewModel — calling it changes the current screen.
        private readonly Action<ObservableObject> _navigate;

        public HomeViewModel(Action<ObservableObject> navigate)
        {
            _navigate = navigate;
        }

        // SC-02: Navigates to the section shell, which starts at S1 (Report Header).
        [RelayCommand]
        private void StartNewReport()
        {
            _navigate(new SectionShellViewModel(_navigate));
        }

        // Placeholder — report history view is Sprint 2 (BL-28).
        [RelayCommand]
        private void ViewPastReports() { }
    }
}
