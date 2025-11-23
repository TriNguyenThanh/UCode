using System.Windows.Controls;
using UCode.Desktop.ViewModels.Admin;

namespace UCode.Desktop.Pages.Admin
{
    public partial class AdminLogsPage : UserControl
    {
        public AdminLogsPage()
        {
            InitializeComponent();
        }

        public void SetViewModel(AdminLogsViewModel viewModel)
        {
            DataContext = viewModel;
            _ = viewModel.InitializeAsync();
        }
    }
}
