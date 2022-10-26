using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace Framework.CustomEditor
{
	public enum Ease
	{
		Linear,
		QuadIn,
		QuadOut,
		QuadInOut,
		QuartIn,
		QuartOut,
		QuartInOut,
	}

	public class Tweener
	{
		public float start;
		public float end;
		public float duration;
		public float timer;
		public Action<float> updateAction;
		public MethodInfo easeMethod;
		
		public static Tweener Create(float start, float end, float duration, Action<float> updateAction, Ease ease = Ease.Linear)
		{
			Tweener tweener = new Tweener();
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
				updateAction((float)easeMethod.Invoke(null, new object[4]{start, end - start, timer, duration}));
				return true;
			}
		}



		public static float Linear(float b, float c, float t, float d)
		{
			return c*t/d + b;
		}
		public static float QuadIn(float b, float c, float t, float d)
		{
			t /= d;
			return c*t*t + b;
        }
		public static float QuadOut(float b, float c, float t, float d)
		{
   			t /= d;
			return -c * t*(t-2) + b;
        }
		public static float QuadInOut(float b, float c, float t, float d)
		{
            t /= d/2;
			if (t < 1) return c/2*t*t + b;
			t--;
			return -c/2 * (t*(t-2) - 1) + b;
        }

		public static float QuartIn(float b, float c, float t, float d)
		{
			t /= d;
			return c*t*t*t*t + b;
        }
		public static float QuartOut(float b, float c, float t, float d)
		{
   			t /= d;
			t--;
			return -c * (t*t*t*t - 1) + b;
        }
		public static float QuartInOut(float b, float c, float t, float d)
		{
            t /= d/2;
			if (t < 1) return c/2*t*t*t*t + b;
			t -= 2;
			return -c/2 * (t*t*t*t - 2) + b;
        }
	}
}
