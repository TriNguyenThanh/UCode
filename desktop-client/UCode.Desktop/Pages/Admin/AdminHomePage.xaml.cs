using System.Windows.Controls;
using UCode.Desktop.ViewModels.Admin;

namespace UCode.Desktop.Pages.Admin
{
    public partial class AdminHomePage : UserControl
    {
        public AdminHomePage()
        {
            InitializeComponent();
        }

        public void SetViewModel(AdminHomeViewModel viewModel)
        {
            DataContext = viewModel;
            _ = viewModel.InitializeAsync();
        }
    }
}
