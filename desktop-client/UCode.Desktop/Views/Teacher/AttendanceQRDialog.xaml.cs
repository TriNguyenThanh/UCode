using MahApps.Metro.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Views
{
    public partial class AttendanceQRDialog : MetroWindow
    {
        public AttendanceQRDialog(AttendanceQRViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void Close_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}
