using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class ProblemCreatePage : UserControl
    {
        public ProblemCreatePage(ProblemCreateViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
