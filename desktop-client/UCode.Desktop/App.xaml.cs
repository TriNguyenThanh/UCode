using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UCode.Desktop.Services;
using UCode.Desktop.ViewModels;
using UCode.Desktop.Views;

namespace UCode.Desktop;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            // Log to file for debugging
            var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup.log");
            System.IO.File.WriteAllText(logPath, $"App starting at {DateTime.Now}\n");

            var serviceCollection = new ServiceCollection();
            System.IO.File.AppendAllText(logPath, "Configuring services...\n");
            ConfigureServices(serviceCollection);

            System.IO.File.AppendAllText(logPath, "Building service provider...\n");
            ServiceProvider = serviceCollection.BuildServiceProvider();

            System.IO.File.AppendAllText(logPath, "Creating login window...\n");
            // Show login window
            var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();

            System.IO.File.AppendAllText(logPath, "Showing login window...\n");
            loginWindow.Show();

            System.IO.File.AppendAllText(logPath, "App started successfully!\n");
        }
        catch (Exception ex)
        {
            var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            var errorMsg = $"ERROR at {DateTime.Now}\n{ex.GetType().Name}: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}\n\nInner Exception:\n{ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}";
            System.IO.File.WriteAllText(logPath, errorMsg);

            MessageBox.Show($"App crashed! Check error.log\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddHttpClient<ApiService>();
        services.AddSingleton<AuthService>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();

        // Views
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
    }
}

