using System.Windows;
using System.Windows.Controls;
using UCode.Desktop.Services;
using UCode.Desktop.ViewModels.Admin;

namespace UCode.Desktop.Pages.Admin
{
    public partial class AdminHomePage : UserControl
    {
        private readonly AdminHomeViewModel _viewModel;

        public AdminHomePage(AdminService adminService, NavigationService navigationService)
        {
            InitializeComponent();
            _viewModel = new AdminHomeViewModel(adminService, navigationService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDashboardDataAsync();
        }

        private void UsersCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel.NavigateToUsersCommand.Execute(null);
        }

        private void ClassesCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel.NavigateToClassesCommand.Execute(null);
        }

        private void ProblemsCard_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel.NavigateToProblemsCommand.Execute(null);
        }
    }
}
