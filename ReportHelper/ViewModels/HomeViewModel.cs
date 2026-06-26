using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportHelper.Services;

namespace ReportHelper.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;

        public HomeViewModel(INavigationService navigation)
        {
            _navigation = navigation;
        }

        // SC-02: Navigates to the section shell, which starts at S1 (Report Header).
        [RelayCommand]
        private void StartNewReport()
        {
            _navigation.NavigateTo<SectionShellViewModel>();
        }

        // Placeholder — report history view is Sprint 2 (BL-28).
        [RelayCommand]
        private void ViewPastReports() { }
    }
}
