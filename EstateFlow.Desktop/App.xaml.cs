using EstateFlow.Desktop.Mvvm;
using EstateFlow.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace EstateFlow.Desktop;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var services = new ServiceCollection();

            // ==========================================
            // AUTHENTICATION
            // ==========================================

            services.AddSingleton<IAuthState, AuthState>();
            services.AddTransient<AuthTokenHandler>();

            // ==========================================
            // API CLIENT
            // ==========================================

            services.AddHttpClient<IApiClient, ApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://estateflow-hmbmeqd3brdnfab6.uaenorth-01.azurewebsites.net/");
            })
 .AddHttpMessageHandler<AuthTokenHandler>();

            // ==========================================
            // VIEW MODELS
            // ==========================================

            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<EstatesViewModel>();
            services.AddTransient<EstateDetailViewModel>();
            services.AddTransient<DiaryViewModel>();
            services.AddTransient<ReportsViewModel>();
            services.AddTransient<DocumentScanViewModel>();

            // New Create / Edit Estate ViewModel
            services.AddTransient<EstateFormViewModel>();

            // ==========================================
            // SHELL / NAVIGATION
            // ==========================================

            services.AddSingleton<ShellViewModel>();

            services.AddSingleton<INavigationService>(sp =>
                sp.GetRequiredService<ShellViewModel>());

            services.AddSingleton<IShellState>(sp =>
                sp.GetRequiredService<ShellViewModel>());

            // ==========================================
            // MAIN WINDOW
            // ==========================================

            services.AddSingleton<MainWindow>();

            // ==========================================
            // BUILD DI CONTAINER
            // ==========================================

            _serviceProvider = services.BuildServiceProvider();

            // IMPORTANT:
            // ShellViewModel must finish constructing BEFORE
            // LoginViewModel is resolved.
            var shell =
                _serviceProvider.GetRequiredService<ShellViewModel>();

            // Initial navigation happens HERE,
            // not inside ShellViewModel's constructor.
            shell.NavigateToLogin();

            // ==========================================
            // SHOW MAIN WINDOW
            // ==========================================

            var mainWindow =
                _serviceProvider.GetRequiredService<MainWindow>();

            MainWindow = mainWindow;

            mainWindow.Show();
            mainWindow.Activate();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Startup failed:\n\n{ex}",
                "EstateFlow — Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();

        base.OnExit(e);
    }
}