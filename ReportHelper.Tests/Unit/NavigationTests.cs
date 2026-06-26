using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using ReportHelper.Models;
using ReportHelper.Services;
using ReportHelper.ViewModels;
using ReportHelper.ViewModels.Sections;

namespace ReportHelper.Tests.Unit
{
    // A minimal zero-dependency ViewModel used only to prove NavigateTo<T>() works
    // for the simplest possible case, without pulling in real app ViewModels.
    public partial class FakeNoDependencyViewModel : ObservableObject { }

    // A ViewModel that takes one registered service — proves NavigateTo<T>() resolves
    // services from the container automatically.
    public partial class FakeServiceDependencyViewModel : ObservableObject
    {
        public IStorageService StorageService { get; }
        public FakeServiceDependencyViewModel(IStorageService storageService)
        {
            StorageService = storageService;
        }
    }

    // A ViewModel that takes one registered service AND one piece of runtime data not
    // in the container — proves NavigateTo<T>(extraArgs) correctly mixes both sources,
    // which is exactly the SignOffViewModel(IStorageService, ReportRecord) shape.
    public partial class FakeMixedDependencyViewModel : ObservableObject
    {
        public IStorageService StorageService { get; }
        public ReportRecord Report { get; }
        public FakeMixedDependencyViewModel(IStorageService storageService, ReportRecord report)
        {
            StorageService = storageService;
            Report = report;
        }
    }

    // A no-op fake storage service — these tests care about navigation/construction
    // behaviour, not storage behaviour, so every method is a harmless no-op.
    public class NoOpStorageService : IStorageService
    {
        public void SaveDraft(ReportRecord draft) { }
        public ReportRecord? LoadDraft() => null;
        public void SaveReport(ReportRecord report) { }
        public List<ReportRecord> GetAllReports() => new List<ReportRecord>();
        public string GetNextReportNumber(DateTime date) => "TEST-0001";
    }

    public class NavigationTests
    {
        // Builds a real DI container (not a fake) with the fake ViewModels/services
        // above registered. This is the most honest way to test MainViewModel's
        // NavigateTo<T>(), since its whole job IS talking to a real IServiceProvider —
        // faking the container itself would mean not testing the thing we actually
        // care about (does ActivatorUtilities.CreateInstance behave the way we expect).
        //
        // IMPORTANT: MainViewModel's constructor calls NavigateTo<HomeViewModel>()
        // immediately (the app always opens on the home screen) — so ANY test that
        // constructs a MainViewModel must also have HomeViewModel's own dependencies
        // registered, even if the test itself only cares about a different ViewModel.
        // INavigationService is registered here pointing back at the container's own
        // MainViewModel for that reason.
        private static IServiceProvider BuildTestContainer()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IStorageService, NoOpStorageService>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<INavigationService>(p => p.GetRequiredService<MainViewModel>());
            services.AddTransient<HomeViewModel>();
            services.AddTransient<FakeNoDependencyViewModel>();
            services.AddTransient<FakeServiceDependencyViewModel>();
            services.AddTransient<FakeMixedDependencyViewModel>();
            return services.BuildServiceProvider();
        }

        [Fact]
        public void NavigateTo_NoDependencyViewModel_BecomesCurrentViewModel()
        {
            // Arrange
            var provider = BuildTestContainer();
            var mainViewModel = provider.GetRequiredService<MainViewModel>();

            // Act
            mainViewModel.NavigateTo<FakeNoDependencyViewModel>();

            // Assert
            Assert.IsType<FakeNoDependencyViewModel>(mainViewModel.CurrentViewModel);
        }

        [Fact]
        public void NavigateTo_ServiceDependencyViewModel_ResolvesServiceFromContainer()
        {
            // Arrange
            var provider = BuildTestContainer();
            var mainViewModel = provider.GetRequiredService<MainViewModel>();

            // Act
            mainViewModel.NavigateTo<FakeServiceDependencyViewModel>();

            // Assert
            var result = Assert.IsType<FakeServiceDependencyViewModel>(mainViewModel.CurrentViewModel);
            Assert.NotNull(result.StorageService);
            Assert.IsType<NoOpStorageService>(result.StorageService);
        }

        [Fact]
        public void NavigateTo_MixedDependencyViewModel_ResolvesServiceAndAcceptsExtraArg()
        {
            // Arrange — this is the SignOffViewModel(IStorageService, ReportRecord) case:
            // one constructor parameter comes from the container, one comes from extraArgs.
            var provider = BuildTestContainer();
            var mainViewModel = provider.GetRequiredService<MainViewModel>();
            var report = new ReportRecord { ReportNumber = "20260623-0001" };

            // Act
            mainViewModel.NavigateTo<FakeMixedDependencyViewModel>(report);

            // Assert
            var result = Assert.IsType<FakeMixedDependencyViewModel>(mainViewModel.CurrentViewModel);
            Assert.NotNull(result.StorageService);
            Assert.Same(report, result.Report); // confirms the EXACT instance we passed in was used, not a new/blank one
        }

        [Fact]
        public void NavigateToHome_SetsHomeViewModelAsCurrentViewModel()
        {
            // Arrange — uses the same shared container helper as the other tests above,
            // since HomeViewModel's dependencies are already registered there.
            // NavigateToHome() is called explicitly here rather than expected from
            // construction — see MainViewModel.cs for why eager construction-time
            // navigation causes a circular-resolution deadlock with INavigationService.
            var provider = BuildTestContainer();
            var mainViewModel = provider.GetRequiredService<MainViewModel>();

            // Act
            mainViewModel.NavigateToHome();

            // Assert
            Assert.IsType<HomeViewModel>(mainViewModel.CurrentViewModel);
        }

        // BL-09 / SC-02, SC-03: When the officer starts a new report, SectionShellViewModel
        // is created and must immediately load S1 (Report Header) into
        // CurrentSectionViewModel — WITHOUT replacing itself on MainViewModel's screen.
        // This proves Resolve<T>() (not NavigateTo<T>()) is what SectionShellViewModel
        // uses internally — if it had used NavigateTo<T>() by mistake, CurrentViewModel
        // on MainViewModel would have been overwritten, but CurrentSectionViewModel
        // would still be null, which the assertion below would catch.
        [Fact]
        public void SectionShellViewModel_Constructor_LoadsReportHeaderAsCurrentSection()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<IStorageService, NoOpStorageService>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<INavigationService>(p => p.GetRequiredService<MainViewModel>());
            services.AddTransient<SectionShellViewModel>();
            services.AddTransient<HomeViewModel>();
            var provider = services.BuildServiceProvider();
            var navigation = provider.GetRequiredService<INavigationService>();
            var mainViewModel = provider.GetRequiredService<MainViewModel>();
            mainViewModel.NavigateToHome(); // establish a known starting screen, like the real app does

            // Act
            var shell = navigation.Resolve<SectionShellViewModel>();

            // Assert
            Assert.IsType<ReportHeaderViewModel>(shell.CurrentSectionViewModel);

            // Assert — this is the regression check for the exact mistake the design
            // discussion was guarding against: if SectionShellViewModel's constructor
            // had called NavigateTo<ReportHeaderViewModel>() instead of Resolve<T>(),
            // MainViewModel.CurrentViewModel would have been silently overwritten with
            // a ReportHeaderViewModel, replacing the whole shell instead of populating
            // its inner content. It must still be the HomeViewModel we navigated to above.
            Assert.IsType<HomeViewModel>(mainViewModel.CurrentViewModel);
        }
    }
}
