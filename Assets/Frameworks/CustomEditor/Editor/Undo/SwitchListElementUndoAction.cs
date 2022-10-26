using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.CustomEditor.Undo
{
	public class SwitchListElementUndoAction : BaseUndoAction
	{
		public IList list;
		public int index;
		public int origIndex;

		public SwitchListElementUndoAction(IList list, int index, int origIndex)
		{
			this.list = list;
			this.index = index;
			this.origIndex = origIndex;
		}

		public override void Undo()
		{
			object temp = list[index];
			list.Remove(temp);
			list.Insert(origIndex, temp);
			base.Undo();
		}

		public override void Redo()
		{
			object temp = list[origIndex]; 
			list.Remove(temp);
			list.Insert(index, temp);
			base.Redo();
		}

		public override int Merge(IUndoAction action)
		{
			return 0; 
		}
	}
}