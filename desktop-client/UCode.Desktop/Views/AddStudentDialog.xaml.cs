using System.Windows;
using UCode.Desktop.ViewModels;
using MahApps.Metro.Controls;

namespace UCode.Desktop.Views
{
    public partial class AddStudentDialog : MetroWindow
    {
        public AddStudentDialog(AddStudentDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

