using System.Windows;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class AddProblemDialog : Window
    {
        public AddProblemDialog(AddProblemDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (s, e) => await viewModel.LoadProblemsAsync();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    // Helper class for filter options
    public class FilterOption
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}

