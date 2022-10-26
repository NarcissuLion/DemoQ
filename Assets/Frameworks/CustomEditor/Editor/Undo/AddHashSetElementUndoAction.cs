using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.CustomEditor.Undo
{
	public class AddHashSetElementUndoAction<T> : BaseUndoAction
	{
		public HashSet<T> set;
		public T element;
		public AddHashSetElementUndoAction(HashSet<T> set, T element)
		{
			this.set = set;
			this.element = element;
		}

		public override void Undo()
		{
			set.Remove(element);
			base.Undo();
		}

		public override void Redo()
		{
			set.Add(element);
			base.Undo();
		}

		public override int Merge(IUndoAction action)
		{
			return 0;
		}
	}
}