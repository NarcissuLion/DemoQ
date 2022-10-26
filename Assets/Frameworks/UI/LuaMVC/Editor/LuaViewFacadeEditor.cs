using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Framework.LuaMVC.Editor
{

    [UnityEditor.CustomEditor(typeof(LuaViewFacade))]
    public class LuaViewFacadeEditor : UnityEditor.Editor
    {
        private SerializedProperty viewNameProperty;
        private SerializedProperty compsProperty;
        private List<SerializedProperty> comps = new List<SerializedProperty>();
        private ReorderableList list;

        void OnEnable()
        {
            viewNameProperty = serializedObject.FindProperty("viewName");
            compsProperty = serializedObject.FindProperty("comps");
            list = new ReorderableList(comps, typeof(LuaViewComponent));
            list.elementHeight = 68f;
            list.onAddCallback = OnAddCallback;
            list.onRemoveCallback = OnRemoveCallback;
            list.drawElementCallback = OnDrawElementCallback;
            list.drawElementBackgroundCallback = OnDrawElementBackgroundCallback;
            list.drawHeaderCallback = OnDrawHeaderCallback;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(viewNameProperty);
            EditorGUILayout.Space();
            comps.Clear();
            for (int i = 0; i < compsProperty.arraySize; i++)
            {
                comps.Add(compsProperty.GetArrayElementAtIndex(i));
            }
            list.DoLayoutList();
            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            LuaViewFacade facade = target as LuaViewFacade;
            if (facade.transform.parent == null)
            {
                if (GUILayout.Button("Generate Lua Script"))
                {
                    LuaViewScriptGenerator.Generate(facade);
                }
            }
        }

        private void OnAddCallback(ReorderableList list)
        {
            compsProperty.arraySize++;
        }

        private void OnRemoveCallback(ReorderableList list)
        {
            compsProperty.DeleteArrayElementAtIndex(list.index);
        }

        private static List<string> __supportCompTypes = new List<string>(10)
        {
            "Object",
            "LuaViewFacade",
            "Image",
            "RawImage",
            "Text",
            "TextMeshProUGUI",
            "InputField",
            "TMP_InputField",
            "Toggle",
            "ToggleGroup",
            "Button",
            "Slider",
            "Scrollbar",
            "ScrollRect",
        };
        private void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (rect.width <= 0) return;

            SerializedProperty typeProperty = comps[index].FindPropertyRelative("type");
            SerializedProperty nameProperty = comps[index].FindPropertyRelative("name");
            SerializedProperty targetProperty = comps[index].FindPropertyRelative("target");
            // SerializedProperty paramStrProperty = comps[index].FindPropertyRelative("paramStr");
            // SerializedProperty paramIntProperty = comps[index].FindPropertyRelative("paramInt");
            
            List<string> typeOptions = new List<string>(10);
            List<Object> typeTargets = new List<Object>(10);

            Object obj = targetProperty.objectReferenceValue as Object;
            GameObject go = obj == null ? null : (obj is GameObject ? (GameObject)obj : ((MonoBehaviour)obj).gameObject);
            if (go != null)
            {
                typeOptions.Add("Object");
                typeTargets.Add(go);
                
                for (int i = 1; i < __supportCompTypes.Count; i++)
                {
                    string compType = __supportCompTypes[i];
                    var comp = go.GetComponent(compType);
                    if (comp == null) continue;
                    typeOptions.Add(compType);
                    typeTargets.Add(comp);
                }
            }
            
            Rect fieldRect = new Rect(rect.x, rect.y, rect.width, 20f);
            EditorGUI.LabelField(new Rect(fieldRect.x, fieldRect.y + 1, 60f, 18f), "Type");
            EditorGUI.BeginChangeCheck();
            int typeIdx = EditorGUI.Popup(new Rect(fieldRect.x + 60, fieldRect.y + 1, fieldRect.width - 60f, 18f), typeOptions.IndexOf(typeProperty.stringValue), typeOptions.ToArray());
            if (EditorGUI.EndChangeCheck())
            {
                typeProperty.stringValue = typeOptions[typeIdx];
                targetProperty.objectReferenceValue = typeTargets[typeIdx];
            }

            fieldRect.y += 20f;
            EditorGUI.LabelField(new Rect(fieldRect.x, fieldRect.y + 1, 60f, 18f), "Target");
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(fieldRect.x + 60, fieldRect.y + 1, fieldRect.width - 60f, 18f), targetProperty, new GUIContent(""));
            if (EditorGUI.EndChangeCheck())
            {
                if (targetProperty.objectReferenceValue == null) typeProperty.stringValue = string.Empty;
                else typeProperty.stringValue = "Object";
            }
            
            fieldRect.y += 20f;
            EditorGUI.LabelField(new Rect(fieldRect.x, fieldRect.y + 1, 60f, 18f), "Name");
            EditorGUI.PropertyField(new Rect(fieldRect.x + 60, fieldRect.y + 1, fieldRect.width - 60f, 18f), nameProperty, new GUIContent(""));

            // switch (typeProperty.stringValue)
            // {
            //     case "Text":
            //     case "TextMeshProUGUI":
            //         fieldRect.y += 20f;
            //         EditorGUI.LabelField(new Rect(fieldRect.x, fieldRect.y + 1, 60f, 18f), "LangTbl");
            //         EditorGUI.PropertyField(new Rect(fieldRect.x + 60, fieldRect.y + 1, fieldRect.width * 0.5f - 60f, 18f), paramStrProperty, new GUIContent(""));
            //         EditorGUI.LabelField(new Rect(fieldRect.x + fieldRect.width * 0.5f + 10, fieldRect.y + 1, 50f, 18f), "LangID");
            //         EditorGUI.PropertyField(new Rect(fieldRect.x + fieldRect.width * 0.5f + 60, fieldRect.y + 1, fieldRect.width * 0.5f - 60f, 18f), paramIntProperty, new GUIContent(""));
            //         break;
            //     default:
            //         break;
            // }
        }

        private void OnDrawElementBackgroundCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (rect.width <= 0) return;
            if (list.index == index) EditorGUI.DrawRect(rect, new Color(0f, 0.1f, 0.2f, 0.5f));
            else if (index % 2 == 1) EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.1f));
        }

        private void OnDrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Lua UI Components");
        }
    }
}
