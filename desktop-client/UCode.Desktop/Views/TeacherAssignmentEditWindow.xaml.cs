using System.Windows;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class TeacherAssignmentEditWindow : Window
    {
        public TeacherAssignmentEditWindow(TeacherAssignmentEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public async void Initialize(string assignmentId = null, string classId = null)
        {
            if (DataContext is TeacherAssignmentEditViewModel viewModel)
            {
                await viewModel.InitializeAsync(assignmentId, classId);
            }
        }
    }
}

