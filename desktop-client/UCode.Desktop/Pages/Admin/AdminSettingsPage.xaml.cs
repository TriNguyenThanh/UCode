using System.Windows.Controls;
using UCode.Desktop.ViewModels.Admin;

namespace UCode.Desktop.Pages.Admin
{
    public partial class AdminSettingsPage : UserControl
    {
        public AdminSettingsPage()
        {
            InitializeComponent();
        }

        public void SetViewModel(AdminSettingsViewModel viewModel)
        {
            DataContext = viewModel;
            _ = viewModel.InitializeAsync();
        }
    }
}
