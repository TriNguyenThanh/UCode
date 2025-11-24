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
                    var loginWindow = App.ServiceProvider.GetRequiredService<LoginWindow>();
                    loginWindow.Show();
                    Close();
                }
            });

            NavigateToHomeCommand = new RelayCommand(async _ =>
            {
                var adminHomePage = App.ServiceProvider.GetRequiredService<AdminHomePage>();
                var adminHomeViewModel = App.ServiceProvider.GetRequiredService<AdminHomeViewModel>();
                adminHomePage.SetViewModel(adminHomeViewModel);
                _navigationService.NavigateTo(adminHomePage);
                await adminHomeViewModel.LoadStatisticsAsync();
            });

            NavigateToUsersCommand = new RelayCommand(async _ =>
            {
                var adminUsersPage = App.ServiceProvider.GetRequiredService<AdminUsersPage>();
                var adminUsersViewModel = App.ServiceProvider.GetRequiredService<AdminUsersViewModel>();
                adminUsersPage.SetViewModel(adminUsersViewModel);
                _navigationService.NavigateTo(adminUsersPage);
                await adminUsersViewModel.LoadUsersAsync();
            });

            NavigateToClassesCommand = new RelayCommand(async _ =>
            {
                var adminClassesPage = App.ServiceProvider.GetRequiredService<Pages.Admin.AdminClassesPage>();
                var adminClassesViewModel = App.ServiceProvider.GetRequiredService<AdminClassesViewModel>();
                adminClassesPage.SetViewModel(adminClassesViewModel);
                _navigationService.NavigateTo(adminClassesPage);
                await adminClassesViewModel.LoadClassesAsync();
            });

            NavigateToLogsCommand = new RelayCommand(async _ =>
            {
                var adminLogsPage = App.ServiceProvider.GetRequiredService<AdminLogsPage>();
                var adminLogsViewModel = App.ServiceProvider.GetRequiredService<AdminLogsViewModel>();
                adminLogsPage.SetViewModel(adminLogsViewModel);
                _navigationService.NavigateTo(adminLogsPage);
                await adminLogsViewModel.InitializeAsync();
            });

            NavigateToSettingsCommand = new RelayCommand(async _ =>
            {
                var adminSettingsPage = App.ServiceProvider.GetRequiredService<AdminSettingsPage>();
                var adminSettingsViewModel = App.ServiceProvider.GetRequiredService<AdminSettingsViewModel>();
                adminSettingsPage.SetViewModel(adminSettingsViewModel);
                _navigationService.NavigateTo(adminSettingsPage);
                await adminSettingsViewModel.InitializeAsync();
            });

            // Navigate to home page by default
            Loaded += async (s, e) =>
            {
                var adminHomePage = App.ServiceProvider.GetRequiredService<AdminHomePage>();
                var adminHomeViewModel = App.ServiceProvider.GetRequiredService<AdminHomeViewModel>();
                adminHomePage.SetViewModel(adminHomeViewModel);
                _navigationService.NavigateTo(adminHomePage);
                await adminHomeViewModel.LoadStatisticsAsync();
            };
        }
    }
}
