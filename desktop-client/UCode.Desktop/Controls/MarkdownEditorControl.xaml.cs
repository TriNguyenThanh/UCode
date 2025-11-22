using System;
using System.Windows;
using System.Windows.Controls;

namespace UCode.Desktop.Controls
{
    public partial class MarkdownEditorControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(string),
                typeof(MarkdownEditorControl),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged));

        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register(
                nameof(Placeholder),
                typeof(string),
                typeof(MarkdownEditorControl),
                new PropertyMetadata("Nhập nội dung markdown..."));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        private bool _isUpdatingText;

        public MarkdownEditorControl()
        {
            InitializeComponent();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownEditorControl control && !control._isUpdatingText)
            {
                control.EditorTextBox.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUpdatingText)
            {
                _isUpdatingText = true;
                Value = EditorTextBox.Text;
                _isUpdatingText = false;
            }
        }

        private void InsertMarkdown(string before, string after = "", bool newLine = false)
        {
            var textBox = EditorTextBox;
            var selectionStart = textBox.SelectionStart;
            var selectionLength = textBox.SelectionLength;
            var text = textBox.Text;

            string selectedText = selectionLength > 0 
                ? text.Substring(selectionStart, selectionLength) 
                : "";

            string insertion = newLine 
                ? $"\n{before}{selectedText}{after}\n"
                : $"{before}{selectedText}{after}";

            var newText = text.Substring(0, selectionStart) + insertion + text.Substring(selectionStart + selectionLength);
            
            textBox.Text = newText;
            textBox.SelectionStart = selectionStart + before.Length;
            textBox.SelectionLength = selectedText.Length;
            textBox.Focus();
        }

        private void OnBoldClick(object sender, RoutedEventArgs e)
        {
            InsertMarkdown("**", "**");
        }

        private void OnItalicClick(object sender, RoutedEventArgs e)
        {
            InsertMarkdown("*", "*");
        }

        private void OnStrikethroughClick(object sender, RoutedEventArgs e)
        {
            InsertMarkdown("~~", "~~");
        }

        private void OnHeadingClick(object sender, RoutedEventArgs e)
        {
            InsertMarkdown("## ", "", true);
        }

        private void OnCodeClick(object sender, RoutedEventArgs e)
        {
            if (EditorTextBox.SelectionLength > 0)
            {
                InsertMarkdown("```\n", "\n```", true);
            }
            else
            {
                InsertMarkdown("`", "`");
            }
        }

        private void OnQuoteClick(object sender, RoutedEventArgs e)
        {
            InsertMarkdown("> ", "", true);
        }

        private void OnUnorderedListClick(object sender, RoutedEventArgs e)
        {
            InsertMarkdown("- ", "", true);
        }

        private void OnOrderedListClick(object sender, RoutedEventArgs e)
        {
            InsertMarkdown("1. ", "", true);
        }

        private void OnImageClick(object sender, RoutedEventArgs e)
        {
            var dialog = new ImageUploadDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                var markdown = $"![{dialog.AltText}]({dialog.ImageUrl})";
                InsertMarkdown(markdown, "", true);
            }
        }

        private void OnLinkClick(object sender, RoutedEventArgs e)
        {
            InsertMarkdown("[", "](url)");
        }

        private void OnPdfClick(object sender, RoutedEventArgs e)
        {
            var dialog = new PdfUploadDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                var markdown = $"[{dialog.LinkText}]({dialog.PdfUrl})";
                InsertMarkdown(markdown, "", true);
            }
        }
    }
}
