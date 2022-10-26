using System.Reflection;
using UnityEditor;
namespace Framework.CustomEditor.Undo
{
	public class PropertyUndoAction : BaseUndoAction
	{
		public object target;
		public PropertyInfo propertyInfo;
		public object value;
		public object origValue;

		public PropertyUndoAction(object target, PropertyInfo propertyInfo, object origValue, object value)
		{
			this.target = target;
			this.propertyInfo = propertyInfo;
			this.origValue = origValue;
			this.value = value;
		}

		public override void Undo()
		{
			propertyInfo.SetValue(target, origValue, null);
			base.Undo();
		}

		public override void Redo()
		{
			propertyInfo.SetValue(target, value, null);
			base.Redo();
		}

		public override int Merge(IUndoAction action)
		{
			if (!(action is PropertyUndoAction))
			{
				return 0;
			}
			PropertyUndoAction propertyAction = (PropertyUndoAction)action;
			if (propertyAction.target != target || propertyAction.propertyInfo != propertyInfo)
			{
				return 0;
			}
			if (origValue.IsSameAs(propertyAction.value))
			{
				return -1;
			}
			else
			{
				value = propertyAction.value;
				return 1;
			}
		}
	}
}