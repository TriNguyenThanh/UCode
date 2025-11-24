using System.Windows;
using System.Windows.Controls;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class ProblemSolverPage : UserControl
    {
        public ProblemSolverPage(ProblemSolverViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void CopyInputButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string text)
            {
                try
                {
                    Clipboard.SetText(text);
                    // Optional: Show a brief notification
                    button.ToolTip = "Đã copy!";
                    
                    // Reset tooltip after 2 seconds
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = System.TimeSpan.FromSeconds(2)
                    };
                    timer.Tick += (s, args) =>
                    {
                        button.ToolTip = "Copy Input";
                        timer.Stop();
                    };
                    timer.Start();
                }
                catch
                {
                    // Silently fail if clipboard access is denied
                }
            }
        }

        private void CopyOutputButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string text)
            {
                try
                {
                    Clipboard.SetText(text);
                    // Optional: Show a brief notification
                    button.ToolTip = "Đã copy!";
                    
                    // Reset tooltip after 2 seconds
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = System.TimeSpan.FromSeconds(2)
                    };
                    timer.Tick += (s, args) =>
                    {
                        button.ToolTip = "Copy Output";
                        timer.Stop();
                    };
                    timer.Start();
                }
                catch
                {
                    // Silently fail if clipboard access is denied
                }
            }
        }
    }
}
