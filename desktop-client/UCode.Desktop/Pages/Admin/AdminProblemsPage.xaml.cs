using System.Windows;
using System.Windows.Controls;
using UCode.Desktop.Services;
using UCode.Desktop.ViewModels.Admin;

namespace UCode.Desktop.Pages.Admin
{
    public partial class AdminProblemsPage : Page
    {
        private readonly AdminProblemsViewModel _viewModel;

        public AdminProblemsPage(AdminService adminService)
        {
            InitializeComponent();
            _viewModel = new AdminProblemsViewModel(adminService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadProblemsAsync();
        }
    }
}
