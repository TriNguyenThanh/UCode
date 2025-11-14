using System.Windows;
using MahApps.Metro.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class TeacherAssignmentEditWindow : MetroWindow
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

