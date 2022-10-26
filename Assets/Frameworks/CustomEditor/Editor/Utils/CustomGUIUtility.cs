using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
#if UNITY_EDITOR
namespace Framework.CustomEditor
{
    public class CustomGUIUtility : GUIUtility
    {
        public enum NestedWindowType
        {
            /// <summary>
            /// 调用GUI.Window此种方式需要在前后加BeginWindow/EndWindow，且不能嵌套使用。
            /// </summary>
            Window,
            /// <summary>
            /// 调用GUI.BeginGroup/GUI.EndGroup此种方式需要控制GUI控件width不要超出范围，多Group时GUILayout需要自己进行适应，或配合GUIRectHelper使用。
            /// </summary>
            Group,
            /// <summary>
            /// 调用GUI.BeginClip/GUI.EndClip此种方式需要控制GUI控件width不要超出范围，GUIStyle参数对此无效，多Clip时GUILayout需要自己进行适应，或配合GUIRectHelper使用。
            /// </summary>
            Clip
        }
        private static bool s_IsResizingSplitLineHorizontal;
        private static bool s_IsResizingSplitLineVertical;
        private static Dictionary<int, Rect> s_NestedWindowInfo = new Dictionary<int, Rect>();

        public static Action<object> OnResizeSplitLineFinish;

        public static Action OnSelectedChanged;
        private static List<object> s_SelectedObjects = new List<object>(16);
        public static object SelectedObject
        {
            get
            {
                if (s_SelectedObjects.Count > 0)
                {
                    return s_SelectedObjects[0];
                }

                return null; 
            }
            set
            {
                if (value != null && value is UnityEngine.Object)
                {
                    UnityEditor.Selection.activeObject = value as UnityEngine.Object;
                }

                s_SelectedObjects.Clear();
                AddToSelection(value);
            }
        }
        
        public static bool IsInSelection(object o)
        {
            if (o == null)
            {
                return false;
            }

            return s_SelectedObjects.Contains(o);
        }

        public static void AddToSelection(object o)
        {
            if (o == null)
            {
                return;
            }

            if (IsInSelection(o))
            {
                return;
            }

            s_SelectedObjects.Add(o);

            if (OnSelectedChanged != null)
            {
                OnSelectedChanged();
            }
        }

        public static List<object> GetSelections()
        {
            return s_SelectedObjects;
        }


        public static void GUIWindow(string text, Rect windowRect, GUI.WindowFunction func, NestedWindowType type = NestedWindowType.Group)
        {
            GUIWindow(text, windowRect, func, type, GUIStyle.none);
        }

        public static void GUIWindow(string text, Rect windowRect, GUI.WindowFunction func, NestedWindowType type, GUIStyle style)
        {
            int windowId = text.GetHashCode();
            s_NestedWindowInfo[windowId] = new Rect(Vector2.zero, windowRect.size);
            GUI.Box(windowRect, "");
            if (type == NestedWindowType.Window)
            {
                GUI.Window(windowId, windowRect, func, style == GUIStyle.none ? "" : text, style);
            }
            else if (type == NestedWindowType.Group)
            {
                GUI.BeginGroup(windowRect,style);
                func(windowId);
                GUI.EndGroup();
            }
            else
            {
                GUI.BeginClip(windowRect);
                func(windowId);
                GUI.EndClip();
            }
            //windowRect.position = Vector2.zero;
            //s_NestedWindowInfo[windowId] = windowRect;
        }

        public static Rect GetWindowRect(int windowId)
        {
            return s_NestedWindowInfo[windowId];
        }
        public static bool IsMouseEnterWindow(int windowId)
        {
            return s_NestedWindowInfo[windowId].Contains(Event.current.mousePosition);
        }

        public static Rect GetControlRect(params GUILayoutOption[] options)
        {
            return EditorGUILayout.GetControlRect(options);
        }

        public static void GUISearchField(Rect searchRect, ref string searchString, ref bool isChanged)
        {
            searchString = GUISearchField(searchRect, searchString, ref isChanged);
        }

        public static string GUISearchField(Rect searchRect, string searchString, ref bool isChanged)
        {
            searchRect.width -= 15;
            EditorGUI.BeginChangeCheck();
            string result = EditorGUI.TextField(searchRect, searchString, (GUIStyle)"ToolbarSeachTextField");
            isChanged = EditorGUI.EndChangeCheck();
            searchRect.x += searchRect.width;
            searchRect.width = 15;
            if (GUI.Button(searchRect, "", (GUIStyle)"ToolbarSeachCancelButton"))
            {
                isChanged = true;
                result = string.Empty;
                GUIUtility.keyboardControl = 0;
            }
            return result;
        }

        public static void ResizeSplitLine(string controlName, float start, float end, ref float splitLine, bool isHorizontalSplitter = true)
        {
            ResizeSplitLine(controlName.GetHashCode(), start, end, ref splitLine, isHorizontalSplitter);
        }

        public static void ResizeSplitLine(string controlName, float start, float end, float min, float max, ref float splitLine, bool isHorizontalSplitter = true)
        {
            ResizeSplitLine(controlName.GetHashCode(), start, end, min, max, ref splitLine, isHorizontalSplitter);
        }

        public static void ResizeSplitLine(int controlID, float start, float end, ref float splitLine, bool isHorizontalSplitter = true)
        {
            const float CONST_SPLITLINE_WIDTH_SINGER = 1.5f;
            if (GUIUtility.hotControl != 0 && GUIUtility.hotControl != controlID) return;
            Event e = Event.current;
            Rect splitLineRect;
            if (isHorizontalSplitter)
            {
                splitLineRect = Rect.MinMaxRect(splitLine - CONST_SPLITLINE_WIDTH_SINGER, start, splitLine + CONST_SPLITLINE_WIDTH_SINGER, end);
            }
            else
            {
                splitLineRect = Rect.MinMaxRect(start, splitLine - CONST_SPLITLINE_WIDTH_SINGER, end, splitLine + CONST_SPLITLINE_WIDTH_SINGER);
            }
            //DrawSplitLine(splitLineRect);
            EditorGUIUtility.AddCursorRect(splitLineRect, isHorizontalSplitter ? UnityEditor.MouseCursor.SplitResizeLeftRight : MouseCursor.SplitResizeUpDown);
            if (e.type == EventType.MouseDown && e.button == 0 && splitLineRect.Contains(e.mousePosition))
            {
                GUIUtility.hotControl = controlID;
                if (isHorizontalSplitter)
                    s_IsResizingSplitLineHorizontal = true;
                else
                    s_IsResizingSplitLineVertical = true;
                e.Use();
            }
            if (isHorizontalSplitter && s_IsResizingSplitLineHorizontal) { splitLine = e.mousePosition.x; }
            if (!isHorizontalSplitter && s_IsResizingSplitLineVertical) { splitLine = e.mousePosition.y; }
            if (e.rawType == EventType.MouseUp)
            {
                s_IsResizingSplitLineVertical = false;
                s_IsResizingSplitLineHorizontal = false;
                GUIUtility.hotControl = 0;
                if (OnResizeSplitLineFinish != null)
                {
                    OnResizeSplitLineFinish(null);
                }
            }
        }

        public static void ResizeSplitLine(int controlID, float start, float end, float min, float max, ref float splitLine, bool isHorizontalSplitter = true)
        {
            ResizeSplitLine(controlID, start, end, ref splitLine, isHorizontalSplitter);
            splitLine = Mathf.Clamp(splitLine, min, max);
        }

        public static void GUISplitLine(Rect splitLineRect, Color color)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Color orginColor = GUI.color;
                GUI.color = color;
                GUI.DrawTexture(splitLineRect, UnityEditor.EditorGUIUtility.whiteTexture);
                GUI.color = orginColor;
            }
        }

        public static void GUISplitLine(Rect splitLineRect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Color color = GUI.color * ((!UnityEditor.EditorGUIUtility.isProSkin) ? new Color(0.6f, 0.6f, 0.6f, 1.333f) : new Color(0.12f, 0.12f, 0.12f, 1.333f));
                GUISplitLine(splitLineRect, color);
            }
        }

        public static void AcceptDrops(int windowId,Action<UnityEngine.Object[]> OnAcceptDropsCallback)
        {
            var e = Event.current;
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }
            if (e.type == EventType.DragPerform && CustomGUIUtility.IsMouseEnterWindow(windowId))
            {
                if (OnAcceptDropsCallback != null)
                {
                    OnAcceptDropsCallback(DragAndDrop.objectReferences);
                }
            }
        }
    }
}
#endif
