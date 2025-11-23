using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;
using UCode.Desktop.Models;

namespace UCode.Desktop.Views.Students
{
    public partial class SubmissionDetailDialog : MetroWindow
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
