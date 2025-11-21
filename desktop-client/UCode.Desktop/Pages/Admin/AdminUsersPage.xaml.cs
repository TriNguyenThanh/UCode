using System.Windows.Controls;
using UCode.Desktop.ViewModels.Admin;

namespace UCode.Desktop.Pages.Admin
{
    public partial class AdminUsersPage : UserControl
    {
        public AdminUsersPage()
        {
            InitializeComponent();
        }

        public void SetViewModel(AdminUsersViewModel viewModel)
        {
            DataContext = viewModel;
            _ = viewModel.InitializeAsync();
        }
    }
}
