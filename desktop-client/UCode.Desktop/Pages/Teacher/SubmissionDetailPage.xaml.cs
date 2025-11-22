using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class SubmissionDetailPage : UserControl
    {
        public SubmissionDetailPage(SubmissionDetailViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
