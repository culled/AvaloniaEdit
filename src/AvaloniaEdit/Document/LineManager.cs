﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AvaloniaEdit.Document
{
    /// <summary>
    /// Creates/Deletes lines when text is inserted/removed.
    /// </summary>
    internal sealed class LineManager
    {
        #region Constructor

        private readonly TextDocument _document;
        private readonly DocumentLineTree _documentLineTree;

        /// <summary>
        /// A copy of the line trackers. We need a copy so that line trackers may remove themselves
        /// while being notified (used e.g. by WeakLineTracker)
        /// </summary>
        private ILineTracker[] _lineTrackers;

        internal void UpdateListOfLineTrackers()
        {
            _lineTrackers = _document.LineTrackers.ToArray();
        }

        public LineManager(DocumentLineTree documentLineTree, TextDocument document)
        {
            _document = document;
            _documentLineTree = documentLineTree;
            UpdateListOfLineTrackers();

            Rebuild();
        }
        #endregion

        #region Change events
        /*
		HashSet<DocumentLine> deletedLines = new HashSet<DocumentLine>();
		readonly HashSet<DocumentLine> changedLines = new HashSet<DocumentLine>();
		HashSet<DocumentLine> deletedOrChangedLines = new HashSet<DocumentLine>();
		
		/// <summary>
		/// Gets the list of lines deleted since the last RetrieveChangedLines() call.
		/// The returned list is unsorted.
		/// </summary>
		public ICollection<DocumentLine> RetrieveDeletedLines()
		{
			var r = deletedLines;
			deletedLines = new HashSet<DocumentLine>();
			return r;
		}
		
		/// <summary>
		/// Gets the list of lines changed since the last RetrieveChangedLines() call.
		/// The returned list is sorted by line number and does not contain deleted lines.
		/// </summary>
		public List<DocumentLine> RetrieveChangedLines()
		{
			var list = (from line in changedLines
			            where !line.IsDeleted
			            let number = line.LineNumber
			            orderby number
			            select line).ToList();
			changedLines.Clear();
			return list;
		}
		
		/// <summary>
		/// Gets the list of lines changed since the last RetrieveDeletedOrChangedLines() call.
		/// The returned list is not sorted.
		/// </summary>
		public ICollection<DocumentLine> RetrieveDeletedOrChangedLines()
		{
			var r = deletedOrChangedLines;
			deletedOrChangedLines = new HashSet<DocumentLine>();
			return r;
		}
		 */
        #endregion

        #region Rebuild
        public void Rebuild()
        {
            // keep the first document line
            var ls = _documentLineTree.GetByNumber(1);
            // but mark all other lines as deleted, and detach them from the other nodes
            for (var line = ls.NextLine; line != null; line = line.NextLine)
            {
                line.IsDeleted = true;
                line.Parent = line.Left = line.Right = null;
            }
            // Reset the first line to detach it from the deleted lines
            ls.ResetLine();
            var ds = NewLineFinder.NextNewLine(_document, 0);
            var lines = new List<DocumentLine>();
            var lastDelimiterEnd = 0;
            while (ds != SimpleSegment.Invalid)
            {
                ls.TotalLength = ds.Offset + ds.Length - lastDelimiterEnd;
                ls.DelimiterLength = ds.Length;
                lastDelimiterEnd = ds.Offset + ds.Length;
                lines.Add(ls);

                ls = new DocumentLine(_document);
                ds = NewLineFinder.NextNewLine(_document, lastDelimiterEnd);
            }
            ls.TotalLength = _document.TextLength - lastDelimiterEnd;
            lines.Add(ls);
            _documentLineTree.RebuildTree(lines);
            foreach (var lineTracker in _lineTrackers)
                lineTracker.RebuildDocument();
        }
        #endregion

        #region Remove
        public void Remove(int offset, int length)
        {
            Debug.Assert(length >= 0);
            if (length == 0) return;
            var startLine = _documentLineTree.GetByOffset(offset);
            var startLineOffset = startLine.Offset;

            Debug.Assert(offset < startLineOffset + startLine.TotalLength);
            if (offset > startLineOffset + startLine.Length)
            {
                Debug.Assert(startLine.DelimiterLength == 2);
                // we are deleting starting in the middle of a delimiter

                // remove last delimiter part
                SetLineLength(startLine, startLine.TotalLength - 1);
                // remove remaining text
                Remove(offset, length - 1);
                return;
            }

            if (offset + length < startLineOffset + startLine.TotalLength)
            {
                // just removing a part of this line
                //startLine.RemovedLinePart(ref deferredEventList, offset - startLineOffset, length);
                SetLineLength(startLine, startLine.TotalLength - length);
                return;
            }
            // merge startLine with another line because startLine's delimiter was deleted
            // possibly remove lines in between if multiple delimiters were deleted
            var charactersRemovedInStartLine = startLineOffset + startLine.TotalLength - offset;
            Debug.Assert(charactersRemovedInStartLine > 0);
            //startLine.RemovedLinePart(ref deferredEventList, offset - startLineOffset, charactersRemovedInStartLine);


            var endLine = _documentLineTree.GetByOffset(offset + length);
            if (endLine == startLine)
            {
                // special case: we are removing a part of the last line up to the
                // end of the document
                SetLineLength(startLine, startLine.TotalLength - length);
                return;
            }
            var endLineOffset = endLine.Offset;
            var charactersLeftInEndLine = endLineOffset + endLine.TotalLength - (offset + length);
            //endLine.RemovedLinePart(ref deferredEventList, 0, endLine.TotalLength - charactersLeftInEndLine);
            //startLine.MergedWith(endLine, offset - startLineOffset);

            if (charactersRemovedInStartLine == startLine.TotalLength)
                startLine.LineFormat = endLine.LineFormat;

            // remove all lines between startLine (excl.) and endLine (incl.)
            var tmp = startLine.NextLine;
            DocumentLine lineToRemove;
            do
            {
                lineToRemove = tmp;
                tmp = tmp.NextLine;
                RemoveLine(lineToRemove);
            } while (lineToRemove != endLine);

            SetLineLength(startLine, startLine.TotalLength - charactersRemovedInStartLine + charactersLeftInEndLine);
        }

        private void RemoveLine(DocumentLine lineToRemove)
        {
            foreach (var lt in _lineTrackers)
                lt.BeforeRemoveLine(lineToRemove);
            _documentLineTree.RemoveLine(lineToRemove);
            //			foreach (ILineTracker lt in lineTracker)
            //				lt.AfterRemoveLine(lineToRemove);
            //			deletedLines.Add(lineToRemove);
            //			deletedOrChangedLines.Add(lineToRemove);
        }

        #endregion

        #region Insert
        public void Insert(int offset, ITextSource text)
        {
            var line = _documentLineTree.GetByOffset(offset);
            var lineOffset = line.Offset;

            Debug.Assert(offset <= lineOffset + line.TotalLength);
            if (offset > lineOffset + line.Length)
            {
                Debug.Assert(line.DelimiterLength == 2);
                // we are inserting in the middle of a delimiter

                // shorten line
                SetLineLength(line, line.TotalLength - 1);
                // add new line
                line = InsertLineAfter(line, 1);
                line = SetLineLength(line, 1);
            }

            var ds = NewLineFinder.NextNewLine(text, 0);
            if (ds == SimpleSegment.Invalid)
            {
                // no newline is being inserted, all text is inserted in a single line
                //line.InsertedLinePart(offset - line.Offset, text.Length);
                SetLineLength(line, line.TotalLength + text.TextLength);
                return;
            }
            //DocumentLine firstLine = line;
            //firstLine.InsertedLinePart(offset - firstLine.Offset, ds.Offset);
            var lastDelimiterEnd = 0;
            while (ds != SimpleSegment.Invalid)
            {
                // split line segment at line delimiter
                var lineBreakOffset = offset + ds.Offset + ds.Length;
                lineOffset = line.Offset;
                var lengthAfterInsertionPos = lineOffset + line.TotalLength - (offset + lastDelimiterEnd);
                line = SetLineLength(line, lineBreakOffset - lineOffset);
                var newLine = InsertLineAfter(line, lengthAfterInsertionPos);
                newLine = SetLineLength(newLine, lengthAfterInsertionPos);

                line = newLine;
                lastDelimiterEnd = ds.Offset + ds.Length;

                ds = NewLineFinder.NextNewLine(text, lastDelimiterEnd);
            }
            //firstLine.SplitTo(line);
            // insert rest after last delimiter
            if (lastDelimiterEnd != text.TextLength)
            {
                //line.InsertedLinePart(0, text.Length - lastDelimiterEnd);
                SetLineLength(line, line.TotalLength + text.TextLength - lastDelimiterEnd);
            }
        }

        private DocumentLine InsertLineAfter(DocumentLine line, int length)
        {
            var newLine = _documentLineTree.InsertLineAfter(line, length);
            foreach (var lt in _lineTrackers)
                lt.LineInserted(line, newLine);
            return newLine;
        }
        #endregion

        #region SetLineLength
        /// <summary>
        /// Sets the total line length and checks the delimiter.
        /// This method can cause line to be deleted when it contains a single '\n' character
        /// and the previous line ends with '\r'.
        /// </summary>
        /// <returns>Usually returns <paramref name="line"/>, but if line was deleted due to
        /// the "\r\n" merge, returns the previous line.</returns>
        private DocumentLine SetLineLength(DocumentLine line, int newTotalLength)
        {
            var delta = newTotalLength - line.TotalLength;
            if (delta != 0)
            {
                foreach (var lt in _lineTrackers)
                    lt.SetLineLength(line, newTotalLength);
                line.TotalLength = newTotalLength;
                DocumentLineTree.UpdateAfterChildrenChange(line);
            }
            // determine new DelimiterLength
            if (newTotalLength == 0)
            {
                line.DelimiterLength = 0;
            }
            else
            {
                var lineOffset = line.Offset;
                var lastChar = _document.GetCharAt(lineOffset + newTotalLength - 1);
                if (lastChar == '\r')
                {
                    line.DelimiterLength = 1;
                }
                else if (lastChar == '\n')
                {
                    if (newTotalLength >= 2 && _document.GetCharAt(lineOffset + newTotalLength - 2) == '\r')
                    {
                        line.DelimiterLength = 2;
                    }
                    else if (newTotalLength == 1 && lineOffset > 0 && _document.GetCharAt(lineOffset - 1) == '\r')
                    {
                        // we need to join this line with the previous line
                        var previousLine = line.PreviousLine;
                        RemoveLine(line);
                        return SetLineLength(previousLine, previousLine.TotalLength + 1);
                    }
                    else
                    {
                        line.DelimiterLength = 1;
                    }
                }
                else
                {
                    line.DelimiterLength = 0;
                }
            }
            return line;
        }
        #endregion

        #region ChangeComplete
        public void ChangeComplete(DocumentChangeEventArgs e)
        {
            foreach (var lt in _lineTrackers)
            {
                lt.ChangeComplete(e);
            }
        }
        #endregion
    }
}
