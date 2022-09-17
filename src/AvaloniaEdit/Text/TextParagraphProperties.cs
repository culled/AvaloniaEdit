using Avalonia;
using Avalonia.Media;

namespace AvaloniaEdit.Text
{
    public sealed class TextParagraphProperties
    {
        public double DefaultIncrementalTab { get; set; }

        public bool FirstLineInParagraph { get; set; }

        public TextRunProperties DefaultTextRunProperties { get; set; }

        public TextWrapping TextWrapping { get; set; }

        public double Indent { get; set; }

        public Thickness Margins { get; set; }

        public double LineSpacingPercentage { get; set; }

        public Document.DocumentLineFormat.TextCasingMode TextCasing { get; set; }

        public TextParagraphProperties Clone()
		{
            return new TextParagraphProperties()
            {
                DefaultIncrementalTab = DefaultIncrementalTab,
                FirstLineInParagraph = FirstLineInParagraph,
                DefaultTextRunProperties = DefaultTextRunProperties.Clone(),
                TextWrapping = TextWrapping,
                Indent = Indent,
                Margins = Margins,
                LineSpacingPercentage = LineSpacingPercentage,
                TextCasing = TextCasing
            };
		}
    }
}
