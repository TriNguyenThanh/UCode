using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Block = Markdig.Syntax.Block; // Alias to avoid ambiguous reference

namespace UCode.Desktop.Controls
{
    /// <summary>
    /// Lightweight Markdown renderer using Markdig.
    /// Inherits from StackPanel to avoid internal scrolling.
    /// </summary>
    public class MarkdownViewer : StackPanel
    {
        public static readonly DependencyProperty MarkdownProperty =
            DependencyProperty.Register(nameof(Markdown), typeof(string), typeof(MarkdownViewer),
                new PropertyMetadata(string.Empty, OnMarkdownChanged));

        public string Markdown
        {
            get => (string)GetValue(MarkdownProperty);
            set => SetValue(MarkdownProperty, value);
        }

        public MarkdownViewer()
        {
            // Appearance defaults
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
        }

        private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MarkdownViewer viewer)
            {
                viewer.RenderMarkdown(e.NewValue as string);
            }
        }

        private void RenderMarkdown(string markdown)
        {
            Children.Clear();

            if (string.IsNullOrWhiteSpace(markdown))
                return;

            try
            {
                // Parse markdown using Markdig
                var pipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Build();

                var document = Markdig.Markdown.Parse(markdown, pipeline);

                foreach (var block in document)
                {
                    var control = RenderBlock(block);
                    if (control != null)
                    {
                        Children.Add(control);
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback for errors
                Children.Add(new TextBlock
                {
                    Text = markdown,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Red
                });
                Debug.WriteLine($"Markdown rendering error: {ex.Message}");
            }
        }

        private FrameworkElement RenderBlock(Block block)
        {
            switch (block)
            {
                case HeadingBlock heading:
                    return CreateHeader(heading);

                case ParagraphBlock paragraph:
                    return CreateParagraph(paragraph);

                case FencedCodeBlock codeBlock:
                    return CreateCodeBlock(codeBlock);

                case QuoteBlock quote:
                    return CreateQuoteBlock(quote);

                case ListBlock list:
                    return CreateListBlock(list);

                case ThematicBreakBlock:
                    return new Separator { Margin = new Thickness(0, 10, 0, 10) };

                default:
                    return null;
            }
        }

        private FrameworkElement CreateHeader(HeadingBlock heading)
        {
            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 12, 0, 8),
                FontFamily = new FontFamily("Segoe UI")
            };

            // Set size based on header level
            textBlock.FontSize = heading.Level switch
            {
                1 => 24,
                2 => 20,
                3 => 18,
                4 => 16,
                _ => 14
            };

            ProcessInlines(textBlock, heading.Inline);
            return textBlock;
        }

        private FrameworkElement CreateParagraph(ParagraphBlock paragraph)
        {
            // Check if paragraph contains ONLY an image
            if (paragraph.Inline.FirstChild is LinkInline link && link.IsImage && paragraph.Inline.FirstChild.NextSibling == null)
            {
                return CreateImage(link);
            }

            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12),
                FontSize = 14,
                LineHeight = 22,
                FontFamily = new FontFamily("Segoe UI"),
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)) // #333
            };

            ProcessInlines(textBlock, paragraph.Inline);
            return textBlock;
        }

        private FrameworkElement CreateCodeBlock(FencedCodeBlock codeBlock)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 16)
            };

            var text = new System.Text.StringBuilder();
            if (codeBlock.Lines.Count > 0)
            {
                // FencedCodeBlock stores lines in a specialized StringLineGroup
                // We iterate manually
                foreach (var line in codeBlock.Lines)
                {
                    text.AppendLine(line.ToString());
                }
            }

            var textBlock = new TextBlock
            {
                Text = text.ToString().TrimEnd(),
                FontFamily = new FontFamily("Consolas, Courier New"),
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(220, 220, 220))
            };

            border.Child = textBlock;
            return border;
        }

        private FrameworkElement CreateQuoteBlock(QuoteBlock quote)
        {
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(16, 0, 0, 16)
            };

            var border = new Border
            {
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(4, 0, 0, 0),
                Child = stackPanel
            };

            foreach (var block in quote)
            {
                var child = RenderBlock(block);
                if (child != null)
                {
                    if (child is TextBlock tb) tb.FontStyle = FontStyles.Italic;
                    stackPanel.Children.Add(child);
                }
            }

            return border;
        }

        private FrameworkElement CreateListBlock(ListBlock list)
        {
            var stackPanel = new StackPanel
            {
                Margin = new Thickness(16, 0, 0, 16)
            };

            int index = 1;
            foreach (var item in list)
            {
                if (item is ListItemBlock listItem)
                {
                    var grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    // Bullet or Number
                    var bullet = new TextBlock
                    {
                        Text = list.IsOrdered ? $"{index}." : "•",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 8, 4)
                    };
                    Grid.SetColumn(bullet, 0);
                    grid.Children.Add(bullet);

                    // Content
                    var contentPanel = new StackPanel();
                    foreach (var subBlock in listItem)
                    {
                        var control = RenderBlock(subBlock);
                        if (control != null)
                        {
                            // Strip bottom margin for compactness
                            if (control is TextBlock tb) tb.Margin = new Thickness(0, 0, 0, 4);
                            contentPanel.Children.Add(control);
                        }
                    }
                    Grid.SetColumn(contentPanel, 1);
                    grid.Children.Add(contentPanel);

                    stackPanel.Children.Add(grid);
                    index++;
                }
            }

            return stackPanel;
        }

        private FrameworkElement CreateImage(LinkInline imageLink)
        {
            try
            {
                var url = imageLink.Url;
                var img = new Image
                {
                    Stretch = Stretch.Uniform,
                    MaxWidth = 600,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 8, 0, 16)
                };

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load fully to prevent file locking
                bitmap.EndInit();

                img.Source = bitmap;
                return img;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load image: {ex.Message}");
                return new TextBlock { Text = $"[Image: {imageLink.Url}]", Foreground = Brushes.Red };
            }
        }

        private void ProcessInlines(TextBlock textBlock, ContainerInline inline)
        {
            if (inline == null) return;

            foreach (var item in inline)
            {
                switch (item)
                {
                    case LiteralInline literal:
                        textBlock.Inlines.Add(new Run(literal.Content.ToString()));
                        break;

                    case EmphasisInline emphasis:
                        var span = new Span();
                        if (emphasis.DelimiterCount == 2)
                            span.FontWeight = FontWeights.Bold;
                        else
                            span.FontStyle = FontStyles.Italic;

                        // Create a temporary TextBlock to process children, then move them
                        var tempTb = new TextBlock();
                        ProcessInlines(tempTb, emphasis);
                        while (tempTb.Inlines.Count > 0)
                        {
                            var child = tempTb.Inlines.FirstInline;
                            tempTb.Inlines.Remove(child);
                            span.Inlines.Add(child);
                        }
                        textBlock.Inlines.Add(span);
                        break;

                    case LinkInline link:
                        if (link.IsImage)
                        {
                            // Inline images are tricky in TextBlock, showing alt text for now
                            // Or use InlineUIContainer if we really want to embed
                            textBlock.Inlines.Add(new Run($"[Image: {link.Url}]"));
                        }
                        else
                        {
                            var hyperlink = new Hyperlink
                            {
                                NavigateUri = new Uri(link.Url, UriKind.RelativeOrAbsolute),
                                Foreground = new SolidColorBrush(Color.FromRgb(0, 39, 94))
                            };

                            // Process link text
                            var linkTb = new TextBlock();
                            ProcessInlines(linkTb, link);
                            while (linkTb.Inlines.Count > 0)
                            {
                                var child = linkTb.Inlines.FirstInline;
                                linkTb.Inlines.Remove(child);
                                hyperlink.Inlines.Add(child);
                            }

                            hyperlink.RequestNavigate += (s, e) =>
                            {
                                try
                                {
                                    Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                                }
                                catch { }
                            };
                            textBlock.Inlines.Add(hyperlink);
                        }
                        break;

                    case CodeInline code:
                        textBlock.Inlines.Add(new Run(code.Content)
                        {
                            FontFamily = new FontFamily("Consolas"),
                            Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
                            Foreground = new SolidColorBrush(Color.FromRgb(200, 50, 50))
                        });
                        break;

                    case LineBreakInline:
                        textBlock.Inlines.Add(new LineBreak());
                        break;
                }
            }
        }
    }
}
