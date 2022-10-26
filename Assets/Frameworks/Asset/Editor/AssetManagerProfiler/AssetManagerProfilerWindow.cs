using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Framework.Noti;
using Framework.CustomEditor;

namespace Framework.Asset.Profiler
{
    public class ProfilerWindow : ToolsEditorWindow<ProfilerWindow>
    {
        private static string TITLE = "Asset Manager Profiler";

        [MenuItem("Framework/Asset/Profiler")]
        public static void Open()
        {
            ProfilerWindow.s_window.minSize = new Vector2(1080f, 720f);
            // SimulatorWindow.s_window.maxSize = new Vector2(1250f, 860f);
            ProfilerWindow.s_window.maximized = true;
        }

        protected override void OnWindowEnable()
        {
            m_rootArea = new RootArea(0);
            // m_popupMgr.RegisterPopup(POPUP_EDIT_RESULT_FIELDS, new EditResultFieldPopup());
        }

        protected override void OnWindowDisable()
        {
            Clear();
        }

        protected override void OnWindowUpdate(float deltaTime)
		{
            // if (Context.simulation_running) Repaint();
			// _core.Update();
		}

        public void Clear()
        {
        }
    }
}

