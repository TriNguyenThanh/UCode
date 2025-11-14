using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MahApps.Metro.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class LanguageSelectionDialog : MetroWindow
    {
        public LanguageSelectionDialog(LanguageSelectionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (s, e) => await viewModel.LoadLanguagesAsync();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DataGridRow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is LanguageItemViewModel language)
            {
                // Toggle checkbox khi click vào row
                language.IsSelected = !language.IsSelected;
            }
        }
    }
}

