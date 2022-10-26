using System.Collections.Generic;
using UnityEngine;
using System.Text;

public static class GameObjectExtensiton
{
    public static void SetLayer(this object obj, int layer)
    {
        Stack<GameObject> stack = new Stack<GameObject>();
        stack.Push(obj as GameObject);

        while (stack.Count > 0)
        {
            GameObject go = stack.Pop();
            go.layer = layer;
            for (int i = 0; i < go.transform.childCount; i++)
            {
                stack.Push(go.transform.GetChild(i).gameObject);
            }
        }
    }

    public static string GetUIPathInHierarchy(this GameObject go)
    {
        string path = go.name;
        Transform temp = go.transform;

        while (temp.parent != null)
        {
            path = temp.parent.gameObject.name + "/" + path;
            temp = temp.parent;
        }

        return path;
    }
}
