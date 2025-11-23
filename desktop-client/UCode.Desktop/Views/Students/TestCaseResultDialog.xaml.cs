using System.Collections.Generic;
using System.Windows;
using MahApps.Metro.Controls;
using UCode.Desktop.Models;

namespace UCode.Desktop.Views.Students
{
    public partial class TestCaseResultDialog : MetroWindow
    {
        public TestCaseResultDialog(Submission submission)
        {
            InitializeComponent();
            DataContext = submission;

            // Parse test case results from CompareResult
            if (!string.IsNullOrEmpty(submission.CompareResult))
            {
                var testCaseResults = ParseTestCaseResults(submission.CompareResult);
                TestCasesItemsControl.ItemsSource = testCaseResults;
            }
        }

        private List<TestCaseResultItem> ParseTestCaseResults(string compareResult)
        {
            var results = new List<TestCaseResultItem>();

            for (int i = 0; i < compareResult.Length; i++)
            {
                var statusCode = compareResult[i];
                var result = new TestCaseResultItem
                {
                    Number = i + 1,
                    StatusCode = statusCode
                };

                switch (statusCode)
                {
                    case '0':
                        result.StatusText = "Passed";
                        break;
                    case '1':
                        result.StatusText = "Time Limit Exceeded";
                        break;
                    case '2':
                        result.StatusText = "Memory Limit Exceeded";
                        break;
                    case '3':
                        result.StatusText = "Runtime Error";
                        break;
                    case '4':
                        result.StatusText = "Internal Error";
                        break;
                    case '5':
                        result.StatusText = "Wrong Answer";
                        break;
                    case '6':
                        result.StatusText = "Compilation Error";
                        break;
                    case '7':
                        result.StatusText = "Skipped";
                        break;
                    default:
                        result.StatusText = "Unknown";
                        break;
                }

                results.Add(result);
            }

            return results;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class TestCaseResultItem
    {
        public int Number { get; set; }
        public char StatusCode { get; set; }
        public string StatusText { get; set; } = string.Empty;
    }
}
