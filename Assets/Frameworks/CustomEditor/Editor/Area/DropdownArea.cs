using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework.CustomEditor
{
    public class DropdownArea : Area
    {
        public Vector2 size;
        public Vector2 relativePos;

        public string[] options;
        public int selectedIdx = 0;

        public string selectedOption
        {
            get
            {
                return options == null || options.Length <= selectedIdx ? string.Empty : options[selectedIdx];
            }
        }

        public DropdownArea(int layer) : base(layer)
        {
        }
        public override float width { get { return size.x; } }
        public override float height { get { return size.y; } }
        public override Vector2 relativePosition { get { return relativePos; } }

        protected override void OnDrawGUI(Event evt)
        {
            GUI.color = Color.white;

            Rect globalRect = GetGlobalRect();
            GUI.BeginGroup(globalRect);

            GUI.Box(localRect, string.Empty, new GUIStyle("toolbarButton"));
            GUI.Label(new Rect(2f, 1f, width - 20f, height - 1f), selectedOption);

            GUI.color = new Color(1f, 1f, 1f, 0.75f);
            GUI.DrawTexture(new Rect(width - 14f, (height - 4f) * 0.5f, 8f, 5f), EditorGUIUtility.FindTexture("d_icon dropdown"));
            GUI.color = Color.white;

            GUI.EndGroup();
        }

        protected override void OnDoInteraction(Event evt)
        {
        }
    }
}
