using CommunityToolkit.Mvvm.ComponentModel;

namespace ReportHelper.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // The ViewModel currently on screen. Changing this property automatically
        // triggers WPF to swap to the matching view via DataTemplates in MainWindow.xaml.
        [ObservableProperty]
        private ObservableObject _currentViewModel = null!;

        public MainViewModel()
        {
            // App always opens on the home screen.
            CurrentViewModel = new HomeViewModel(NavigateTo);
        }

        // Passed as a callback to child ViewModels so they can trigger navigation
        // without needing a direct reference to MainViewModel.
        private void NavigateTo(ObservableObject viewModel)
        {
            CurrentViewModel = viewModel;
        }
    }
}
