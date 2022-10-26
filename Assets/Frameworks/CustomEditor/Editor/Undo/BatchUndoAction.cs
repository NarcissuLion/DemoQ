using System.Collections.Generic;
using System;

namespace Framework.CustomEditor.Undo
{
    public class BatchUndoAction : BaseUndoAction
    {
        private List<IUndoAction> m_actions = new List<IUndoAction>();

        public int ActionCount { get { return m_actions.Count; } }

        public BatchUndoAction(params IUndoAction[] actions)
        {
            foreach (IUndoAction action in actions)
            {
                AddAction(action);
            }
        }

        public void AddAction(IUndoAction action)
        {
            if (!m_actions.Contains(action))
            {
                m_actions.Add(action);
            }
        }

        public void RemoveAction(IUndoAction action)
        {
            if (m_actions.Contains(action))
            {
                m_actions.Remove(action);
            }
        }

        public override void Undo()
        {
            for (int i = m_actions.Count - 1; i >= 0; i--)
            {
                m_actions[i].Undo();
            }
            base.Undo();
        }
        public override void Redo()
        {
            for (int i = 0; i < m_actions.Count; i++)
            {
                m_actions[i].Redo();
            }
            base.Redo();
        }

        public override int Merge(IUndoAction action)
		{
			if (!(action is BatchUndoAction))
			{
				return 0;
			}
			BatchUndoAction batchAction = (BatchUndoAction)action;
            if (batchAction.m_actions.Count != m_actions.Count)
            {
                return 0;
            }

            if (batchAction.m_actions.Count == 0)
            {
                return 0;
            }

            for (int i = 0; i < m_actions.Count; ++i)
            {
                Type actionAType = m_actions[i].GetType();
                Type actionBType = batchAction.m_actions[i].GetType();
                if (actionAType != actionBType || (actionAType != typeof(FieldUndoAction) && actionAType != typeof(PropertyUndoAction)))
                {
                    return 0;
                }
            }

            bool allSame = true;
            for (int i = 0; i < m_actions.Count; ++i)
            {
                if (m_actions[i] is FieldUndoAction)
                {
                    FieldUndoAction actionA = m_actions[i] as FieldUndoAction;
                    FieldUndoAction actionB = batchAction.m_actions[i] as FieldUndoAction;
                    if (actionA.target != actionB.target || actionA.fieldInfo != actionB.fieldInfo) return 0;
                    allSame &= actionA.origValue.IsSameAs(actionB.value);
                }
                else
                {
                    PropertyUndoAction actionA = m_actions[i] as PropertyUndoAction;
                    PropertyUndoAction actionB = batchAction.m_actions[i] as PropertyUndoAction;
                    if (actionA.target != actionB.target || actionA.propertyInfo != actionB.propertyInfo) return 0;
                    allSame &= actionA.origValue.IsSameAs(actionB.value);
                }
            }

            if (allSame) return -1;

            for (int i = 0; i < m_actions.Count; ++i)
            {
                if (m_actions[i] is FieldUndoAction)
                {
                    FieldUndoAction actionA = m_actions[i] as FieldUndoAction;
                    FieldUndoAction actionB = batchAction.m_actions[i] as FieldUndoAction;
                    actionA.value = actionB.value;
                }
                else
                {
                    PropertyUndoAction actionA = m_actions[i] as PropertyUndoAction;
                    PropertyUndoAction actionB = batchAction.m_actions[i] as PropertyUndoAction;
                    actionA.value = actionB.value;
                }
            }

            return 1;
		}
    }
}