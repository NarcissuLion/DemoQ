using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class ObjectExtensiton
{
    public static bool IsSameAs(this object one, object another)
    {
        if (one == null && another == null) return true;
        else  if (one == null || another == null)
        {
            if (one != null )
            {
                //  字符串null与长度为0判断为相等
                if (one is string && ((string)one).Length == 0)
                    return true;
                //  迭代器null与0元素判断为相等
                else if (one is IEnumerable && !(one as IEnumerable).GetEnumerator().MoveNext())
                    return true;
                return false;
            }
            return false;
        }

        var type = one.GetType();

        if (type != another.GetType()) return false;

        if (type.IsPrimitive || type.IsEnum || type == typeof(string))
        {
            return one.Equals(another);
        }
        else if (one is IEnumerable)
        {
            var a = (one as IEnumerable).GetEnumerator();
            var b = (another as IEnumerable).GetEnumerator();

            bool aFlag = a.MoveNext();
            bool bFlag = b.MoveNext();

            while (aFlag && bFlag)
            {
                if (!IsSameAs(a.Current, b.Current))
                {
                    return false;
                }
                aFlag = a.MoveNext();
                bFlag = b.MoveNext();
            }

            return bFlag == aFlag;
        }
        else
        {
            var fields = FindInspectorFields(type);
            foreach (var field in fields)
            {
                if (!IsSameAs(field.GetValue(one), field.GetValue(another)))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public static FieldInfo[] FindInspectorFields(Type type)
    {
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).ToList();
        for (int i = fields.Count - 1; i >= 0; i--)
        {
            var field = fields[i];
            //  如果有 NonSerializedAttribute / HideInInspector ，则不显示
            if (Attribute.GetCustomAttribute(field, typeof(NonSerializedAttribute)) != null ||
                Attribute.GetCustomAttribute(field, typeof(HideInInspector)) != null ||
                //  或者是nopublic，且没有 SerializeField ，则不显示
                (!field.IsPublic && Attribute.GetCustomAttribute(field, typeof(SerializeField)) == null)
                )
            {
                fields.RemoveAt(i);
            }
        }
        fields.Sort((a, b) => a.MetadataToken.CompareTo(b.MetadataToken));
        return fields.ToArray();
    }
}
