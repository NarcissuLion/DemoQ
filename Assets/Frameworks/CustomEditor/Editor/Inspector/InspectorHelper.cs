using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using Framework.CustomEditor.Undo;

namespace Framework.CustomEditor
{
    public class InspectorHelper
    {
        static BindingFlags s_BindingFlags = BindingFlags.Instance | BindingFlags.Public;
        static Dictionary<CustomMemberEditor, PropertyDrawer> s_PropertyDrawerTemplateDict = new Dictionary<CustomMemberEditor, PropertyDrawer>();
        static Dictionary<object, PropertyDrawer> s_uniqueDataPropertyDrawerDict = new Dictionary<object, PropertyDrawer>();
        static bool inited;

        public static void Init()
        {
            if (inited) return;
            Assembly assembly = typeof(InspectorHelper).Assembly;
            foreach (Type type in assembly.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(PropertyDrawer))) continue;

                CustomMemberEditor customEditor = GetAttribute<CustomMemberEditor>(type);
                if (customEditor != null)
                {
                    s_PropertyDrawerTemplateDict[customEditor] = System.Activator.CreateInstance(type) as PropertyDrawer;
                }
            }
            inited = true;
        }

        static PropertyDrawer GetWrapperPropertyDrawer(MemberInfo memberInfo, object value)
        {
            if (value != null && s_uniqueDataPropertyDrawerDict.ContainsKey(value))
            {
                return s_uniqueDataPropertyDrawerDict[value];
            }
            Type type = null;
            if (memberInfo is FieldInfo)
            {
                type = (memberInfo as FieldInfo).FieldType;
            }
            else if (memberInfo is PropertyInfo)
            {
                type = (memberInfo as PropertyInfo).PropertyType;
            }

            if (type == null) return null;

            if (type == typeof(string))
            {
                var attribute = GetAttribute<AcceptDragPathAttribute>(memberInfo);
                if (attribute != null)
                {
                    var acceptKeys = GetPropertyDrawerKeys(attribute.GetType());
                    foreach (var key in acceptKeys)
                    {
                        if (key.LimitTypes == null || key.LimitTypes.Length == 0)
                        {
                            if (s_PropertyDrawerTemplateDict.ContainsKey(key))
                            {
                                var wrapper = s_PropertyDrawerTemplateDict[key];
                                wrapper.attribute = attribute;
                                return wrapper;
                            }
                        }
                    }
                }
            }

            var keys = GetPropertyDrawerKeys(type);

            CustomMemberEditor matchKey = null;
            if (keys.Length == 1)
            {
                matchKey = keys[0];
            }
            else if (keys.Length > 1)
            {
                Type classType = memberInfo.ReflectedType;

                // CustomMemberEditor noLimitKey = null;
                int maxWeight = -1;
                foreach (CustomMemberEditor key in keys)
                {
                    bool basePass = false;
                    if (key.LimitTypes != null)
                    {
                        foreach (Type limitType in key.LimitTypes)
                        {
                            if (limitType.IsAssignableFrom(classType))
                            {
                                basePass = true;
                                break;
                            }
                        }
                    }
                    bool typePass = key.LimitTypes == null || key.LimitTypes.Contains(classType);
                    bool namePass = key.LimitFieldNames == null || key.LimitFieldNames.Contains(memberInfo.Name);
                    typePass = !typePass ? basePass : typePass;
                    if (typePass && namePass)
                    {
                        int weight = (key.LimitTypes != null ? 1 : 0) + (key.LimitFieldNames != null ? 1 : 0);
                        if (weight > maxWeight)
                        {
                            maxWeight = weight;
                            matchKey = key;
                        }
                    }
                }
            }

            PropertyDrawer drawer = matchKey == null ? null : s_PropertyDrawerTemplateDict[matchKey];

            if (drawer != null && matchKey.UseUniqueDrawerInspector)
            {
                drawer = System.Activator.CreateInstance(drawer.GetType()) as PropertyDrawer;
                s_uniqueDataPropertyDrawerDict.Add(value, drawer);
            }

            return drawer;
        }

        static CustomMemberEditor[] GetPropertyDrawerKeys(Type type)
        {
            List<CustomMemberEditor> keys = new List<CustomMemberEditor>();
            foreach (CustomMemberEditor key in s_PropertyDrawerTemplateDict.Keys)
            {
                foreach (Type inspectedType in key.InspectedTypes)
                {
                    if (inspectedType.IsAssignableFrom(type))
                    {
                        keys.Add(key);
                    }
                }
            }
            return keys.ToArray();
        }

        public static TAttribute GetAttribute<TAttribute>(MemberInfo element) where TAttribute : Attribute
        {
            return Attribute.GetCustomAttribute(element, typeof(TAttribute)) as TAttribute;
        }

        public static TAttribute[] GetAttributes<TAttribute>(MemberInfo element) where TAttribute : Attribute
        {
            return Attribute.GetCustomAttributes(element, typeof(TAttribute)) as TAttribute[];
        }

        public static bool HasAttribute<TAttribute>(MemberInfo element) where TAttribute : Attribute
        {
            return GetAttribute<TAttribute>(element) != null;
        }

        public static float DisplayByReflection(object data, ref float y, float x, float maxWidth, object context, UndoManager undoMgr, Action undoCallback = null, Action redoCallback = null)
        {
            if (data == null) return y;

            Type dataType = data.GetType();

            FieldInfo[] fieldInfos = dataType.GetFields(s_BindingFlags);
            foreach (FieldInfo field in fieldInfos)
            {
                bool isSerialized = HasAttribute<DataSerializeAttribute>(field);
                bool isNonSerialized = HasAttribute<NonDataSerializeAttribute>(field);
                bool hideInInspector = HasAttribute<HideInInspector>(field);
                if (!hideInInspector && !isNonSerialized && (isSerialized || field.IsPublic))
                {
                    if (ValidateFieldDependency(data, field))
                    {
                        DrawSplitLine(GetAttribute<DrawSplitLineAttribute>(field), ref y, x, maxWidth);
                        DrawFiledInfo(field, data, ref y, x, maxWidth, context, undoMgr, undoCallback, redoCallback);
                    }
                }
            }

            PropertyInfo[] propertyInfos = dataType.GetProperties(s_BindingFlags);

            foreach (PropertyInfo property in propertyInfos)
            {

                bool isSerialized = HasAttribute<DataSerializeAttribute>(property);
                bool isNonSerialized = HasAttribute<NonDataSerializeAttribute>(property);
                bool hideInInspector = HasAttribute<HideInInspector>(property);
                if (!hideInInspector && !isNonSerialized && (isSerialized || (property.CanWrite && property.CanRead)))
                {
                    if (ValidateFieldDependency(data, property))
                    {
                        DrawSplitLine(GetAttribute<DrawSplitLineAttribute>(property), ref y, x, maxWidth);
                        DrawPropertyInfo(property, data, ref y, x, maxWidth, context, undoMgr, undoCallback, redoCallback);
                    }
                }
            }
            return y;
        }

        static void DrawSplitLine(DrawSplitLineAttribute drawSplitLine, ref float y, float x, float maxWidth)
        {
            if (drawSplitLine != null)
            {
                y += 10f;
                if (string.IsNullOrEmpty(drawSplitLine.label))
                {
                    EditorGUI.DrawTextureAlpha(new Rect(x, y, maxWidth - 5f, 1f), EditorGUIUtility.whiteTexture);
                }
                else
                {
                    float labelWidth = EditorStyles.boldLabel.CalcSize(new GUIContent(drawSplitLine.label)).x;
                    EditorGUI.DrawTextureAlpha(new Rect(x, y, (maxWidth - 5f - labelWidth) * 0.5f, 1f), EditorGUIUtility.whiteTexture);
                    EditorGUI.LabelField(new Rect(x + (maxWidth - 5f - labelWidth) * 0.5f, y - 10f, labelWidth, 18f), drawSplitLine.label, EditorStyles.boldLabel);
                    EditorGUI.DrawTextureAlpha(new Rect(x + (maxWidth - 5f + labelWidth) * 0.5f, y, (maxWidth - 5f - labelWidth) * 0.5f, 1f), EditorGUIUtility.whiteTexture);
                }
                y += 10f;
            }
        }

        public static bool ValidateFieldDependency(object data, MemberInfo checkMember)
        {
            Type dataType = data.GetType();

            FieldDependencyAttribute[] dependencyAttrs = GetAttributes<FieldDependencyAttribute>(checkMember);
            foreach (FieldDependencyAttribute dependency in dependencyAttrs)
            {
                MemberInfo[] members = dataType.GetMember(dependency.memberName);
                if (members.Length == 0)
                {
                    return false;
                }
                MemberInfo member = members[0];
                object memberValue = null;
                bool isFlags = false;
                Type underType = null;
                if (member.MemberType == MemberTypes.Field)
                {
                    FieldInfo field = (FieldInfo)member;
                    memberValue = field.GetValue(data);
                    if (field.FieldType == typeof(Framework.FMath.Fix64))
                    {
                        for (int i = 0; i < dependency.memberValues.Length; i++)
                        {
                            if (dependency.memberValues[i].GetType() != typeof(Framework.FMath.Fix64))
                            {
                                dependency.memberValues[i] = new Framework.FMath.Fix64((float)dependency.memberValues[i]);
                            }
                        }
                    }
                    isFlags = field.FieldType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;
                    if (isFlags) { underType = Enum.GetUnderlyingType(field.FieldType); }
                }
                else if (member.MemberType == MemberTypes.Property)
                {
                    PropertyInfo property = (PropertyInfo)member;
                    memberValue = property.GetValue(data, null);
                    if (property.PropertyType == typeof(Framework.FMath.Fix64))
                    {
                        for (int i = 0; i < dependency.memberValues.Length; i++)
                        {
                            if (dependency.memberValues[i].GetType() != typeof(Framework.FMath.Fix64))
                            {
                                dependency.memberValues[i] = new Framework.FMath.Fix64((float)dependency.memberValues[i]);
                            }
                        }
                    }
                    isFlags = property.PropertyType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;
                    if (isFlags) { underType = Enum.GetUnderlyingType(property.PropertyType); }
                }

                if (memberValue == null)
                {
                    return false;
                }
                if (!isFlags)
                {
                    if (dependency.isEqual && !dependency.memberValues.Any(a => a.Equals(memberValue)))
                    {
                        return false;
                    }
                    if (!dependency.isEqual && dependency.memberValues.Any(a => a.Equals(memberValue)))
                    {
                        return false;
                    }
                }
                if (isFlags)
                {
                    bool hit = false;
                    if (underType == typeof(System.Byte))
                    {
                        byte value = (byte)memberValue;
                        foreach (byte flag in dependency.memberValues)
                        {
                            if (flag == 0) { return value == 0; }
                            if (flag == byte.MaxValue && value != flag) {return false;}
                            hit |= (value & flag) > 0;
                        }
                        if (!hit) {return false;}
                    }
                    else if (underType == typeof(System.UInt16))
                    {
                        ushort value = (ushort)memberValue;
                        foreach (ushort flag in dependency.memberValues)
                        {
                            if (flag == 0) { return value == 0; }
                            if (flag == byte.MaxValue && value != flag) {return false;}
                            hit |= (value & flag) > 0;
                        }
                        if (!hit) {return false;}
                    }
                    else if (underType == typeof(System.UInt32))
                    {
                        uint value = (uint)memberValue;
                        foreach (uint flag in dependency.memberValues)
                        {
                            if (flag == 0) { return value == 0; }
                            if (flag == byte.MaxValue && value != flag) {return false;}
                            hit |= (value & flag) > 0;
                        }
                        if (!hit) {return false;}
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        protected static void DrawFiledInfo(FieldInfo field, object data, ref float y, float x, float maxWidth, object context, UndoManager undoMgr, Action undoCallback = null, Action redoCallback = null)
        {
            object fieldValue = field.GetValue(data);
            PropertyDrawer drawer = GetWrapperPropertyDrawer(field, fieldValue);
            if (drawer != null)
            {
                string label = field.Name;
                object origValue = field.GetValue(data);
                if (field.FieldType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                {
                    origValue = Convert.ChangeType(origValue, Enum.GetUnderlyingType(field.FieldType));
                }
                object value = drawer.OnDisplay(field.FieldType, label, fieldValue, context, ref y, x, maxWidth);
                if (!origValue.IsSameAs(value) && undoMgr != null)
                {
                    FieldUndoAction undoAction = new FieldUndoAction(data, field, origValue, value);
                    if (undoCallback != null) undoAction.undoCallback += undoCallback;
                    if (redoCallback != null) undoAction.redoCallback += redoCallback;
                    undoMgr.Record(undoAction);
                }

                RangeAttribute rangeAttribute = field.GetCustomAttribute<RangeAttribute>();
                if (rangeAttribute != null)
                {
                    if (fieldValue.GetType() == typeof(int))
                    {
                        value = (int)Mathf.Clamp((int)value, rangeAttribute.min, rangeAttribute.max);
                    }
                    else if(fieldValue.GetType() == typeof(float))
                    {
                        value = (float)Mathf.Clamp((float)value, rangeAttribute.min, rangeAttribute.max);
                    }
                }

                field.SetValue(data, value);
            }
        }

        protected static void DrawPropertyInfo(PropertyInfo property, object data, ref float y, float x, float maxWidth, object context, UndoManager undoMgr, Action undoCallback = null, Action redoCallback = null)
        {
            object propertyValue = property.GetValue(data, null);
            PropertyDrawer drawer = GetWrapperPropertyDrawer(property, propertyValue);
            if (drawer != null)
            {
                string label = property.Name;
                object origValue = property.GetValue(data, null);
                if (property.PropertyType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                {
                    origValue = Convert.ChangeType(origValue, Enum.GetUnderlyingType(property.PropertyType));
                }
                object value = drawer.OnDisplay(property.PropertyType, label, propertyValue, context, ref y, x, maxWidth);
                if (!origValue.IsSameAs(value) && undoMgr != null)
                {
                    PropertyUndoAction undoAction = new PropertyUndoAction(data, property, origValue, value);
                    if (undoCallback != null) undoAction.undoCallback += undoCallback;
                    if (redoCallback != null) undoAction.redoCallback += redoCallback;
                    undoMgr.Record(undoAction);
                }

                RangeAttribute rangeAttribute = property.GetCustomAttribute<RangeAttribute>();
                if (rangeAttribute != null)
                {
                    if (propertyValue.GetType() == typeof(int))
                    {
                        value = (int)Mathf.Clamp((int)value, rangeAttribute.min, rangeAttribute.max);
                    }
                    else if (propertyValue.GetType() == typeof(float))
                    {
                        value = (float)Mathf.Clamp((float)value, rangeAttribute.min, rangeAttribute.max);
                    }
                }

                property.SetValue(data, value, null);
            }
        }


        public static void Dispose()
        {
            s_PropertyDrawerTemplateDict.Clear();
            s_uniqueDataPropertyDrawerDict.Clear();
        }
    }
}
