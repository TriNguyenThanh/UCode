using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class TeacherProblemsPage : System.Windows.Controls.UserControl
    {
        public TeacherProblemsPage(TeacherProblemsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Load data when page loads
            Loaded += async (s, e) =>
            {
                await viewModel.LoadProblemsAsync();
            };
        }
    }
}
