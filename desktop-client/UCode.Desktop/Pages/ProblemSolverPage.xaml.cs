using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class ProblemSolverPage : UserControl
    {
        public ProblemSolverPage(ProblemSolverViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
