using System.Windows;
using System.Windows.Controls;
using UCode.Desktop.Services;
using UCode.Desktop.ViewModels.Admin;

namespace UCode.Desktop.Pages.Admin
{
    public partial class AdminUsersPage : Page
    {
        private readonly AdminUsersViewModel _viewModel;

        public AdminUsersPage(AdminService adminService)
        {
            InitializeComponent();
            _viewModel = new AdminUsersViewModel(adminService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadUsersAsync();
        }
    }
}
