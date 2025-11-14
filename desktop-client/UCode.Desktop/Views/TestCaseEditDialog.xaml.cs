using System.Windows;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class TestCaseEditDialog : Window
    {
        public TestCaseEditDialog(TestCaseEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

