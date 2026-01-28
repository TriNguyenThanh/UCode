using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Input;
using UCode.Desktop.Pages.Admin;
using UCode.Desktop.Services;
using UCode.Desktop.Helpers;
using UCode.Desktop.ViewModels.Admin;

namespace UCode.Desktop.Views
{
    public partial class AdminMainWindow : MetroWindow
    {
        private readonly NavigationService _navigationService;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly AuthService _authService;
        private readonly AIDetectorService _aiDetectorService;

        public ICommand GoBackCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand NavigateToHomeCommand { get; }
        public ICommand NavigateToUsersCommand { get; }
        public ICommand NavigateToClassesCommand { get; }
        public ICommand NavigateToLogsCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }

        public Models.User? CurrentUser { get; private set; }

        public AdminMainWindow()
        {
            InitializeComponent();

            // Get services from DI
            _navigationService = App.ServiceProvider.GetRequiredService<NavigationService>();
            _dialogCoordinator = App.ServiceProvider.GetRequiredService<IDialogCoordinator>();
            _authService = App.ServiceProvider.GetRequiredService<AuthService>();
            _aiDetectorService = App.ServiceProvider.GetRequiredService<AIDetectorService>();

            // Set data context to this window for command bindings
            DataContext = this;

            // Set up navigation
            _navigationService.SetFrame(MainContentFrame);
            _navigationService.CanGoBackChanged += (s, canGoBack) =>
            {
                // Update UI or command can execute if needed
            };

            // Load current user
            CurrentUser = _authService.CurrentUser;

            // Initialize commands
            GoBackCommand = new RelayCommand(
                _ => _navigationService.GoBack(),
                _ => _navigationService.CanGoBack
            );

            LogoutCommand = new RelayCommand(async _ =>
            {
                var result = await _dialogCoordinator.ShowMessageAsync(
                    this,
                    "Xác nhận đăng xuất",
                    "Bạn có chắc chắn muốn đăng xuất?",
                    MessageDialogStyle.AffirmativeAndNegative
                );

                if (result == MessageDialogResult.Affirmative)
                {
                    _authService.Logout();

                    // Clear navigation stack
                    _navigationService.ClearNavigationStack();

                    // Reset shutdown mode to prevent app from closing
                    Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                    var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
                    loginWindow.Show();
                    Close();
                }
            });

            NavigateToHomeCommand = new RelayCommand(_ =>
            {
                var adminHomePage = App.ServiceProvider?.GetService(typeof(AdminHomePage)) as AdminHomePage;
                if (adminHomePage != null)
                {
                    _navigationService.NavigateTo(adminHomePage, null, false);
                }
            });

            NavigateToUsersCommand = new RelayCommand(_ =>
            {
                var adminUsersPage = App.ServiceProvider?.GetService(typeof(AdminUsersPage)) as AdminUsersPage;
                if (adminUsersPage != null)
                {
                    _navigationService.NavigateTo(adminUsersPage, null, false);
                }
            });

            NavigateToClassesCommand = new RelayCommand(_ =>
            {
                var adminClassesPage = App.ServiceProvider?.GetService(typeof(AdminClassesPage)) as AdminClassesPage;
                if (adminClassesPage != null)
                {
                    _navigationService.NavigateTo(adminClassesPage, null, false);
                }
            });

            NavigateToLogsCommand = new RelayCommand(_ =>
            {
                var adminLogsPage = App.ServiceProvider?.GetService(typeof(AdminLogsPage)) as AdminLogsPage;
                if (adminLogsPage != null)
                {
                    _navigationService.NavigateTo(adminLogsPage, null, false);
                }
            });

            NavigateToSettingsCommand = new RelayCommand(_ =>
            {
                var adminSettingsPage = App.ServiceProvider?.GetService(typeof(AdminSettingsPage)) as AdminSettingsPage;
                if (adminSettingsPage != null)
                {
                    _navigationService.NavigateTo(adminSettingsPage, null, false);
                }
            });

            // Navigate to home page by default
            Loaded += (s, e) =>
            {
                var adminHomePage = App.ServiceProvider?.GetService(typeof(AdminHomePage)) as AdminHomePage;
                if (adminHomePage != null)
                {
                    _navigationService.NavigateTo(adminHomePage, null, false);
                }
            };

            // Cleanup when window closes
            Closing += (s, e) =>
            {
                try
                {
                    _aiDetectorService?.StopAIDetector();
                }
                catch { /* Ignore cleanup errors */ }
            };

            // Handle Backspace key for navigation back
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Back && _navigationService.CanGoBack)
                {
                    // Don't trigger if focus is on a TextBox or similar input control
                    var focusedElement = Keyboard.FocusedElement;
                    if (focusedElement is System.Windows.Controls.TextBox ||
                        focusedElement is System.Windows.Controls.PasswordBox ||
                        focusedElement is System.Windows.Controls.RichTextBox ||
                        focusedElement is ICSharpCode.AvalonEdit.Editing.TextArea ||
                        focusedElement is ICSharpCode.AvalonEdit.TextEditor)
                    {
                        return; // Let input control handle the Backspace
                    }

                    // Don't navigate back if on main tab pages
                    bool isMainTab = MainContentFrame.Content is AdminHomePage ||
                                     MainContentFrame.Content is AdminUsersPage ||
                                     MainContentFrame.Content is Pages.Admin.AdminClassesPage ||
                                     MainContentFrame.Content is AdminLogsPage ||
                                     MainContentFrame.Content is AdminSettingsPage;
                    if (isMainTab)
                    {
                        e.Handled = true;
                        return;
                    }

                    _navigationService.GoBack();
                    e.Handled = true;
                }
            };
        }
    }
}
