using System;
using System.Net.Http;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using UCode.Desktop.Services;
using UCode.Desktop.Services.Admin;
using UCode.Desktop.ViewModels;
using UCode.Desktop.ViewModels.Admin;
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
                
                // Debug logging
                System.IO.File.AppendAllText(logPath, $"User Role: {user?.Role} (Enum value: {(int?)user?.Role})\n");
                System.IO.File.AppendAllText(logPath, $"Comparing with UserRole.Admin: {Models.UserRole.Admin} (Enum value: {(int)Models.UserRole.Admin})\n");
                
                // Change shutdown mode to close when main window closes
                ShutdownMode = ShutdownMode.OnMainWindowClose;
                
                if (user?.Role == Models.UserRole.Admin)
                {
                    System.IO.File.AppendAllText(logPath, "✅ Opening AdminMainWindow...\n");
                    var adminWindow = ServiceProvider.GetRequiredService<Views.AdminMainWindow>();
                    MainWindow = adminWindow;
                    adminWindow.Show();
                }
                else if (user?.Role == Models.UserRole.Teacher)
                {
                    System.IO.File.AppendAllText(logPath, "✅ Opening TeacherHomeWindow...\n");
                    var teacherWindow = ServiceProvider.GetRequiredService<TeacherHomeWindow>();
                    MainWindow = teacherWindow;
                    teacherWindow.Show();
                }
                else
                {
                    System.IO.File.AppendAllText(logPath, "✅ Opening MainWindow (Student)...\n");
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
        services.AddSingleton<AIDetectorService>();
        services.AddSingleton<AttendanceService>();

        // Admin Services
        services.AddSingleton<IDialogCoordinator, DialogCoordinator>();
        services.AddSingleton<AdminStatisticsService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            var dialogCoordinator = sp.GetRequiredService<IDialogCoordinator>();
            var tokenStorage = sp.GetRequiredService<TokenStorageService>();
            var authService = sp.GetRequiredService<AuthService>();
            return new AdminStatisticsService(httpClient, dialogCoordinator, tokenStorage, authService);
        });
        services.AddSingleton<AdminUserService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            var dialogCoordinator = sp.GetRequiredService<IDialogCoordinator>();
            var tokenStorage = sp.GetRequiredService<TokenStorageService>();
            var authService = sp.GetRequiredService<AuthService>();
            return new AdminUserService(httpClient, dialogCoordinator, tokenStorage, authService);
        });
        services.AddSingleton<AdminClassService>(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            var dialogCoordinator = sp.GetRequiredService<IDialogCoordinator>();
            var tokenStorage = sp.GetRequiredService<TokenStorageService>();
            var authService = sp.GetRequiredService<AuthService>();
            return new AdminClassService(httpClient, dialogCoordinator, tokenStorage, authService);
        });

        // ViewModels - Student
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<ClassDetailViewModel>();
        services.AddTransient<AssignmentDetailViewModel>();
        services.AddTransient<ProblemSolverViewModel>();

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
        services.AddTransient<CreateClassViewModel>();
        services.AddTransient<VisualSelectTabViewModel>();
        services.AddTransient<ImportExcelTabViewModel>();
        services.AddTransient<CreateAttendanceSessionViewModel>();
        services.AddTransient<AttendanceDetailViewModel>();

        // ViewModels - Admin
        services.AddTransient<AdminHomeViewModel>();
        services.AddTransient<AdminUsersViewModel>();
        services.AddTransient<AdminClassesViewModel>();

        // Views - Student
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
        services.AddTransient<ClassDetailWindow>();
        services.AddTransient<AssignmentDetailWindow>();
        services.AddTransient<ProblemSolverWindow>();

        // Views - Admin
        services.AddTransient<Views.AdminMainWindow>();
        services.AddTransient<Pages.Admin.AdminHomePage>();
        services.AddTransient<Pages.Admin.AdminUsersPage>();
        services.AddTransient<Pages.Admin.AdminClassesPage>();

        // Views - Teacher
        services.AddTransient<TeacherHomeWindow>();
        // services.AddTransient<TeacherProblemsWindow>(); // ← Đã chuyển sang Page
        services.AddTransient<TeacherGradingWindow>();
        // services.AddTransient<TeacherClassWindow>(); // ← Đã chuyển sang Page
        // services.AddTransient<TeacherAssignmentWindow>(); // ← Đã chuyển sang Page
        services.AddTransient<TeacherAssignmentEditWindow>();
        services.AddTransient<CreateAssignmentWindow>();
        services.AddTransient<CreateClassDialog>();
        // services.AddTransient<ProblemCreateWindow>(); // ← Đã chuyển sang Page
        // services.AddTransient<ProblemEditWindow>(); // ← Đã chuyển sang Page

        // Pages - Teacher (for navigation)
        services.AddTransient<Pages.TeacherHomePage>();
        services.AddTransient<Pages.TeacherClassPage>();
        services.AddTransient<Pages.TeacherAssignmentPage>();
        services.AddTransient<Pages.TeacherProblemsPage>();
        services.AddTransient<Pages.ProblemCreatePage>();
        services.AddTransient<Pages.ProblemEditPage>();
        services.AddTransient<Pages.CreateAttendanceSessionPage>(sp =>
        {
            var page = new Pages.CreateAttendanceSessionPage();
            page.DataContext = sp.GetRequiredService<CreateAttendanceSessionViewModel>();
            return page;
        });
        services.AddTransient<Pages.AttendanceDetailPage>(sp =>
        {
            var page = new Pages.AttendanceDetailPage();
            page.DataContext = sp.GetRequiredService<AttendanceDetailViewModel>();
            return page;
        });

        // Pages - Admin (for navigation)
        services.AddTransient<Pages.Admin.AdminHomePage>();
        services.AddTransient<Pages.Admin.AdminUsersPage>();
    }
}

