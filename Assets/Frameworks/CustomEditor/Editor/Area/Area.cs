using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

namespace Framework.CustomEditor
{
	abstract public class Area
	{	
		protected int m_layer;  // 同一Parent下的child Area之间的层级关系，越大越靠上

		protected Area m_parent;
		protected List<Area> m_children = new List<Area>();
        protected bool m_ignoreChildrenDraw = false;
		protected bool m_ignoreChildrenInteraction = false;
		protected bool m_ignoreChildrenSceneGUI = false;

		public Area(int layer)
		{
			this.m_layer = layer;
		}

		public int layer
		{
			get { return m_layer; }
			set { m_layer = value; }
		}

		abstract public float width{get;}
		abstract public float height{get;}
		abstract public Vector2 relativePosition{get;}
		protected Vector2 m_relativePositionOffset;

		public bool isVisible = true;
		public bool isInteractable = true;
		public bool isIgnoreHoverTest = false;
		
		public Rect GetGlobalRect()
		{
			if (m_parent == null)
			{
				return GetRelativeRect();
			}
			else
			{
				Rect parentGlobalRect = m_parent.GetGlobalRect();
				return new Rect(relativePosition.x + parentGlobalRect.x, relativePosition.y + parentGlobalRect.y, width, height);
			}
		}

		public Rect GetRelativeRect()
		{
			return new Rect(relativePosition.x, relativePosition.y, width, height);
		}

		public Rect GetRelativeRect(Area refArea)
		{
			Rect refGlobalRect = refArea.GetGlobalRect();
			Rect selfGlobalRect = GetGlobalRect();
			return new Rect(selfGlobalRect.x - refGlobalRect.x, selfGlobalRect.y - refGlobalRect.y, width, height);
		}

		public Rect localRect{get{return new Rect(0f, 0f, width, height);}}

		public Area parent
		{
			get{ return m_parent; }
			set
			{
				if (m_parent != value)
				{
					if (m_parent != null)
					{
						m_parent.RemoveChild(this);
					}
					m_parent = value;
					if (m_parent != null)
					{
						m_parent.AddChild(this);
					}
				}
			}
		}

		public void AddChild(Area child)
		{
			if (!m_children.Contains(child))
			{
				if (GetChildByLayer(child.layer) != null)
				{
					Debug.Log(child.layer);
					throw new Exception("Area->AddChild Error! Duplicate layer => " + child.layer); 
				}
				m_children.Add(child);
				child.parent = this;
				child.OnBeAdded();
                SortChildrenAscending();
			}
		}

		public void RemoveChild(Area child)
		{
			if (m_children.Contains(child))
			{
				m_children.Remove(child);
				child.m_parent = null;
				child.OnBeRemoved();
			}
		}

		public int GetChildIdx(Area child)
		{
			return m_children.IndexOf(child);
		}

		public List<Area> GetChildByField(string fieldName1, BindingFlags flags1, string fieldName2, object fieldValue2, BindingFlags flags2)
		{
			List<Area> result = new List<Area>();
			foreach (Area child in m_children)
			{
				FieldInfo field1 = child.GetType().GetField(fieldName1, flags1);
				if (field1 != null)
				{
					object field1Value = field1.GetValue(child);
					FieldInfo field2 = field1Value.GetType().GetField(fieldName2, flags2);
					if (field2 != null && field2.GetValue(field1Value).Equals(fieldValue2))
					{
						result.Add(child);
					}
				}
				result.AddRange(child.GetChildByField(fieldName1, flags1, fieldName2, fieldValue2, flags2));
			}
			return result;
		}

		public List<Area> GetChildByField(string fieldName1, Type fieldType1, BindingFlags flags1)
		{
			List<Area> result = new List<Area>();
			foreach (Area child in m_children)
			{
				FieldInfo field1 = child.GetType().GetField(fieldName1, flags1);
				if (field1 != null)
				{
					object field1Value = field1.GetValue(child);
					if (field1Value.GetType() == fieldType1)
					{
						result.Add(child);
					}
				}
				result.AddRange(child.GetChildByField(fieldName1, fieldType1, flags1));
			}
			return result;
		}

		public int GetSelfIdx()
		{
			return parent == null ? 0 : parent.GetChildIdx(this);
		}

		protected virtual void OnBeAdded()
		{
		}

		protected virtual void OnBeRemoved()
		{
		}

		public int childCount { get { return m_children == null ? 0 : m_children.Count; } }

		public void RemoveAllChildren()
		{
			int count = childCount;
			for (int i = count - 1; i >= 0; i--)
			{
				RemoveChild(m_children[i]);
			}
		}

		public Area GetChild(int index)
		{
			return m_children[index];
		}

		public Area GetChildByLayer(int layer)
		{
			foreach (Area child in m_children)
			{
				if (child.layer == layer)
				{
					return child;
				}
			}
			return null;
		}
		
		public void SortChildrenAscending()
		{
			m_children.Sort((a,b) => a.layer.CompareTo(b.layer));
		}
		public void SortChildrenDescending()
		{
			m_children.Sort((a,b) => b.layer.CompareTo(a.layer));
		}


		public void Update(float deltaTime)
		{
			OnUpdate(deltaTime);
			foreach (Area child in m_children)
			{
				child.Update(deltaTime);
			}
			OnLateUpdate(deltaTime);
		}

		public virtual void OnUpdate(float deltaTime)
		{
		}

		public virtual void OnLateUpdate(float deltaTime)
		{
		}

        public void DrawGUI(Event evt)
        {
            if (!isVisible) { return; }
            //从下往上画
            OnDrawGUI(evt);
            if (!m_ignoreChildrenDraw)
            {
                for (int i = 0; i < m_children.Count; i++)
                {
                    m_children[i].DrawGUI(evt);
                }
            }
            OnPostDrawGUI(evt);
        }

		protected virtual void OnDrawGUI(Event evt)
		{
		}

		protected virtual void OnPostDrawGUI(Event evt)
		{
		}

        public virtual void DoInteraction(Event evt)
        {
            if (!isInteractable || !isVisible) { return; }
            OnPreDoInteraction(evt);
            //从上往下交互
            if (!m_ignoreChildrenInteraction)
            {
                for (int i = m_children.Count - 1; i > -1; i--)
                {
                    m_children[i].DoInteraction(evt);
                }
            }
            OnDoInteraction(evt);
        }

		protected virtual void OnPreDoInteraction(Event evt)
		{

		}

		protected virtual void OnDoInteraction(Event evt)
		{

		}

		public Area GetTopHoverArea(Vector2 globalMousePosi, params Type[] excludeTypes)
		{
			if (isIgnoreHoverTest) return null;
			if (excludeTypes != null)
			{
				foreach (Type excludeType in excludeTypes)
				{
					if (excludeType == GetType()) return null;
				}
			}
			for (int i = m_children.Count - 1; i > -1; i--)
			{
				Area area = m_children[i].GetTopHoverArea(globalMousePosi);
				if (area != null) return area;
			}
			if (GetGlobalRect().Contains(globalMousePosi)) return this;
			return null;
		}

		public virtual void OnSceneGUI(Event evt, SceneView sceneView)
		{
			if (!isVisible) { return; }
            OnDoSceneGUI(evt, sceneView);
            if (!m_ignoreChildrenSceneGUI)
            {
                for (int i = 0; i < m_children.Count; i++)
                {
                    m_children[i].OnSceneGUI(evt, sceneView);
                }
            }
		}

		protected virtual void OnDoSceneGUI(Event evt, SceneView sceneView)
		{

		}

        public virtual void Dispose()
        {
			if (m_children != null)
			{
				for (int i = m_children.Count - 1; i > -1; i--)
				{
					m_children[i].Dispose();
				}
			}
            m_children = null;
            m_parent = null;
        }
	}
}
