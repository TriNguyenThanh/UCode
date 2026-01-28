using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace UCode.Desktop.Helpers
{
    /// <summary>
    /// Helper class for managing code editor with read-only regions (head and tail)
    /// Uses IReadOnlySectionProvider for robust read-only handling
    /// </summary>
    public class CodeEditorHelper
    {
        private readonly TextEditor _editor;
        private int _headLength;
        private int _tailLength;
        private CodeEditorReadOnlySectionProvider _readOnlyProvider;

        public CodeEditorHelper(TextEditor editor)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            SetupEditor();
        }

        private void SetupEditor()
        {
            // Visual feedback for read-only sections
            _editor.TextArea.TextView.BackgroundRenderers.Add(new ReadOnlyBackgroundRenderer(this));

            // Set default syntax highlighting (C++)
            SetSyntaxHighlighting("cpp");
        }

        /// <summary>
        /// Sets the syntax highlighting based on language code
        /// </summary>
        /// <param name="languageCode">Language code like: c, cpp, java, python, javascript, csharp</param>
        public void SetSyntaxHighlighting(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
                return;

            // Map common language codes to AvalonEdit highlighting definitions
            var highlightingName = languageCode.ToLower() switch
            {
                "c" => "C++",           // C uses C++ highlighting
                "cpp" or "c++" => "C++",
                "java" => "Java",
                "python" or "py" => "Python",
                "javascript" or "js" => "JavaScript",
                "csharp" or "cs" or "c#" => "C#",
                "php" => "PHP",
                "xml" => "XML",
                "html" => "HTML",
                "css" => "CSS",
                "sql" => "TSQL",
                "ruby" or "rb" => "Ruby",
                "vb" or "vbnet" => "VB",
                _ => "C++"              // Default to C++ for unknown languages
            };

            try
            {
                var highlighting = HighlightingManager.Instance.GetDefinition(highlightingName);
                if (highlighting != null)
                {
                    // Apply dark theme colors (VS Code Dark+ style)
                    ApplyDarkThemeColors(highlighting);
                    _editor.SyntaxHighlighting = highlighting;
                }
            }
            catch
            {
                // Silently fail if highlighting definition not found
            }
        }

        /// <summary>
        /// Applies dark theme colors to the highlighting definition (VS Code Dark+ style)
        /// </summary>
        private void ApplyDarkThemeColors(IHighlightingDefinition highlighting)
        {
            // VS Code Dark+ inspired colors
            var keywordColor = Color.FromRgb(86, 156, 214);      // #569CD6 - Blue keywords
            var controlKeywordColor = Color.FromRgb(197, 134, 192); // #C586C0 - Purple control keywords  
            var typeColor = Color.FromRgb(78, 201, 176);         // #4EC9B0 - Teal types/classes
            var stringColor = Color.FromRgb(206, 145, 120);      // #CE9178 - Orange strings
            var commentColor = Color.FromRgb(106, 153, 85);      // #6A9955 - Green comments
            var numberColor = Color.FromRgb(181, 206, 168);      // #B5CEA8 - Light green numbers
            var methodColor = Color.FromRgb(220, 220, 170);      // #DCDCAA - Yellow methods
            var preprocessorColor = Color.FromRgb(155, 155, 155); // #9B9B9B - Gray preprocessor
            var operatorColor = Color.FromRgb(212, 212, 212);    // #D4D4D4 - Light gray operators

            foreach (var color in highlighting.NamedHighlightingColors)
            {
                var name = color.Name.ToLower();

                // Keywords
                if (name.Contains("keyword") || name.Contains("keywords"))
                {
                    if (name.Contains("control") || name.Contains("flow"))
                    {
                        color.Foreground = new SimpleHighlightingBrush(controlKeywordColor);
                    }
                    else
                    {
                        color.Foreground = new SimpleHighlightingBrush(keywordColor);
                    }
                    color.FontWeight = FontWeights.Normal;
                }
                // Types and classes
                else if (name.Contains("type") || name.Contains("class") || name.Contains("namespace") || name.Contains("struct"))
                {
                    color.Foreground = new SimpleHighlightingBrush(typeColor);
                }
                // Strings and characters
                else if (name.Contains("string") || name.Contains("char") || name.Contains("literal"))
                {
                    color.Foreground = new SimpleHighlightingBrush(stringColor);
                }
                // Comments
                else if (name.Contains("comment"))
                {
                    color.Foreground = new SimpleHighlightingBrush(commentColor);
                    color.FontStyle = FontStyles.Italic;
                }
                // Numbers
                else if (name.Contains("number") || name.Contains("digit") || name.Contains("numeric"))
                {
                    color.Foreground = new SimpleHighlightingBrush(numberColor);
                }
                // Methods and functions
                else if (name.Contains("method") || name.Contains("function"))
                {
                    color.Foreground = new SimpleHighlightingBrush(methodColor);
                }
                // Preprocessor directives
                else if (name.Contains("preprocessor") || name.Contains("directive"))
                {
                    color.Foreground = new SimpleHighlightingBrush(preprocessorColor);
                }
                // Operators and punctuation
                else if (name.Contains("operator") || name.Contains("punctuation"))
                {
                    color.Foreground = new SimpleHighlightingBrush(operatorColor);
                }
            }
        }

        /// <summary>
        /// Sets up the code template with head, body, and tail sections
        /// </summary>
        public void SetCodeTemplate(string head, string body, string tail)
        {
            _headLength = head.Length;
            _tailLength = tail.Length;

            // Build full text
            var fullText = head + body + tail;
            _editor.Document.Text = fullText;

            // Create and set the read-only section provider
            _readOnlyProvider = new CodeEditorReadOnlySectionProvider(_editor.Document, _headLength, _tailLength);
            _editor.TextArea.ReadOnlySectionProvider = _readOnlyProvider;

            // Position cursor at start of body
            _editor.TextArea.Caret.Offset = _headLength;
            _editor.Focus();
        }

        /// <summary>
        /// Gets the editable start offset (after head)
        /// </summary>
        public int EditableStartOffset => _headLength;

        /// <summary>
        /// Gets the editable end offset (before tail)
        /// </summary>
        public int EditableEndOffset => _editor.Document.TextLength - _tailLength;

        /// <summary>
        /// Gets only the editable body content
        /// </summary>
        public string GetBodyContent()
        {
            if (_editor.Document.TextLength == 0)
                return string.Empty;

            var bodyLength = EditableEndOffset - EditableStartOffset;
            if (bodyLength <= 0)
                return string.Empty;

            // Ensure we don't go out of bounds
            if (EditableStartOffset + bodyLength > _editor.Document.TextLength)
                return string.Empty;

            return _editor.Document.GetText(EditableStartOffset, bodyLength);
        }

        /// <summary>
        /// Gets the full code including head, body, and tail
        /// </summary>
        public string GetFullCode()
        {
            return _editor.Document.Text;
        }

        /// <summary>
        /// Checks if an offset is in a read-only region
        /// </summary>
        public bool IsInReadOnlyRegion(int offset)
        {
            // Head region: [0, _headLength)
            if (offset < _headLength)
                return true;

            // Tail region: [document.Length - _tailLength, document.Length)
            if (offset >= _editor.Document.TextLength - _tailLength)
                return true;

            return false;
        }

        /// <summary>
        /// Gets the read-only sections for rendering
        /// </summary>
        public IEnumerable<ReadOnlySection> GetReadOnlySections()
        {
            var sections = new List<ReadOnlySection>();

            // Head region
            if (_headLength > 0)
            {
                sections.Add(new ReadOnlySection(0, _headLength));
            }

            // Tail region
            if (_tailLength > 0)
            {
                var tailStart = _editor.Document.TextLength - _tailLength;
                if (tailStart >= 0)
                {
                    sections.Add(new ReadOnlySection(tailStart, _tailLength));
                }
            }

            return sections;
        }

        /// <summary>
        /// Represents a read-only section in the document
        /// </summary>
        public class ReadOnlySection
        {
            public int StartOffset { get; }
            public int Length { get; }

            public ReadOnlySection(int startOffset, int length)
            {
                StartOffset = startOffset;
                Length = length;
            }
        }

        /// <summary>
        /// Read-only section provider that dynamically calculates read-only segments
        /// </summary>
        private class CodeEditorReadOnlySectionProvider : IReadOnlySectionProvider
        {
            private readonly TextDocument _document;
            private readonly int _headLength;
            private readonly int _tailLength;

            public CodeEditorReadOnlySectionProvider(TextDocument document, int headLength, int tailLength)
            {
                _document = document;
                _headLength = headLength;
                _tailLength = tailLength;
            }

            public bool CanInsert(int offset)
            {
                // Can only insert in the editable body region
                // Body region: [_headLength, document.Length - _tailLength]
                return offset >= _headLength && offset <= _document.TextLength - _tailLength;
            }

            public IEnumerable<ISegment> GetDeletableSegments(ISegment segment)
            {
                // Calculate the editable range
                int editableStart = _headLength;
                int editableEnd = _document.TextLength - _tailLength;

                if (editableEnd < editableStart)
                {
                    // No editable region
                    yield break;
                }

                // Calculate the intersection of the requested segment with the editable region
                int deleteStart = Math.Max(segment.Offset, editableStart);
                int deleteEnd = Math.Min(segment.EndOffset, editableEnd);

                if (deleteEnd > deleteStart)
                {
                    yield return new TextSegment { StartOffset = deleteStart, EndOffset = deleteEnd };
                }
            }
        }

        /// <summary>
        /// Background renderer to visually distinguish read-only sections
        /// </summary>
        private class ReadOnlyBackgroundRenderer : ICSharpCode.AvalonEdit.Rendering.IBackgroundRenderer
        {
            private readonly CodeEditorHelper _helper;
            private static readonly SolidColorBrush ReadOnlyBrush = new SolidColorBrush(Color.FromArgb(40, 128, 128, 128));

            public ReadOnlyBackgroundRenderer(CodeEditorHelper helper)
            {
                _helper = helper;
                ReadOnlyBrush.Freeze();
            }

            public ICSharpCode.AvalonEdit.Rendering.KnownLayer Layer =>
                ICSharpCode.AvalonEdit.Rendering.KnownLayer.Background;

            public void Draw(ICSharpCode.AvalonEdit.Rendering.TextView textView, System.Windows.Media.DrawingContext drawingContext)
            {
                if (textView == null || drawingContext == null)
                    return;

                foreach (var section in _helper.GetReadOnlySections())
                {
                    if (section.StartOffset >= textView.Document.TextLength)
                        continue;

                    var endOffset = Math.Min(section.StartOffset + section.Length, textView.Document.TextLength);
                    if (endOffset <= section.StartOffset)
                        continue;

                    var startLine = textView.Document.GetLineByOffset(section.StartOffset);
                    var endLine = textView.Document.GetLineByOffset(endOffset - 1);

                    for (int lineNumber = startLine.LineNumber; lineNumber <= endLine.LineNumber; lineNumber++)
                    {
                        var line = textView.Document.GetLineByNumber(lineNumber);
                        foreach (var rect in ICSharpCode.AvalonEdit.Rendering.BackgroundGeometryBuilder.GetRectsForSegment(textView, line))
                        {
                            drawingContext.DrawRectangle(ReadOnlyBrush, null, rect);
                        }
                    }
                }
            }
        }
    }
}
