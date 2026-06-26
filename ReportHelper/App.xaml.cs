using System.Windows;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using ReportHelper.Services;
using ReportHelper.ViewModels;

namespace ReportHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // The DI container's root provider. Held here because OnStartup is the
        // composition root — the one place in the app responsible for wiring
        // every dependency together. Nothing outside this file should construct
        // services or ViewModels with "new" — everything flows through here.
        private IServiceProvider? _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Resolve MainWindow through the container so its constructor-injected
            // MainViewModel (and everything MainViewModel itself depends on) is
            // wired up automatically.
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            // Navigate to the home screen explicitly, now that MainViewModel is fully
            // constructed and the container has returned control. This canNOT be done
            // inside MainViewModel's own constructor — see the comment in
            // MainViewModel.cs for why that causes a circular-resolution deadlock.
            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
            mainViewModel.NavigateToHome();

            mainWindow.Show();
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // Note: IServiceProvider does NOT need an explicit registration here —
            // Microsoft.Extensions.DependencyInjection's BuildServiceProvider() already
            // makes the container resolvable as IServiceProvider automatically. This is
            // what lets MainViewModel take IServiceProvider as a constructor parameter
            // below with no extra wiring.

            // Services — Singleton because both are safe to share across the whole
            // app's lifetime (see Sprint 2 discussion: VoiceInputService fully resets
            // its recording state on each StartRecording/StopAndTranscribe cycle, and
            // push-to-talk guarantees only one recording happens at a time).
            services.AddSingleton<IStorageService>(_ =>
                new SqliteStorageService(GetDatabasePath()));

            services.AddSingleton<IVoiceInputService>(_ =>
                new VoiceInputService(GetWhisperModelPath()));

            // MainViewModel is registered as BOTH its concrete type and the
            // INavigationService interface, resolving to the SAME singleton instance.
            // This matters: every ViewModel that asks for INavigationService must get
            // back the one MainViewModel that actually owns CurrentViewModel — two
            // separate instances would mean navigation calls silently go nowhere.
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<INavigationService>(provider => provider.GetRequiredService<MainViewModel>());

            // Other ViewModels are Transient — a new instance every time NavigateTo<T>()
            // is called, which is exactly what "starting a new report" or "going home"
            // should mean. (Singleton here would mean re-using the same HomeViewModel
            // or SectionShellViewModel forever, which would be wrong — e.g. a second
            // report would start with leftover state from the first.)
            services.AddTransient<HomeViewModel>();
            services.AddTransient<SectionShellViewModel>();

            services.AddTransient<MainWindow>();
        }

        // Centralised here (rather than hardcoded inside SqliteStorageService) so the
        // composition root is the one place that decides where app data actually lives.
        private static string GetDatabasePath() =>
            Path.Combine(AppContext.BaseDirectory, "reporthelper.db");

        private static string GetWhisperModelPath() =>
            Path.Combine(AppContext.BaseDirectory, "Models", "Whisper", "ggml-small.bin");
    }
}
