using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using UCode.Desktop.Helpers;
using UCode.Desktop.ViewModels;

namespace UCode.Desktop.Pages
{
    public partial class ProblemSolverPage : UserControl
    {
        private CodeEditorHelper _editorHelper;

        public ProblemSolverPage(ProblemSolverViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            // Initialize editor helper after the control is loaded
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Get the TextEditor control from XAML
            if (FindName("CodeEditor") is TextEditor editor && _editorHelper == null)
            {
                _editorHelper = new CodeEditorHelper(editor);
                
                // Hook up to ViewModel if available
                if (DataContext is ProblemSolverViewModel viewModel)
                {
                    viewModel.EditorHelper = _editorHelper;
                }
                
                // Ensure editor gets focus to handle keyboard events properly
                editor.Focus();
                
                // Handle mouse click to ensure focus
                editor.PreviewMouseDown += (s, args) =>
                {
                    if (!editor.IsFocused)
                    {
                        editor.Focus();
                    }
                };
            }
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
