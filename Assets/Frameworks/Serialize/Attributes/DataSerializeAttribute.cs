using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class DataSerializeAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class NonDataSerializeAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class DataPoolParamsAttribute : Attribute
{
    public int paramIndex;
    public DataPoolParamsAttribute(int paramIndex)
    {
        this.paramIndex = paramIndex;
    }
}
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class DataPoolDStructAttribute : Attribute
{
    public DataPoolDStructAttribute()
    {
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public class FieldDependencyAttribute : Attribute
{
    public bool isEqual;
    public string memberName;
    public object[] memberValues;

    public FieldDependencyAttribute(bool isEqual, string memberName, params object[] memberValues)
    {
        this.isEqual = isEqual;
        this.memberName = memberName;
        this.memberValues = memberValues;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class DrawSplitLineAttribute : Attribute
{
    public string label;
    public DrawSplitLineAttribute(string label = null)
    {
        this.label = label;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class AcceptDragPathAttribute : Attribute
{
    public string rootDir;
    public bool hasExtension;
    public Type acceptType;
    /// <summary>
    /// 让GUI的接收拖拽对象转换为路径
    /// </summary>
    /// <param name="folderLevel">从自身开始往父目录数，保留多少层级的文件夹，如自身为1，自身的父目录为2，以此类推</param>
    /// <param name="hasExtension">是否保留文件后缀名</param>
    /// <param name="acceptType">允许的对象类型， null 时为所有</param>
    public AcceptDragPathAttribute(Type acceptType = null, string rootDir = "", bool hasExtension = true)
    {
        this.rootDir = rootDir;
        this.hasExtension = hasExtension;
        this.acceptType = acceptType;
    }
}