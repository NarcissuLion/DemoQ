using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.CustomEditor.Undo
{
	public class SetListElementUndoAction : BaseUndoAction
	{
		public IList list;
		public object element;
		public object origElement;
		public int index;
		public SetListElementUndoAction(IList list, object origElement, object element, int index)
		{
			this.list = list;
			this.origElement = origElement;
			this.element = element;
			this.index = index;
		}

		public override void Undo()
		{
			list[index] = origElement;
			base.Undo();
		}

		public override void Redo()
		{
			list[index] = element;
			base.Redo();
		}

		public override int Merge(IUndoAction action)
		{
			return 0;
		}
	}
}