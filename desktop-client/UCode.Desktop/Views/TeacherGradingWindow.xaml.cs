using System.Windows;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class TeacherGradingWindow : Window
    {
        public TeacherGradingWindow(TeacherGradingViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public async void Initialize(string assignmentId)
        {
            if (DataContext is TeacherGradingViewModel viewModel)
            {
                await viewModel.InitializeAsync(assignmentId);
            }
        }
    }
}

