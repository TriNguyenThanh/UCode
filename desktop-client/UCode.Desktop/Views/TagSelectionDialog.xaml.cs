using System.Linq;
using System.Windows;
using System.Windows.Input;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class TagSelectionDialog : Window
    {
        public TagSelectionDialog(TagSelectionViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += async (s, e) => await viewModel.LoadTagsAsync();
        }

        private void Tag_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is TagItem tag)
            {
                if (DataContext is TagSelectionViewModel viewModel)
                {
                    viewModel.ToggleTagCommand.Execute(tag.TagId);
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

