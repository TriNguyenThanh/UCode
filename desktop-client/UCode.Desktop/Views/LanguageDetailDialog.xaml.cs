using MahApps.Metro.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class LanguageDetailDialog : MetroWindow
    {
        public LanguageDetailDialog(LanguageDetailViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.SaveCompleted += (s, e) =>
            {
                DialogResult = true;
                Close();
            };

            viewModel.CancelRequested += (s, e) =>
            {
                DialogResult = false;
                Close();
            };
        }
    }
}
