using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Framework.FMath;

namespace Framework.CustomEditor
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    public class DescriptionAttribute : Attribute
    {
        public string description;
        public DescriptionAttribute(string description)
        {
            this.description = description;
        }
    }

    public class CustomMemberEditor : System.Attribute
    {
        public System.Type[] InspectedTypes { get; private set; }

        /// <summary>
        /// 要限制为派生自LimitType。与LimitFileName一起使用。
        /// </summary>
        public Type[] LimitTypes { get; set; }

        /// <summary>
        /// 要限制为制定的字段名(区分大小写)。与LimitType一起使用。
        /// </summary>
        public string[] LimitFieldNames { get; set; }

        /// <summary>
        /// 是否使用唯一的Inspector面板实例，而不是通用模板
        /// </summary>
        public bool UseUniqueDrawerInspector { get; set; }

        public CustomMemberEditor(params Type[] inspectedTypes)
        {
            InspectedTypes = inspectedTypes;
        }
    }
    public abstract class PropertyDrawer
    {
        public Attribute attribute;
        public abstract object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth);

    }

    [CustomMemberEditor(typeof(string))]
    public class StringPropertyEditor : PropertyDrawer
    {
        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            string text = EditorGUI.TextField(new Rect(x, y, maxWidth - 5f, 20f), label, (string)value);
            y += 20f;
            return text;
        }
    }
    [CustomMemberEditor(typeof(List<string>))]
    public class StringListPropertyEditor : PropertyDrawer
    {
        ReorderableList reorderableList;

        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            List<string> data = (List<string>)value;

            if (reorderableList == null)
            {
                reorderableList = new ReorderableList(data, typeof(string), true, false, true, true);

                reorderableList.onAddCallback = (list) => 
                {
                    list.list.Add(string.Empty);
                };

                reorderableList.drawHeaderCallback = (rect) => {
                    EditorGUI.LabelField(rect, label);
                };

                reorderableList.drawElementCallback = (rect, index, isActive, isFocused) => {
                    rect.height -= 4;
                    rect.y += 2;
                    data[index] = EditorGUI.TextField(rect, data[index]);
                };
            }

            reorderableList.DoList(new Rect(x, y, maxWidth - 5f, reorderableList.GetHeight()));
            y += reorderableList.GetHeight();
            return data;
        }
    }

    [CustomMemberEditor(typeof(Color))]
    public class ColorPropertyEditor : PropertyDrawer
    {
        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            Color color = EditorGUI.ColorField(new Rect(x, y, maxWidth - 5f, 20f), label, (Color)value);
            y += 20f;
            return color;
        }
    }

    [CustomMemberEditor(typeof(int))]
    public class IntProtertyEditor : PropertyDrawer
    {
        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            int intValue = (int)value;
            intValue = EditorGUI.IntField(new Rect(x, y, maxWidth - 5f, 20f), label, intValue);
            y += 20f;
            return Convert.ChangeType(intValue, value.GetType());
        }
    }

    [CustomMemberEditor(typeof(uint))]
    public class UIntProtertyEditor : PropertyDrawer
    {
        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            string strValue = value+"";
            strValue = EditorGUI.TextField(new Rect(x, y, maxWidth - 5f, 20f), label, strValue);
            y += 20f;
            uint newValue;
            if (!uint.TryParse(strValue, out newValue))
            {
                newValue = (uint)value;
            }
            return newValue;
        }
    }

    [CustomMemberEditor(typeof(float))]
    public class FloatPropertyEditor : PropertyDrawer
    {
        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            float data = (float)value; 
            data = EditorGUI.FloatField(new Rect(x, y, maxWidth - 5f, 20f), label, data);
            y += 20f;
            return data;
        }
    }

    [CustomMemberEditor(typeof(RectOffset))]
    public class RectOffsetPropertyEditor : PropertyDrawer
    {
        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            RectOffset data = (RectOffset)value;

            float wholeWidth = maxWidth - 5f;
            Rect rect = new Rect(x, y, wholeWidth, 20f);
            EditorGUI.LabelField(rect, label);
            y += 20f;

            rect = new Rect(x + 20f, y, wholeWidth - 20f, 20f);
            data.left = EditorGUI.IntField(rect, "Left", data.left);

            y += 20f;
            rect = new Rect(x + 20f, y, wholeWidth - 20f, 20f);
            data.right = EditorGUI.IntField(rect, "Right", data.right);

            y += 20f;
            rect = new Rect(x + 20f, y, wholeWidth - 20f, 20f);
            data.top = EditorGUI.IntField(rect, "Top", data.top);

            y += 20f;
            rect = new Rect(x + 20f, y, wholeWidth - 20f, 20f);
            data.bottom = EditorGUI.IntField(rect, "Bottom", data.bottom);

            y += 20f;    
            return data;
        }
    }

    [CustomMemberEditor(typeof(Fix64))]
    public class Fix64PropertyEditor : PropertyDrawer
    {
        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            Fix64 deltaFix64 = (Fix64)value;
            deltaFix64 = (Fix64)UnityEditor.EditorGUI.FloatField(new Rect(x, y, maxWidth - 5f, 20f), label, (float)deltaFix64);
            y += 20f;
            return deltaFix64;
        }
    }

    [CustomMemberEditor(typeof(bool))]
    public class BoolPropertyEditor : PropertyDrawer
    {
        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            bool toggle = UnityEditor.EditorGUI.Toggle(new Rect(x, y, maxWidth - 5f, 20f), label, (bool)value);
            y += 20f;
            return toggle;
        }
    }

    [CustomMemberEditor(typeof(Enum))]
    public class EnumPropertyEditor : PropertyDrawer
    {
        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            if (type.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
            {
                EditorGUI.LabelField(new Rect(x, y, maxWidth - 5f, 20f), label);
                y += 20f;

                EditorGUI.indentLevel = 1;

                Type underType = Enum.GetUnderlyingType(type);

                string[] names = Enum.GetNames(type);
                Array flags = Enum.GetValues(type);

                object result;

                bool allChecked = true;
                if (underType == typeof(System.Byte))
                {
                    byte oldValue = (byte)value;
                    byte newValue = 0;
                    for (int i = 0; i < names.Length; i++)
                    {
                        byte flag = (byte)flags.GetValue(i);
                        bool isNone = flag == 0;
                        bool isAll = flag == byte.MaxValue;
                        bool check = false;
                        if (isAll)
                        {
                            using (var scope = new EditorGUI.ChangeCheckScope())
                            {
                                check = EditorGUI.Toggle(new Rect(x + 10f, y, maxWidth - 5f, 20f), names[i], allChecked);
                                y += 20f;
                                if (scope.changed)
                                {
                                    if (!check)
                                    {
                                        newValue = (byte)0;
                                    }
                                    else
                                    {
                                        newValue = 0;
                                        foreach (byte f in flags)
                                        {
                                            if (f < byte.MaxValue)
                                            {
                                                newValue |= f;
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            check = EditorGUI.Toggle(new Rect(x + 10f, y, maxWidth - 5f, 20f), names[i], (oldValue & flag) > 0);
                            y += 20f;
                        }
                        if (check)
                        {
                            if (isNone)
                            {
                                newValue = 0;
                                break;
                            }
                            if (!isAll)
                            {
                                newValue |= flag;
                            }
                        }
                        else if (!isNone && !isAll)
                        {
                            allChecked = false;
                        }
                    }
                    result = newValue;
                }
                else if (underType == typeof(System.UInt16))
                {
                    ushort oldValue = (ushort)value;
                    ushort newValue = 0;
                    for (int i = 0; i < names.Length; i++)
                    {
                        ushort flag = (ushort)flags.GetValue(i);
                        bool isNone = flag == 0;
                        bool isAll = flag == ushort.MaxValue;
                        bool check = false;
                        if (isAll)
                        {
                            using (var scope = new EditorGUI.ChangeCheckScope())
                            {
                                check = EditorGUI.Toggle(new Rect(x + 10f, y, maxWidth - 5f, 20f), names[i], allChecked);
                                y += 20f;
                                if (scope.changed)
                                {
                                    if (!check)
                                    {
                                        newValue = (ushort)0;
                                    }
                                    else
                                    {
                                        newValue = 0;
                                        foreach (ushort f in flags)
                                        {
                                            if (f < byte.MaxValue)
                                            {
                                                newValue |= f;
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            check = EditorGUI.Toggle(new Rect(x + 10f, y, maxWidth - 5f, 20f), names[i], (oldValue & flag) > 0);
                            y += 20f;
                        }
                        if (check)
                        {
                            if (isNone)
                            {
                                newValue = 0;
                                break;
                            }
                            if (!isAll)
                            {
                                newValue |= flag;
                            }
                        }
                        else if (!isNone && !isAll)
                        {
                            allChecked = false;
                        }
                    }
                    result = newValue;
                }
                else if (underType == typeof(System.UInt32))
                {
                    uint oldValue = (uint)value;
                    uint newValue = 0;
                    for (int i = 0; i < names.Length; i++)
                    {
                        uint flag = (uint)flags.GetValue(i);
                        bool isNone = flag == 0;
                        bool isAll = flag == uint.MaxValue;
                        bool check = false;
                        if (isAll)
                        {
                            using (var scope = new EditorGUI.ChangeCheckScope())
                            {
                                check = EditorGUI.Toggle(new Rect(x + 10f, y, maxWidth - 5f, 20f), names[i], allChecked);
                                y += 20f;
                                if (scope.changed)
                                {
                                    if (!check)
                                    {
                                        newValue = (uint)0;
                                    }
                                    else
                                    {
                                        newValue = 0;
                                        foreach (uint f in flags)
                                        {
                                            if (f < byte.MaxValue)
                                            {
                                                newValue |= f;
                                            }
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            check = EditorGUI.Toggle(new Rect(x + 10f, y, maxWidth - 5f, 20f), names[i], (oldValue & flag) > 0);
                            y += 20f;
                        }
                        if (check)
                        {
                            if (isNone)
                            {
                                newValue = 0;
                                break;
                            }
                            if (!isAll)
                            {
                                newValue |= flag;
                            }
                        }
                        else if (!isNone && !isAll)
                        {
                            allChecked = false;
                        }
                    }
                    result = newValue;
                }
                else
                {
                    Debug.LogError("Invalide Flags Enum UnderType.");
                    result = value;
                }
                

                EditorGUI.indentLevel = 0;

                return result;
            }
            else
            {
                Enum result = EditorGUI.EnumPopup(new Rect(x, y, maxWidth - 5f, 20f), label, (Enum)value);
                y += 20f;
                return result;
            }
        }
    }

    [CustomMemberEditor(typeof(Fix64Vector2))]
    public class Fix64Vector2PropertyEditor : PropertyDrawer
    {
        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            Fix64Vector2 data = (Fix64Vector2)value;
            data = new Fix64Vector2(EditorGUI.Vector2Field(new Rect(x, y, maxWidth - 5f, 20f), label, data.ToVector2()));
            y += 20f;
            return data;
        }
    }

    [CustomMemberEditor(typeof(List<Fix64Vector2>), UseUniqueDrawerInspector = true)]
    public class Fix64Vector2ListPropertyEditor : PropertyDrawer
    {
        ReorderableList reorderableList;

        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            List<Fix64Vector2> data = (List<Fix64Vector2>)value;

            if (reorderableList == null)
            {
                reorderableList = new ReorderableList(data, typeof(string), true, false, true, true);

                reorderableList.drawHeaderCallback = (rect) => {
                    EditorGUI.LabelField(rect, label);
                };

                reorderableList.drawElementCallback = (rect, index, isActive, isFocused) => {
                    rect.height -= 4;
                    rect.y += 2;
                    data[index] = new Fix64Vector2(EditorGUI.Vector2Field(rect, string.Empty, data[index].ToVector2()));
                };
            }

            reorderableList.DoList(new Rect(x, y, maxWidth - 5f, reorderableList.GetHeight()));
            y += reorderableList.GetHeight();
            return data;
        }
    }

    [CustomMemberEditor(typeof(AcceptDragPathAttribute))]
    public class AcceptDragDropStringPropertyEditor : PropertyDrawer
    {
        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            float height = 20;
            var position = new Rect(x, y, maxWidth - 5f, height);
            y += height;
            EditorGUI.LabelField(position, label);

            height = GUI.skin.textArea.CalcHeight(new GUIContent(value.ToString()), maxWidth - 5f);
            if (height < 20)
                height *= 2;
            height += 4;
            position = new Rect(x, y, maxWidth - 5f, height);
            y += height + 4f;

            var adpAttribute = attribute as AcceptDragPathAttribute;
            value = CheckDragDrop(position, value, adpAttribute.rootDir, adpAttribute.hasExtension, adpAttribute.acceptType);

            return EditorGUI.TextArea(position, value.ToString(), GUI.skin.textArea);
        }

        public object CheckDragDrop(Rect position, object value, string rootDir, bool hasExtension, Type acceptType)
        {
            if (position.Contains(Event.current.mousePosition) && DragAndDrop.paths.Length > 0)
            {
                if (acceptType == null || acceptType == DragAndDrop.objectReferences[0].GetType())
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (Event.current.type == EventType.DragExited)
                    {
                        string path = DragAndDrop.paths[0].Replace("\\", "/");
                        var subPaths = path.Split('/').Reverse();
                        string newPath = string.Empty;
                        foreach (var subPath in subPaths)
                        {
                            if (subPath != rootDir)
                            {
                                if (newPath.Length == 0)
                                    newPath = hasExtension ? subPath : System.IO.Path.GetFileNameWithoutExtension(subPath);
                                else
                                    newPath = subPath + "/" + newPath;
                            }
                            else
                            {
                                break;
                            }
                        }
                        return newPath;
                    }
                }
            }
            return value;
        }
    }

    // [CustomMemberEditor(typeof(CCActorFilter))]
    // public class ActorFilterPropertyEditor : PropertyDrawer
    // {
    //     public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
    //     {
    //         CCActorFilter data = (CCActorFilter)value;
    //         UndoManager undoMgr = null;
    //         String contextClassName = context == null ? string.Empty : context.GetType().FullName;
    //         InspectorHelper.DisplayByReflection(data, ref y, x, maxWidth, context, undoMgr);
    //         return data;
    //     }
    // }

    [CustomMemberEditor(typeof(SerializableAnimCurve))]
    public class CurvePropertyEditor : PropertyDrawer
    {
        public override object OnDisplay(Type type, string label, object value, object context, ref float y, float x, float maxWidth)
        {
            SerializableAnimCurve data = (SerializableAnimCurve)value;
            if (data == null) data = new SerializableAnimCurve();
            data.curve = EditorGUI.CurveField(new Rect(x, y, maxWidth - 5f, 20f), label, data.curve);
            y += 20f;
            return data;
        }
    }
}