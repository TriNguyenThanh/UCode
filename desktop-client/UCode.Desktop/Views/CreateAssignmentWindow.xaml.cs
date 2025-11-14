using System.Windows;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class CreateAssignmentWindow : Window
    {
        public CreateAssignmentWindow(CreateAssignmentViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public async void Initialize(string classId)
        {
            if (DataContext is CreateAssignmentViewModel viewModel)
            {
                await viewModel.InitializeAsync(classId);
            }
        }
    }
}

