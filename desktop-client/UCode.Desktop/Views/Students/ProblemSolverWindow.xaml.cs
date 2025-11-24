using System.Windows;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views.Students
{
    public partial class ProblemSolverWindow : MahApps.Metro.Controls.MetroWindow
    {
        public ProblemSolverWindow(ProblemSolverViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
