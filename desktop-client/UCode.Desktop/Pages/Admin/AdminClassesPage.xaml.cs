using System.Windows.Controls;
using MahApps.Metro.Controls.Dialogs;
using UCode.Desktop.ViewModels.Admin;

namespace UCode.Desktop.Pages.Admin
{
    public partial class AdminClassesPage : UserControl
    {
        private AdminClassesViewModel? _viewModel;

        public AdminClassesPage()
        {
            InitializeComponent();
        }

        public void SetViewModel(AdminClassesViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}
