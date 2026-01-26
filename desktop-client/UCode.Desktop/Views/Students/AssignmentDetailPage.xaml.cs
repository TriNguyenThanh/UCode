using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views.Students
{
    public partial class AssignmentDetailPage : UserControl
    {
        private AssignmentDetailViewModel _viewModel;

        public AssignmentDetailPage()
        {
            InitializeComponent();
            _viewModel = App.ServiceProvider.GetService(typeof(AssignmentDetailViewModel)) as AssignmentDetailViewModel;
            DataContext = _viewModel;

            // Refresh data when page is loaded (e.g., when navigating back from ProblemSolverPage)
            Loaded += async (s, e) =>
            {
                if (_viewModel != null && !string.IsNullOrEmpty(_viewModel.AssignmentId))
                {
                    await _viewModel.RefreshDataAsync();
                }
            };
        }
    }
}
