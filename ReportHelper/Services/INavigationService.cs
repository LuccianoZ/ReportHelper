using CommunityToolkit.Mvvm.ComponentModel;

namespace ReportHelper.Services
{
    // Abstraction over "switch the screen that's currently shown." ViewModels depend
    // on this instead of either a raw Action<ObservableObject> delegate or a raw
    // IServiceProvider — this keeps each ViewModel's constructor honest about what
    // it actually needs ("the ability to navigate"), and keeps navigation itself
    // fakeable in tests the same way IStorageService/IVoiceInputService already are.
    public interface INavigationService
    {
        // Builds a TViewModel and makes it the active top-level screen (sets
        // MainViewModel.CurrentViewModel). Any constructor parameter of TViewModel
        // that is registered in the DI container (services like IStorageService) is
        // resolved automatically. extraArgs supplies anything that is NOT a registered
        // service — e.g. a ReportRecord, which is per-report runtime data and has no
        // business being a singleton/transient service in the container.
        void NavigateTo<TViewModel>(params object[] extraArgs) where TViewModel : ObservableObject;

        // Builds a TViewModel the same way NavigateTo does (services + extraArgs
        // resolved/mixed via the container), but returns it instead of making it the
        // top-level screen. For callers that manage their OWN "current content" slot
        // rather than the app's top-level screen — e.g. SectionShellViewModel resolving
        // its CurrentSectionViewModel (S1, S2, S3...), which must NOT replace the shell
        // itself on MainViewModel.CurrentViewModel.
        TViewModel Resolve<TViewModel>(params object[] extraArgs) where TViewModel : ObservableObject;
    }
}
