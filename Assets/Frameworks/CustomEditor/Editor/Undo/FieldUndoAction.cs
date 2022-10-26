using System.Reflection;
using UnityEditor;

namespace Framework.CustomEditor.Undo
{
	public class FieldUndoAction : BaseUndoAction
	{
		public object target;
		public FieldInfo fieldInfo;
		public object value;
		public object origValue;

		public FieldUndoAction(object target, FieldInfo fieldInfo, object origValue, object value)
		{
			this.target = target;
			this.fieldInfo = fieldInfo;
			this.origValue = origValue;
			this.value = value;
		}

		public override void Undo()
		{
			fieldInfo.SetValue(target, origValue);
			base.Undo();
		}

		public override void Redo()
		{
			fieldInfo.SetValue(target, value);
			base.Redo();
		}


		public override int Merge(IUndoAction action)
		{
			if (!(action is FieldUndoAction))
			{
				return 0;
			}
			FieldUndoAction fieldAction = (FieldUndoAction)action;
			if (fieldAction.target != target || fieldAction.fieldInfo != fieldInfo)
			{
				return 0;
			}
			if (origValue.IsSameAs(fieldAction.value))
			{
				return -1;
			}
			else
			{
				value = fieldAction.value;
				return 1;
			}
		}
	}
}