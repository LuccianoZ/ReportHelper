using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ReportHelper.ViewModels
{
    public partial class SectionShellViewModel : ObservableObject
    {
        private readonly Action<ObservableObject> _navigate;

        [ObservableProperty]
        private string _sectionTitle = "Report Header";

        [ObservableProperty]
        private int _currentSectionNumber = 1;

        [ObservableProperty]
        private ObservableObject? _currentSectionViewModel;

        // Controls whether the cancel confirmation overlay is shown.
        [ObservableProperty]
        private bool _isCancelPromptVisible = false;

        public int TotalSections => 9;

        public string SectionCounter => $"Section {CurrentSectionNumber} of {TotalSections}";

        public List<bool> ProgressDots =>
            Enumerable.Range(1, TotalSections)
                      .Select(i => i <= CurrentSectionNumber)
                      .ToList();

        public SectionShellViewModel(Action<ObservableObject> navigate)
        {
            _navigate = navigate;
        }

        partial void OnCurrentSectionNumberChanged(int value)
        {
            OnPropertyChanged(nameof(SectionCounter));
            OnPropertyChanged(nameof(ProgressDots));
        }

        // Opens the cancel confirmation overlay.
        [RelayCommand]
        private void ShowCancelPrompt() => IsCancelPromptVisible = true;

        // Closes the overlay without doing anything — officer chose to continue.
        [RelayCommand]
        private void DismissCancel() => IsCancelPromptVisible = false;

        // Called by code-behind after the 4-second hold completes.
        // Hides the overlay then navigates home, discarding the in-progress report.
        public void CancelReport()
        {
            IsCancelPromptVisible = false;
            _navigate(new HomeViewModel(_navigate));
        }

        [RelayCommand]
        private void Confirm() { }

        [RelayCommand]
        private void SaveDraft() { }

        [RelayCommand]
        private void GoHome() => _navigate(new HomeViewModel(_navigate));
    }
}
