using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Noti;

namespace Framework.CustomEditor.Undo
{
    public class UndoManager
    {
        public static string NOTI_RECORD = "__undomgr_RECORD";
        public static string NOTI_UNDO = "__undomgr_UNDO";
        public static string NOTI_REDO = "__undomgr_REDO";

        private const int CAPACITY = 100;

        private IUndoAction[] s_undoList = new IUndoAction[CAPACITY];
        private Stack<IUndoAction> s_redoStack = new Stack<IUndoAction>(CAPACITY);
        private int m_undoHead = 0;

        public bool canUndo { get { return s_undoList[m_undoHead] != null; } }
        public bool canRedo { get { return s_redoStack.Count > 0; } }

        public void Record(IUndoAction undoAction)
        {
            PushUndo(undoAction);
            if (canRedo)
            {
                s_redoStack.Clear();
            }
            NotiCenter.DispatchStatic<IUndoAction>(NOTI_RECORD, undoAction);
        }

        private void PushUndo(IUndoAction undoAction)
        {
            if (canUndo)
            {
                int mergeResult = s_undoList[m_undoHead].Merge(undoAction);
                if (mergeResult == 0)
                {
                    m_undoHead = (m_undoHead + 1) % CAPACITY;
                    s_undoList[m_undoHead] = undoAction;
                    // Debug.Log("PUSH:" + undoAction.GetType());
                }
                else if (mergeResult == -1)
                {
                    PopUndo();
                }
            }
            else
            {
                s_undoList[m_undoHead] = undoAction;
            }
        }

        private void PopUndo()
        {
            s_undoList[m_undoHead] = null;
            m_undoHead = (m_undoHead - 1) < 0 ? CAPACITY - 1 : m_undoHead - 1;
        }

        public void Undo()
        {
            if (canUndo)
            {
                IUndoAction action = s_undoList[m_undoHead];
                action.Undo();
                s_redoStack.Push(action);

                PopUndo();

                NotiCenter.DispatchStatic<IUndoAction>(NOTI_UNDO, action);
                GUI.FocusControl(string.Empty);
            }
        }

        public void Redo()
        {
            if (canRedo)
            {
                IUndoAction action = s_redoStack.Pop();
                action.Redo();
                PushUndo(action);

                NotiCenter.DispatchStatic(NOTI_REDO, action);
                GUI.FocusControl(string.Empty);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < CAPACITY; i++)
            {
                s_undoList[i] = null;
            }
            s_redoStack.Clear();
            m_undoHead = 0;
        }
    }
}
