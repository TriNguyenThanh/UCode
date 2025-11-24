using System.Windows;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views.Students
{
    public partial class ClassDetailWindow : MahApps.Metro.Controls.MetroWindow
    {
        public ClassDetailWindow(ClassDetailViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
