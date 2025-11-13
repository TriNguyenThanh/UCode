using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class ProblemEditPage : UserControl
    {
        public ProblemEditPage(ProblemEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public async void InitializeWithProblemId(string problemId)
        {
            if (DataContext is ProblemEditViewModel viewModel)
            {
                await viewModel.InitializeAsync(problemId);
            }
        }
    }
}
