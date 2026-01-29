using MahApps.Metro.Controls;
using System.Linq;
using System.Windows;
using UCode.Desktop.Models;

namespace UCode.Desktop.Views.Students
{
    public partial class TestCaseResultDialog : MetroWindow
    {
        public TestCaseResultDialog(Submission submission)
        {
            InitializeComponent();
            DataContext = submission;
            UpdateStatistics(submission);
        }

        private void UpdateStatistics(Submission submission)
        {
            if (submission?.TestCaseResults == null || !submission.TestCaseResults.Any())
                return;

            var stats = new
            {
                WrongAnswer = submission.TestCaseResults.Count(tc => tc.StatusCode == 5),
                TimeLimitExceeded = submission.TestCaseResults.Count(tc => tc.StatusCode == 1),
                MemoryLimitExceeded = submission.TestCaseResults.Count(tc => tc.StatusCode == 2),
                RuntimeError = submission.TestCaseResults.Count(tc => tc.StatusCode == 3),
                CompilationError = submission.TestCaseResults.Count(tc => tc.StatusCode == 6),
                InternalError = submission.TestCaseResults.Count(tc => tc.StatusCode == 4),
                Skipped = submission.TestCaseResults.Count(tc => tc.StatusCode == 7)
            };

            // Show/hide chips based on counts
            if (stats.WrongAnswer > 0)
            {
                WrongAnswerChip.Visibility = Visibility.Visible;
                WrongAnswerText.Text = $"Wrong Answer: {stats.WrongAnswer}";
            }

            if (stats.TimeLimitExceeded > 0)
            {
                TLEChip.Visibility = Visibility.Visible;
                TLEText.Text = $"TLE: {stats.TimeLimitExceeded}";
            }

            if (stats.MemoryLimitExceeded > 0)
            {
                MLEChip.Visibility = Visibility.Visible;
                MLEText.Text = $"MLE: {stats.MemoryLimitExceeded}";
            }

            if (stats.RuntimeError > 0)
            {
                RuntimeErrorChip.Visibility = Visibility.Visible;
                RuntimeErrorText.Text = $"Runtime Error: {stats.RuntimeError}";
            }

            if (stats.CompilationError > 0)
            {
                CompilationErrorChip.Visibility = Visibility.Visible;
                CompilationErrorText.Text = $"Compilation Error: {stats.CompilationError}";
            }

            if (stats.InternalError > 0)
            {
                InternalErrorChip.Visibility = Visibility.Visible;
                InternalErrorText.Text = $"Internal Error: {stats.InternalError}";
            }

            if (stats.Skipped > 0)
            {
                SkippedChip.Visibility = Visibility.Visible;
                SkippedText.Text = $"Skipped: {stats.Skipped}";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
