using System.Windows;
using System.Windows.Controls;
using UCode.Desktop.Services;
using UCode.Desktop.ViewModels.Admin;

namespace UCode.Desktop.Pages.Admin
{
    public partial class AdminClassesPage : Page
    {
        private readonly AdminClassesViewModel _viewModel;

        public AdminClassesPage(AdminService adminService)
        {
            InitializeComponent();
            _viewModel = new AdminClassesViewModel(adminService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadClassesAsync();
        }
    }
}
