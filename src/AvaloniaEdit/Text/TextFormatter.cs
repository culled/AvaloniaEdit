namespace AvaloniaEdit.Text
{
    public class TextFormatter
    {
        public TextLine FormatLine(TextSource textSource, int firstCharIndex, double availableWidth, TextParagraphProperties paragraphProperties)
        {
            return TextLineImpl.Create(paragraphProperties, firstCharIndex, availableWidth, textSource);
        }
    }
}