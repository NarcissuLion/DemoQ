using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.CustomEditor.Undo
{
	public class RemoveHashSetElementUndoAction<T> : BaseUndoAction
	{
		public HashSet<T> set;
		public T element;

		public RemoveHashSetElementUndoAction(HashSet<T> set, T element)
		{
			this.set = set;
			this.element = element;
		}

		public override void Undo()
		{
			set.Add(element);
			base.Undo();
		}

		public override void Redo()
		{
			set.Remove(element);
			base.Redo();
		}

		public override int Merge(IUndoAction action)
		{
			return 0;
		}
	}
}