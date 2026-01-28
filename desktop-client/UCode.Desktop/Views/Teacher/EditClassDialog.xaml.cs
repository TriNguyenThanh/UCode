using System.Windows;
using MahApps.Metro.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class EditClassDialog : MetroWindow
    {
        private readonly EditClassViewModel _viewModel;

        public EditClassDialog(EditClassViewModel viewModel)
        {
            _viewModel = viewModel;
            DataContext = _viewModel;  // Set DataContext TRƯỚC InitializeComponent
            InitializeComponent();

            _viewModel.CloseRequested += (success) =>
            {
                DialogResult = success;
                Close();
            };
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.SaveChangesAsync();
        }
    }
}
