using System;

namespace AvaloniaEdit.Document
{
    public class LineCountChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the previous line count
        /// </summary>
        public int OldLineCount { get; private set; }

        /// <summary>
        /// Gets the new line count
        /// </summary>
        public int NewLineCount { get; private set; }

        public LineCountChangedEventArgs(int oldLineCount, int newLineCount)
        {
            OldLineCount = oldLineCount;
            NewLineCount = newLineCount;
        }
    }
}
