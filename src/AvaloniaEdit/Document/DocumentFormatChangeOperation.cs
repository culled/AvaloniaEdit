using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AvaloniaEdit.Document
{
    internal sealed class DocumentFormatChangeOperation : IUndoableOperationWithContext
    {
        private TextDocument _document;
        private DocumentFormatChangeEventArgs.DocumentLineFormatChange[] _formatChanges;

        public DocumentFormatChangeOperation(TextDocument document, DocumentFormatChangeEventArgs e)
        {
            _document = document;
            _formatChanges = e.LineFormatChanges;
        }

        public void Undo(UndoStack stack)
        {
            Debug.Assert(stack.State == UndoStack.StatePlayback);
            stack.RegisterAffectedDocument(_document);
            stack.State = UndoStack.StatePlaybackModifyDocument;
            Undo();
            stack.State = UndoStack.StatePlayback;
        }

        public void Redo(UndoStack stack)
        {
            Debug.Assert(stack.State == UndoStack.StatePlayback);
            stack.RegisterAffectedDocument(_document);
            stack.State = UndoStack.StatePlaybackModifyDocument;
            Redo();
            stack.State = UndoStack.StatePlayback;
        }

        public void Undo()
        {
            //_document.SetLineFormat(_oldFormat, _lineNumber);
            _document.SetLineFormatsFromChangeList(_formatChanges, false);
        }

        public void Redo()
        {
            _document.SetLineFormatsFromChangeList(_formatChanges, true);
            //_document.SetLineFormat(_newFormat, _lineNumber);
        }
    }
}
