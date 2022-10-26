using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.CustomEditor.Undo
{
    public class BaseUndoAction : IUndoAction
    {
        public event System.Action undoCallback;
        public event System.Action redoCallback;

        public virtual void Undo()
        {
            if (undoCallback != null) undoCallback();
        }
        public virtual void Redo()
        {
            if (redoCallback != null) redoCallback();
        }

        public virtual int Merge(IUndoAction action)
        {
            return 0;
        }
    }
}