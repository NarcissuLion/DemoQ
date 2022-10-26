using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace Framework.CustomEditor
{
	public class TweenerVec2
	{
		public Vector2 start;
		public Vector2 end;
		public float duration;
		public float timer;
		public Action<Vector2> updateAction;
		public MethodInfo easeMethod;
		
		public static TweenerVec2 Create(Vector2 start, Vector2 end, float duration, Action<Vector2> updateAction, Ease ease = Ease.Linear)
		{
			TweenerVec2 tweener = new TweenerVec2();
			tweener.start = start;
			tweener.end = end;
			tweener.duration = duration;
			tweener.timer = 0f;
			tweener.updateAction = updateAction;
			tweener.easeMethod = typeof(Tweener).GetMethod(ease.ToString(), BindingFlags.Public | BindingFlags.Static);

			return tweener;
		}

		public bool isDone
		{
			get { return timer >= duration; }
		}

		public bool Update(float deltaTime)
		{
			timer += deltaTime;
			if (timer >= duration)
			{
				updateAction(end);
				return false;
			}
			else
			{
				updateAction(new Vector2((float)easeMethod.Invoke(null, new object[4]{start.x, end.x - start.x, timer, duration}), (float)easeMethod.Invoke(null, new object[4]{start.y, end.y - start.y, timer, duration})));
				return true;
			}
		}
	}
}
