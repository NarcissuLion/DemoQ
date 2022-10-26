using System.Xml;
using Framework.Log;
using Framework.FMath;
using System.Collections.Generic;
using System;
using System.Reflection;

public static class XmlElementExtensions
{
    public static string GetAttributeCheckEmpty(this XmlElement element, string attrName)
    {
        string str = element.GetAttribute(attrName);
        if (str == "empty") return string.Empty;
        return str;
    }

    public static bool GetAttributeToBool(this XmlElement element, string attrName)
    {
        string str = element.ToString();
        bool result;
        bool.TryParse(str, out result);
        return result;
    }

    public static int GetAttributeToInteger(this XmlElement element, string attrName)
    {
        string str = element.ToString();
        int value;
        int.TryParse(str, out value);
        return value;
    }

    public static float GetAttributeToFloat(this XmlElement element, string attrName)
    {
        string str = element.ToString();
        float value;
        float.TryParse(str, out value);
        return value;
    }

    public static short GetAttributeI16(XmlElement e, string attrName)
    {
        if (e.HasAttribute(attrName))
        {
            string str = e.GetAttribute(attrName);
            if (string.IsNullOrEmpty(str))
                return 0;

            return Convert.ToInt16(str);
        }

        return 0;
    }

    public static int GetAttributeI32(XmlElement e, string attrName)
    {
        if (e.HasAttribute(attrName))
        {
            string str = e.GetAttribute(attrName);
            if (string.IsNullOrEmpty(str))
                return 0;

            return Convert.ToInt32(str);
        }

        return 0;
    }

    public static long GetAttributeI64(XmlElement e, string attrName)
    {
        if (e.HasAttribute(attrName))
        {
            string str = e.GetAttribute(attrName);
            if (string.IsNullOrEmpty(str))
                return 0;

            return Convert.ToInt64(str);
        }

        return 0;
    }

    public static ushort GetAttributeU16(XmlElement e, string attrName)
    {
        if (e.HasAttribute(attrName))
        {
            string str = e.GetAttribute(attrName);
            if (string.IsNullOrEmpty(str))
                return 0;

            return Convert.ToUInt16(str);
        }

        return 0;
    }

    public static uint GetAttributeU32(XmlElement e, string attrName)
    {
        if (e.HasAttribute(attrName))
        {
            string str = e.GetAttribute(attrName);
            if (string.IsNullOrEmpty(str))
                return 0;

            return Convert.ToUInt32(str);
        }

        return 0;
    }

    public static ulong GetAttributeU64(XmlElement e, string attrName)
    {
        if (e.HasAttribute(attrName))
        {
            string str = e.GetAttribute(attrName);
            if (string.IsNullOrEmpty(str))
                return 0;

            return Convert.ToUInt64(str);
        }

        return 0;
    }

    public static byte GetAttributeByte(XmlElement e, string attrName)
    {
        if (e.HasAttribute(attrName))
        {
            string str = e.GetAttribute(attrName);
            if (string.IsNullOrEmpty(str))
                return 0;

            return Convert.ToByte(str);
        }

        return 0;
    }

    public static float GetAttributeF32(XmlElement e, string attrName)
    {
        if (e.HasAttribute(attrName))
        {
            string str = e.GetAttribute(attrName);
            if (string.IsNullOrEmpty(str))
                return 0;

            return Convert.ToSingle(str);
        }

        return 0;
    }

    public static double GetAttributeF64(XmlElement e, string attrName)
    {
        if (e.HasAttribute(attrName))
        {
            string str = e.GetAttribute(attrName);
            if (string.IsNullOrEmpty(str))
                return 0;

            return Convert.ToDouble(str);
        }

        return 0;
    }

    public static bool GetAttributeBool(XmlElement e, string attrName)
    {
        if (e.HasAttribute(attrName))
        {
            string str = e.GetAttribute(attrName);
            if (string.IsNullOrEmpty(str))
                return false;

            return Convert.ToBoolean(str);
        }

        return false;
    }

    public static string GetAttributeStr(XmlElement e, string attrName)
    {
        if (e.HasAttribute(attrName))
            return e.GetAttribute(attrName);

        return string.Empty;
    }

    public static Fix64 GetAttributeFix64(XmlElement e, string attrName)
    {
        return new Fix64(GetAttributeF32(e, attrName));
    }

    public static T GetAttributeEnum<T>(XmlElement e, string attrName)
    {
        object o = GetAttributeEnum(e, attrName, typeof(T));
        if (o == null)
            return default(T);

        return (T)o;
    }

    public static object GetAttributeEnum(XmlElement e, string attrName, Type type)
    {
        if (!type.IsEnum)
        {
            Logger.LogError(string.Format("{0} is not an enum type.", type));
            return type.Assembly.CreateInstance(type.Name);
        }

        if (e.HasAttribute(attrName))
        {
            string str = e.GetAttribute(attrName);
            if (string.IsNullOrEmpty(str))
                return type.Assembly.CreateInstance(type.Name);

            return Enum.Parse(type, str);
        }

        return type.Assembly.CreateInstance(type.Name); ;
    }

    public static T GetAttribute<T>(XmlElement e, string attrName)
    {
        return (T)GetAttribute(e, attrName, typeof(T));
    }

    private static HashSet<string> _legalTypeNames = new HashSet<string>(new string[] { 
        "byte", 
        "Byte", 
        "short",
        "Int16",
        "ushort",
        "UInt16",
        "int",
        "Int32" ,
        "uint",
        "UInt32",
        "long",
        "Int64",
        "ulong",
        "UInt64",
        "bool",
        "Boolean",
        "float",
        "Single",
        "double",
        "Double",
        "string",
        "String",
        "Fix64",
    });

    public static object GetAttribute(XmlElement e, string attrName, Type type)
    {
        if (type.IsEnum)
            return GetAttributeEnum(e, attrName, type);

        switch (type.Name)
        {
            case "byte":
            case "Byte":
                return GetAttributeByte(e, attrName);
            case "short":
            case "Int16":
                return GetAttributeI16(e, attrName);
            case "ushort":
            case "UInt16":
                return GetAttributeU16(e, attrName);
            case "int":
            case "Int32":
                return GetAttributeI32(e, attrName);
            case "uint":
            case "UInt32":
                return GetAttributeU32(e, attrName);
            case "long":
            case "Int64":
                return GetAttributeI64(e, attrName);
            case "ulong":
            case "UInt64":
                return GetAttributeU64(e, attrName);
            case "bool":
            case "Boolean":
                return GetAttributeBool(e, attrName);
            case "float":
            case "Single":
                return GetAttributeF32(e, attrName);
            case "double":
            case "Double":
                return GetAttributeF64(e, attrName);
            case "string":
            case "String":
                return GetAttributeStr(e, attrName);
            case "Fix64":
                return GetAttributeFix64(e, attrName);
            default:
                Logger.LogError("Unsupported type : " + type.Name);
                return type.Assembly.CreateInstance(type.Name);
        }
    }

    public static void SerializeObjectDataToXml<T>(XmlElement e, T o) where T : class, IXmlElement
    {
        SerializeObjectDataToXml<T>(e, ref o);
    }

    public static void SerializeObjectDataToXml<T>(XmlElement e, ref T o) where T : IXmlElement
    {
        Type type = o.GetType();
        FieldInfo[] fields = type.GetFields();

        for (int i = 0; i < fields.Length; ++i)
        {
            FieldInfo field = fields[i];

            DataSerializeAttribute attr = Attribute.GetCustomAttribute(field, typeof(DataSerializeAttribute)) as DataSerializeAttribute;
            if (attr != null)
            {
                Type t = field.FieldType;

                if (typeof(IXmlElement).IsAssignableFrom(t))
                    continue;

                object value = field.GetValue(o);

                string xmlAttribute;
                if (typeof(ICustomSerializer).IsAssignableFrom(t))
                    xmlAttribute = ((ICustomSerializer)value).Serialize();
                else
                    xmlAttribute = value == null ? string.Empty : value.ToString();

                if (!string.IsNullOrEmpty(xmlAttribute))
                    e.SetAttribute(field.Name, xmlAttribute);
            }
        }

        PropertyInfo[] properties = type.GetProperties();
        for (int i = 0; i < properties.Length; ++i)
        {
            PropertyInfo property = properties[i];

            DataSerializeAttribute attr = Attribute.GetCustomAttribute(property, typeof(DataSerializeAttribute)) as DataSerializeAttribute;
            if (attr != null)
            {
                Type t = property.PropertyType;

                if (typeof(IXmlElement).IsAssignableFrom(t))
                    continue;

                object value = property.GetValue(o, null);

                string xmlAttribute;
                if (typeof(ICustomSerializer).IsAssignableFrom(t))
                    xmlAttribute = ((ICustomSerializer)value).Serialize();
                else
                    xmlAttribute = value.ToString();

                if (!string.IsNullOrEmpty(xmlAttribute))
                    e.SetAttribute(property.Name, xmlAttribute);
            }
        }
    }

    public static void DeserializeObjectFromXml<T>(XmlElement e, T o) where T : class, IXmlElement
    {
        DeserializeObjectFromXml(e, ref o);
    }

    public static void DeserializeObjectFromXml<T>(XmlElement e, ref T reference) where T : IXmlElement
    {
        object o = (object)reference;

        Type type = o.GetType();
        FieldInfo[] fields = type.GetFields();

        for (int i = 0; i < fields.Length; ++i)
        {
            FieldInfo field = fields[i];

            DataSerializeAttribute attr = Attribute.GetCustomAttribute(field, typeof(DataSerializeAttribute)) as DataSerializeAttribute;
            if (attr != null)
            {
                Type t = field.FieldType;

                if (typeof(IXmlElement).IsAssignableFrom(t))
                    continue;

                if (t.IsEnum || _legalTypeNames.Contains(t.Name))
                {
                    field.SetValue(o, GetAttribute(e, field.Name, t));
                }
                else if (typeof(ICustomSerializer).IsAssignableFrom(t))
                {
                    object value = field.GetValue(o);
                    ((ICustomSerializer)value).Deserialize(GetAttributeStr(e, field.Name));
                    field.SetValue(o, value);
                }
                else
                {
                    Logger.LogError("Unsupported type: " + t.Name);
                }
            }
        }

        PropertyInfo[] properties = type.GetProperties();
        for (int i = 0; i < properties.Length; ++i)
        {
            PropertyInfo property = properties[i];

            DataSerializeAttribute attr = Attribute.GetCustomAttribute(property, typeof(DataSerializeAttribute)) as DataSerializeAttribute;
            if (attr != null)
            {
                Type t = property.PropertyType;

                if (typeof(IXmlElement).IsAssignableFrom(t))
                    continue;

                if (t.IsEnum || _legalTypeNames.Contains(t.Name))
                {
                    property.SetValue(o, GetAttribute(e, property.Name, t), null);
                }
                else if (typeof(ICustomSerializer).IsAssignableFrom(t))
                {
                    object value = property.GetValue(o, null);
                    ((ICustomSerializer)value).Deserialize(GetAttributeStr(e, property.Name));
                    property.SetValue(o, value, null);
                }
                else
                {
                    Logger.LogError("Unsupported type.");
                }
            }
        }

        reference = (T)o;
    }
}