using System;
using System.Collections.Generic;
using System.Text;

namespace AvaloniaEdit.Document
{
    public class DocumentFormatChangeEventArgs : EventArgs
    {
        public struct DocumentLineFormatChange
        {
            public int LineNumber { get; }
            public DocumentLineFormat OldFormat { get; }
            public DocumentLineFormat NewFormat { get; }

            public DocumentLineFormatChange(int lineNumber, DocumentLineFormat oldFormat, DocumentLineFormat newFormat)
            {
                LineNumber = lineNumber;
                OldFormat = oldFormat;
                NewFormat = newFormat;
            }
        }

        public DocumentLineFormatChange[] LineFormatChanges { get; }
        public int StartOffset { get; }
        public int Length { get; }

        public DocumentFormatChangeEventArgs(List<DocumentLineFormatChange> lineFormatChanges, int startOffset, int length)
        {
            LineFormatChanges = lineFormatChanges.ToArray();
            StartOffset = startOffset;
            Length = length;
        }
    }
}
