using System.Windows.Controls;
using UCode.Desktop.ViewModels.Admin;

namespace UCode.Desktop.Pages.Admin
{
    public partial class AdminHomePage : UserControl
    {
        public AdminHomePage(AdminHomeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            // Load data when page is loaded (non-blocking)
            Loaded += async (s, e) => await viewModel.LoadStatisticsAsync();
        }
    }
}
