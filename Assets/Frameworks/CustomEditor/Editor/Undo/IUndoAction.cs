using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.CustomEditor.Undo
{
    public interface IUndoAction
    {
        void Undo();
        void Redo();

        int Merge(IUndoAction action);
    }
}