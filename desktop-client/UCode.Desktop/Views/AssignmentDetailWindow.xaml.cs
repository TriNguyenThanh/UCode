using System.Windows;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
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

