using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.CustomEditor
{
	abstract public class VerticalScrollArea : Area
	{
		public VerticalScrollArea(int layer) : base(layer)
		{
		}

		protected virtual void InitDatas(int dataCounts, float entryWidth, float entryHeight, Vector2 margin)
		{
			if (tweener != null)
			{
				tweener = null;
			}

			m_dataCount = dataCounts;
			m_entryWidth = entryWidth;
			m_entryHeight = entryHeight;
			m_margin = margin;

			m_amountPerLine = Mathf.Max(1, (int)((width/* - m_margin.x*/) / (m_entryWidth + m_margin.x)));
			m_lineCount = Mathf.CeilToInt(1f * m_dataCount / m_amountPerLine);

			m_datasHeight = m_lineCount * (m_entryHeight + m_margin.y) + m_margin.y;
			m_dragging = false;
			m_offsetDeltaQueue.Clear();

			m_offset = moreThanOneView ? Mathf.Clamp(m_offset, 0f, m_datasHeight - height) : 0f;
		}

		public void Resize()
		{
			m_amountPerLine = Mathf.Max(1, (int)((width/* - m_margin.x*/) / (m_entryWidth + m_margin.x)));
			m_lineCount = Mathf.CeilToInt(1f * m_dataCount / m_amountPerLine);

			m_datasHeight = m_lineCount * (m_entryHeight + m_margin.y) + m_margin.y;

			m_offset = moreThanOneView ? Mathf.Clamp(m_offset, 0f, m_datasHeight - height) : 0f;
		}

		protected bool moreThanOneView { get { return m_datasHeight > height; } }

		protected float m_offset;
		protected Queue<float> m_offsetDeltaQueue = new Queue<float>();
		protected float m_offsetInertia;
		protected int m_dataCount;
		protected float m_entryWidth;
		protected float m_entryHeight;
		protected Vector2 m_margin;
		protected int m_amountPerLine;
		protected int m_lineCount;
		protected float m_datasHeight;
		protected List<Rect> m_entryRects = new List<Rect>();

		protected override void OnDrawGUI(Event evt)
        {
			GUI.BeginGroup(GetGlobalRect());

			OnDrawBackground(evt);

			m_entryRects.Clear();

			int startLine = Mathf.Max(0, Mathf.FloorToInt(m_offset / (m_entryHeight + m_margin.y)));
			int endLine = Mathf.Min(m_lineCount - 1, Mathf.FloorToInt((m_offset + height) / (m_entryHeight + m_margin.y)));
			int startIdx = startLine * m_amountPerLine;
			int endIdx = Mathf.Min(m_dataCount - 1, (endLine + 1) * m_amountPerLine - 1);
			
			for (int i = startIdx; i <= endIdx; i++)
			{
				m_entryRects.Add(OnDrawEntry(evt, i));
			}

			if (m_dragging && m_offsetDeltaQueue.Count > 0)
			{
				if (m_enableMouseCursor) EditorGUIUtility.AddCursorRect(localRect, MouseCursor.Pan);
			}

			GUI.EndGroup();
		}

		protected virtual Rect GetItemRect(int index)
		{
			int col = index % m_amountPerLine;
			int row = index / m_amountPerLine;
			return new Rect(col * (m_entryWidth + m_margin.x) + m_margin.x, row * (m_entryHeight + m_margin.y) + m_margin.y - m_offset, m_entryWidth, m_entryHeight);
		}

		protected virtual Rect GetGlobalItemRect(int index)
		{
			Rect globalRect = GetGlobalRect();
			Rect itemRect = GetItemRect(index);
			itemRect.x += globalRect.x;
			itemRect.y += globalRect.y;
			return itemRect;
		}

		protected virtual void OnDrawBackground(Event evt) {}
		protected virtual Rect OnDrawEntry(Event evt, int index) { return GetItemRect(index); }
		protected virtual void OnSelectEntry(int index) {}
		protected virtual void OnHoverEntry(int index) {}
		protected virtual bool OnDoInteractionEntry(Event evt, int index) { return false; }

		protected bool m_enableMouseCursor;
		protected bool m_wheelDraggingOnly;

		protected bool m_dragging;
		protected Vector2 m_startMousePosi;
		protected Vector2 m_lastMousePosi;
		protected int m_hoverIndex = -1;
		protected override void OnDoInteraction(Event evt)
        {
			GUI.BeginGroup(GetGlobalRect());

			if (!m_dragging)
			{
				bool blockDrag = false;
				int hoverIndex = -1;
				foreach (Rect entryRect in m_entryRects)
				{
					if (entryRect.Contains(evt.mousePosition))
					{
						int startIdx = Mathf.Max(0, Mathf.FloorToInt(m_offset / m_entryHeight));
						hoverIndex = startIdx + m_entryRects.IndexOf(entryRect);
						if (m_enableMouseCursor) EditorGUIUtility.AddCursorRect(entryRect, MouseCursor.Link);
						blockDrag |= OnDoInteractionEntry(evt, hoverIndex);
					}
				}
				if (m_hoverIndex != hoverIndex)
				{
					m_hoverIndex = hoverIndex;
					OnHoverEntry(m_hoverIndex);
				}

				if (!blockDrag && !m_wheelDraggingOnly && evt.type == EventType.MouseDown && localRect.Contains(evt.mousePosition) && evt.button == 0)
				{// 处理点击事件 & 记录开始拖拽
					m_dragging = true;
					m_startMousePosi = evt.mousePosition;
					m_lastMousePosi = evt.mousePosition;
					m_offsetDeltaQueue.Clear();
					evt.Use();
				}

				if (evt.type == EventType.ScrollWheel && localRect.Contains(evt.mousePosition))
				{
					if (moreThanOneView)  // 数据超过一屏
					{
						m_offset += evt.delta.y * 12f;
						m_offset = Mathf.Clamp(m_offset, 0f, m_datasHeight - height);
					}
					evt.Use();
				}
			}
			else
			{
				if (evt.rawType == EventType.MouseUp && evt.button == 0)
				{// 释放鼠标，处理列表归位
					if (m_hoverIndex > -1 && Vector2.Distance(m_startMousePosi, evt.mousePosition) < 4)
					{
						OnSelectEntry(m_hoverIndex);
					}

					m_dragging = false;
					if (moreThanOneView)  // 数据超过一屏
					{
						if (m_offset < 0) tweener = Tweener.Create(m_offset, 0f, 0.2f, (y)=>m_offset=y, Ease.QuartOut);
						else if (m_offset > m_datasHeight - height) tweener = Tweener.Create(m_offset, m_datasHeight - height, 0.2f, (y)=>m_offset=y, Ease.QuartOut);
					}
					else // 数据不足一屏
					{
						if (m_offset != 0) tweener = Tweener.Create(m_offset, 0f, 0.2f, (y)=>m_offset=y, Ease.QuartOut);
					}
					evt.Use();
				}
				if (localRect.Contains(evt.mousePosition))
				{// 处理拖拽
					//记录拖拽速度队列
					float m_offsetDelta = m_lastMousePosi.y - evt.mousePosition.y;
					bool outOfBorder = moreThanOneView ? (m_offset < 0 || m_offset > m_datasHeight - height) : (m_offset != 0);
					if (m_offsetDeltaQueue.Count == 5) m_offsetDeltaQueue.Dequeue();
					m_offsetDeltaQueue.Enqueue(outOfBorder ? 0f : m_offsetDelta); // 超界的拖拽记录0

					if (!m_dragging)
					{// 拖拽中释放处理惯性
						float sum = 0f;
						int count = m_offsetDeltaQueue.Count;
						foreach (float delta in m_offsetDeltaQueue) sum += delta;
						m_offsetDeltaQueue.Clear();
						m_offsetInertia = sum / count;
						if (m_offsetInertia != 0)
						{
							tweener = Tweener.Create(m_offsetInertia, 0f, 0.5f, (y)=>
							{
								m_offsetInertia *= 0.95f;
								if (Mathf.Abs(m_offsetInertia) <= 1)
								{
									m_offsetInertia = 0f;
								}
								else
								{
									m_offset += m_offsetInertia;
									if (m_offset <= 0 || m_offset >= m_datasHeight - height)
									{
										m_offsetInertia = 0f;
										m_offset = Mathf.Clamp(m_offset, 0f, m_datasHeight - height);
									}
								}
							}, Ease.QuadOut);
						}
					}
					else
					{
						m_offset += m_offsetDelta * (outOfBorder ? 0.25f : 1f);
						m_lastMousePosi = evt.mousePosition;
					}
				}
			}

			GUI.EndGroup();
		}

		protected Tweener tweener;
		public override void OnUpdate(float deltaTime)
		{
			if (tweener != null)
			{
				if (!tweener.Update(deltaTime))
				{
					tweener = null;
				}
			}
		}
	}
}
