using System.Windows;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class TeacherProblemsWindow : Window
    {
        public TeacherProblemsWindow(TeacherProblemsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (s, e) => await viewModel.LoadProblemsAsync();
        }
    }
}

