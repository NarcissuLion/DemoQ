using System.Collections.Generic;
using UnityEngine;
using Framework.Noti;

namespace Framework.CustomEditor
{
	public class PopupAreaMgr
	{
		private Dictionary<int, PopupArea> m_popupDict = new Dictionary<int, PopupArea>();
		private PopupArea m_activedPopup;
		public PopupArea activedPopup { get { return m_activedPopup; } }

		public void RegisterPopup(int id, PopupArea area)
		{
			area.mgr = this;
			m_popupDict.Add(id, area);
		}

		public void Dispose()
		{
			Close();
			foreach (var popupArea in m_popupDict.Values)
			{
				popupArea.Dispose();
			}
			m_popupDict.Clear();
			m_popupDict = null;
		}

		public PopupArea Popup(int id)
		{
			Close();
			m_activedPopup = m_popupDict[id];
			if (m_activedPopup == null)
			{
				Debug.LogError("PopupAreaMgr -> Popup Error! Popup<" + id + "> not exists.");
				return null;
			}
			else
			{
				m_activedPopup.OnPopup();
				return m_activedPopup;
			}
		}

		public void Close()
		{
			if (m_activedPopup != null)
			{
				m_activedPopup.OnClose();
				m_activedPopup = null;
			}
		}

		public void Update(float deltaTime)
		{
			if (m_activedPopup != null)
			{
				m_activedPopup.Update(deltaTime);
			}
		}
	}
}
