using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ReportHelper.Services;

namespace ReportHelper.ViewModels
{
    // MainViewModel owns CurrentViewModel (what MainWindow's ContentControl displays)
    // AND implements INavigationService, so it is both "the screen host" and "the thing
    // other ViewModels ask to change the screen." Other ViewModels never see
    // MainViewModel directly — they only see it through the INavigationService
    // interface, which is all they actually need.
    public partial class MainViewModel : ObservableObject, INavigationService
    {
        [ObservableProperty]
        private ObservableObject _currentViewModel = null!;

        // The DI container itself. This is the one place in the app that is allowed
        // to hold a raw IServiceProvider — MainViewModel's whole job IS construction
        // and navigation, so this is the composition root's natural home, not a
        // service-locator shortcut sprinkled elsewhere.
        private readonly IServiceProvider _serviceProvider;

        public MainViewModel(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            // NOTE: Navigation to the home screen is deliberately NOT done here.
            // MainViewModel is registered as a Singleton, and is ALSO what
            // INavigationService resolves to. If this constructor called
            // NavigateTo<HomeViewModel>() (which needs to resolve INavigationService,
            // which resolves back to this same MainViewModel singleton), the container
            // would be asked to hand out a MainViewModel that is still under
            // construction — a circular self-resolution that deadlocks instead of
            // throwing. Callers (App.xaml.cs) call NavigateToHome() explicitly, once
            // construction has fully completed and the container has returned control.
        }

        // Called once, by the composition root, after MainViewModel has been fully
        // constructed and resolved from the container — NOT from inside the
        // constructor itself (see note above for why).
        public void NavigateToHome() => NavigateTo<HomeViewModel>();

        public void NavigateTo<TViewModel>(params object[] extraArgs) where TViewModel : ObservableObject
        {
            CurrentViewModel = Resolve<TViewModel>(extraArgs);
        }

        public TViewModel Resolve<TViewModel>(params object[] extraArgs) where TViewModel : ObservableObject
        {
            // ActivatorUtilities.CreateInstance inspects TViewModel's constructor,
            // resolves any parameter that IS a registered service from _serviceProvider,
            // and fills in any parameter that is NOT a registered service (e.g. a
            // ReportRecord) from extraArgs, matching by type. This is what lets one
            // Resolve<T>(...) method handle every ViewModel shape in the app —
            // zero-dependency (HomeViewModel), pure-service (ReportHeaderViewModel),
            // and service-plus-runtime-data (SignOffViewModel) alike. NavigateTo<T>
            // and SectionShellViewModel's section-loading logic both build on this same
            // method — the only difference is what each caller does with the result.
            return ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider, extraArgs);
        }
    }
}
