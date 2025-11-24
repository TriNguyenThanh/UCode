using System.Windows;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views.Students
{
    public partial class AssignmentDetailWindow : MahApps.Metro.Controls.MetroWindow
    {
        public AssignmentDetailWindow(AssignmentDetailViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
