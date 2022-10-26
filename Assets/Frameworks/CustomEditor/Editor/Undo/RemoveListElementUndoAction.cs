using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.CustomEditor.Undo
{
	public class RemoveListElementUndoAction : BaseUndoAction
	{
		public IList list;
		public object element;
		public int index;

		public RemoveListElementUndoAction(IList list, object element, int index)
		{
			this.list = list;
			this.element = element;
			this.index = index;
		}

		public override void Undo()
		{
			list.Insert(index, element);
			base.Undo();
		}

		public override void Redo()
		{
			list.Remove(element);
			base.Redo();
		}

		public override int Merge(IUndoAction action)
		{
			return 0;
		}
	}
}