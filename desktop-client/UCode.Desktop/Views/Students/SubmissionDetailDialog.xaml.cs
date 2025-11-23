using MahApps.Metro.Controls;
using System.Windows;
using UCode.Desktop.Controls;
using UCode.Desktop.Models;

namespace UCode.Desktop.Views.Students
{
    public partial class SubmissionDetailDialog : UCodeWindow
    {
        public SubmissionDetailDialog(Submission submission)
        {
            InitializeComponent();
            DataContext = submission;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
