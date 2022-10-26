using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;

namespace Framework.CustomEditor
{
    public class ToolsEditorWindow<T> : EditorWindow where T : EditorWindow
    {
        public Area m_rootArea;
        public PopupAreaMgr m_popupMgr;

        protected float m_editorPreviousTime;

        private static T m_window;
        public static T s_window
        {
            get
            {
                if (m_window == null)
                {
                    FieldInfo titleField = typeof(T).GetField("TITLE", BindingFlags.NonPublic | BindingFlags.Static);
                    if (titleField == null) m_window = EditorWindow.GetWindow<T>(typeof(T).Name);
                    else m_window = EditorWindow.GetWindow<T>((string)titleField.GetValue(null));
                }
                return m_window;
            }
        }

		protected virtual void OnWindowEnable()
		{

		}

		protected virtual void OnWindowDisable()
		{
			
		}

		protected virtual void OnWindowClose()
		{

		}

		protected virtual void OnExitEditMode()
		{

		}

		protected virtual void OnExitPlayMode()
		{

		}

		protected virtual void OnWindowUpdate(float deltaTime)
		{
			
		}

        protected virtual void OnPreGUI(Event evt)
        {
            
        }

        protected virtual void OnAfterGUI(Event evt)
        {

        }


        void OnEnable()
        {
            m_popupMgr = new PopupAreaMgr();
            OnWindowEnable();

            EditorApplication.playModeStateChanged += OnPlaymodeChanged;
            EditorApplication.update += OnEditorUpdate;
        }

        void OnDisable()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                OnWindowClose();
            }
            m_popupMgr.Dispose();

			OnWindowDisable();

            if (m_rootArea != null)
            {
                m_rootArea.Dispose();
            }

            EditorApplication.playModeStateChanged -= OnPlaymodeChanged;
            EditorApplication.update -= OnEditorUpdate;

            System.GC.Collect();
        }

        void OnPlaymodeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                OnExitEditMode();
            }
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                OnExitPlayMode();
            }
        }

        public virtual void AddCursorRect(Rect rect, MouseCursor cursor, bool ignorePopup = false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                return;
            }
            if (!ignorePopup && m_popupMgr.activedPopup != null)
            {
                return;
            }
            EditorGUIUtility.AddCursorRect(rect, cursor);
        }

        void OnEditorUpdate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                return;
            }

            float deltaTime = (Time.realtimeSinceStartup - m_editorPreviousTime) * Time.timeScale;
            m_editorPreviousTime = Time.realtimeSinceStartup;
            //delta *= cutscene.playbackSpeed;

			OnWindowUpdate(deltaTime);

            if (m_rootArea != null)
            {                
                m_rootArea.Update(deltaTime);
            }
            m_popupMgr.Update(deltaTime);
        }

        void OnGUI()
        {
            Event evt = Event.current;

            OnPreGUI(evt);

            if (evt.type == EventType.Layout)
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                if (evt.isMouse || evt.isKey || evt.isScrollWheel)
                {
                    evt.Use();
                }
            }

            if (m_popupMgr.activedPopup != null) m_popupMgr.activedPopup.DoInteraction(evt);

            if (m_rootArea != null)
            {
                m_rootArea.DrawGUI(evt);
                m_rootArea.DoInteraction(evt);
            }

            if (m_popupMgr.activedPopup != null) m_popupMgr.activedPopup.DrawGUI(evt);

            DoKeyboardShortcuts(evt);

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                GUI.color = new Color(0f, 0f, 0f, 0.8f);
                GUI.backgroundColor = new Color(0f, 0f, 0f, 0.8f);
                GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), string.Empty, EditorStyles.helpBox);
            }

            OnAfterGUI(evt);

            Repaint();
        }

        protected virtual void DoKeyboardShortcuts(Event evt)
        {
        }
    }
}
