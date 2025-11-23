using System.Windows;
using MahApps.Metro.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class CreateClassDialog : MetroWindow
    {
        private readonly CreateClassViewModel _viewModel;

        public CreateClassDialog(CreateClassViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
            
            _viewModel.CloseRequested += (success, classId) =>
            {
                DialogResult = success;
                if (success)
                {
                    CreatedClassId = classId;
                }
                Close();
            };
        }

        public string? CreatedClassId { get; private set; }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.CreateClassAsync();
        }

        private void DescriptionTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
