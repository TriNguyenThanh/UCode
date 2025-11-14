using System;
using System.Net.Http;
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
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Prevent app from closing when login window closes
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Global exception handlers
        DispatcherUnhandledException += (sender, args) =>
        {
            var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "unhandled_exception.log");
            System.IO.File.WriteAllText(logPath, $"UNHANDLED EXCEPTION at {DateTime.Now}\n{args.Exception.Message}\n\nStack Trace:\n{args.Exception.StackTrace}\n\nInner:\n{args.Exception.InnerException?.Message}\n{args.Exception.InnerException?.StackTrace}");
            MessageBox.Show($"Unhandled exception: {args.Exception.Message}\n\nSee unhandled_exception.log for details", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true; // Prevent app from crashing
        };

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "domain_exception.log");
            System.IO.File.WriteAllText(logPath, $"DOMAIN EXCEPTION at {DateTime.Now}\n{ex?.Message}\n\nStack Trace:\n{ex?.StackTrace}");
        };

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

            System.IO.File.AppendAllText(logPath, "Trying auto-login...\n");
            // Try auto-login first
            var authService = ServiceProvider.GetRequiredService<AuthService>();
            bool autoLoginSuccess = false;
            
            try
            {
                var autoLoginTask = authService.TryAutoLoginAsync();
                autoLoginTask.Wait();
                autoLoginSuccess = autoLoginTask.Result;
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(logPath, $"Auto-login exception: {ex.Message}\n");
                autoLoginSuccess = false;
            }
            
            if (autoLoginSuccess)
            {
                System.IO.File.AppendAllText(logPath, "Auto-login successful, opening main window...\n");
                // Auto-login successful, open main window based on user role
                var user = authService.CurrentUser;
                
                // Change shutdown mode to close when main window closes
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                
                if (user?.Role.ToString().ToLower() == "teacher")
                {
                    var teacherWindow = ServiceProvider.GetRequiredService<TeacherHomeWindow>();
                    MainWindow = teacherWindow;
                    teacherWindow.Show();
                }
                else
                {
                    var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                    MainWindow = mainWindow;
                    mainWindow.Show();
                }
            }
            else
            {
                System.IO.File.AppendAllText(logPath, "Auto-login failed, showing login window...\n");
                // Show login window
                var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
            }

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
        // Register HttpClient and ApiService as Singleton
        services.AddHttpClient();
        services.AddSingleton<ApiService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            return new ApiService(httpClient);
        });

        // Services
        services.AddSingleton<TokenStorageService>();
        services.AddSingleton<AuthService>();
        services.AddSingleton<ProblemService>();
        services.AddSingleton<AssignmentService>();
        services.AddSingleton<ClassService>();
        services.AddSingleton<SubmissionService>();
        services.AddSingleton<LanguageService>();
        services.AddSingleton<DatasetService>();
        services.AddSingleton<TagService>();
        services.AddSingleton<NavigationService>();

        // ViewModels - Student
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();

        // ViewModels - Teacher
        services.AddTransient<TeacherHomeViewModel>();
        services.AddTransient<TeacherProblemsViewModel>();
        services.AddTransient<TeacherGradingViewModel>();
        services.AddTransient<TeacherClassViewModel>();
        services.AddTransient<TeacherAssignmentViewModel>();
        services.AddTransient<TeacherAssignmentEditViewModel>();
        services.AddTransient<CreateAssignmentViewModel>();
        services.AddTransient<ProblemCreateViewModel>();
        services.AddTransient<ProblemEditViewModel>();
        services.AddTransient<TagSelectionViewModel>();
        services.AddTransient<LanguageSelectionViewModel>();
        services.AddTransient<DatasetEditViewModel>();
        services.AddTransient<TestCaseEditViewModel>();
        services.AddTransient<AddProblemDialogViewModel>();
        services.AddTransient<AddStudentDialogViewModel>();
        services.AddTransient<VisualSelectTabViewModel>();
        services.AddTransient<ImportExcelTabViewModel>();

        // Views - Student
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();

        // Views - Teacher
        services.AddTransient<TeacherHomeWindow>();
        // services.AddTransient<TeacherProblemsWindow>(); // ← Đã chuyển sang Page
        services.AddTransient<TeacherGradingWindow>();
        // services.AddTransient<TeacherClassWindow>(); // ← Đã chuyển sang Page
        // services.AddTransient<TeacherAssignmentWindow>(); // ← Đã chuyển sang Page
        services.AddTransient<TeacherAssignmentEditWindow>();
        services.AddTransient<CreateAssignmentWindow>();
        // services.AddTransient<ProblemCreateWindow>(); // ← Đã chuyển sang Page
        // services.AddTransient<ProblemEditWindow>(); // ← Đã chuyển sang Page

        // Pages - Teacher (for navigation)
        services.AddTransient<Pages.TeacherHomePage>();
        services.AddTransient<Pages.TeacherClassPage>();
        services.AddTransient<Pages.TeacherAssignmentPage>();
        services.AddTransient<Pages.TeacherProblemsPage>();
        services.AddTransient<Pages.ProblemCreatePage>();
        services.AddTransient<Pages.ProblemEditPage>();
    }
}

