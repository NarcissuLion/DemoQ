using System.Collections.Generic;
using UnityEngine;
using System.Text;

public static class TransformExtensiton
{
    public static Transform FindChildExt(this Transform root, string childName)
    {
        Stack<Transform> stack = new Stack<Transform>();
        stack.Push(root as Transform);

        while (stack.Count > 0)
        {
            Transform child = stack.Pop();
            if (child.name == childName) return child;
            for (int i = 0; i < child.childCount; i++)
            {
                stack.Push(child.GetChild(i));
            }
        }
        return null;
    }

    public static string GetChildPath(this Transform root, string childName)
    {
        Transform child = root.FindChildExt(childName);
        if (child == null) return string.Empty;
        StringBuilder sb = new StringBuilder();
        while (child != null)
        {
            sb.Append("/").Append(child.name);
            child = child.parent;
        }
        sb.Remove(0, 1);
        return sb.ToString();
    }
}
