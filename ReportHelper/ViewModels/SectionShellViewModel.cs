using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReportHelper.Services;
using ReportHelper.ViewModels.Sections;

namespace ReportHelper.ViewModels
{
    public partial class SectionShellViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;

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

        public SectionShellViewModel(INavigationService navigation)
        {
            _navigation = navigation;

            // S1 (Report Header) is always the first section an officer sees when
            // starting a new report. This uses Resolve<T>(), NOT NavigateTo<T>() —
            // NavigateTo would set MainViewModel.CurrentViewModel, replacing this
            // shell entirely instead of populating its inner content area. Resolve
            // builds the ViewModel the same way (services from the container) but
            // hands it back instead of touching the top-level screen.
            CurrentSectionViewModel = _navigation.Resolve<ReportHeaderViewModel>();
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
            _navigation.NavigateTo<HomeViewModel>();
        }

        [RelayCommand]
        private void Confirm() { }

        [RelayCommand]
        private void SaveDraft() { }

        [RelayCommand]
        private void GoHome() => _navigation.NavigateTo<HomeViewModel>();
    }
}
