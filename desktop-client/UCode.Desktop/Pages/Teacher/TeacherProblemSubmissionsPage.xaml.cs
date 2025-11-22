using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class TeacherProblemSubmissionsPage : UserControl
    {
        public TeacherProblemSubmissionsPage(TeacherProblemSubmissionsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
