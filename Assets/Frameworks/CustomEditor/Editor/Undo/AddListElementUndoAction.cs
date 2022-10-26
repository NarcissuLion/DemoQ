using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.CustomEditor.Undo
{
	public class AddListElementUndoAction : BaseUndoAction
	{
		public IList list;
		public object element;
		public AddListElementUndoAction(IList list, object element)
		{
			this.list = list;
			this.element = element;
		}

		public override void Undo()
		{
			list.Remove(element);
			base.Undo();
		}

		public override void Redo()
		{
			list.Add(element);
			base.Redo();
		}

		public override int Merge(IUndoAction action)
		{
			return 0;
		}
	}
}