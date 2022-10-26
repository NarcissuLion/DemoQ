using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIRectHelper<T> : GUIRectHelper/* where T : class*/
{
    public readonly static GUIRectHelper Instance = new GUIRectHelper();
}

/// <summary>
/// 在GUI的开始务必调用Initialize来清空
/// </summary>
public class GUIRectHelper
{
    private int m_Index = -1;
    private Dictionary<int, Rect> m_Dict = new Dictionary<int, Rect>();
    private Font m_DefaultFont;
    private float m_MinX, m_MaxX, m_MinY, m_MaxY;
    public Vector2 orgin;
    public float DefaultWidth = 60;
    public float DefaultHeight = 15.9f;
    public float IntervalHorizontal = 1f;
    public float IntervalVertical = 1f;

    public bool IsHorizontal { get; set; }

    public Rect CurrentAreaRect { get { return Rect.MinMaxRect(m_MinX, m_MinY, m_MaxX, m_MaxY + IntervalVertical); } }

    /// <summary>
    /// 获取默认字体得行高
    /// </summary>
    public int GetDefaultFontLineHeight { get { return DefaultFont.lineHeight; } }

    /// <summary>
    /// 设置/获取 默认字体
    /// </summary>
    public Font DefaultFont
    {
        get
        {
            if (m_DefaultFont == null) m_DefaultFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
            return m_DefaultFont;
        }
        set { m_DefaultFont = value; }
    }

    /// <summary>
    /// 获取GUI文本得宽度
    /// </summary>
    public int GetGUIStringWidth(string message)
    {
        return GetGUIStringWidth(message, DefaultFont);
    }

    /// <summary>
    /// 获取GUI文本得宽度
    /// </summary>
    public int GetGUIStringWidth(string message, Font font)
    {
        int totalLength = 0;
        font.RequestCharactersInTexture(message, font.fontSize);
        CharacterInfo characterInfo = new CharacterInfo();
        char[] arr = message.ToCharArray();
        foreach (char c in arr)
        {
            font.GetCharacterInfo(c, out characterInfo, font.fontSize);

            totalLength += characterInfo.advance;
        }
        return totalLength;
    }

    public Rect GetRect()
    {
        return GetRect(DefaultWidth, DefaultHeight);
    }

    public Rect GetRect(float width)
    {
        return GetRect(width, DefaultHeight);
    }

    public Rect GetRect(float width, float height)
    {
        Rect rect;
        if (m_Index < 0)
        {
            rect = new Rect(orgin.x + IntervalHorizontal, orgin.y + IntervalVertical, width, height);
        }
        else
        {
            rect = LastRect;
            if (IsHorizontal)
            {
                rect.x += rect.width + IntervalHorizontal * 2;
            }
            else
            {
                rect.x = orgin.x + IntervalHorizontal;
                rect.y += rect.height + IntervalVertical * 2;
            }
            rect.width = width;
            rect.height = height;
        }
        m_Dict.Add(++m_Index, rect);
        CulateMinMax(rect);
        return rect;
    }

    private void CulateMinMax(Rect rect)
    {
        if (m_Index > 0)
        {
            if (m_MinX > rect.xMin) m_MinX = rect.xMin;
            if (m_MinY > rect.yMin) m_MinY = rect.yMin;
            if (m_MaxX < rect.xMax) m_MaxX = rect.xMax;
            if (m_MaxY < rect.yMax) m_MaxY = rect.yMax;
        }
        else
        {
            m_MinX = rect.xMin;
            m_MaxX = rect.xMax;
            m_MinY = rect.yMin;
            m_MaxY = rect.yMax;
        }
    }

    public Rect LastRect
    {
        get { return m_Dict[m_Index]; }
        set { m_Dict[m_Index] = value; CulateMinMax(value); }
    }


    /// <summary>
    /// 清空之前的Rect信息，IsHorizontal初始化为false。
    /// </summary>
    public void Initialize()
    {
        m_Index = -1;
        IsHorizontal = false;
        m_Dict.Clear();
    }

    /// <summary>
    /// 清空之前的Rect信息，IsHorizontal初始化为false。
    /// </summary>
    public void Initialize(Vector2 orgin)
    {
        Initialize();
        this.orgin = orgin;
    }

    /// <summary>
    /// 清空之前的Rect信息，IsHorizontal初始化为false。
    /// </summary>
    public void Initialize(Vector2 orgin, float defaultWidth)
    {
        Initialize(orgin);
        this.DefaultWidth = defaultWidth;
    }

    /// <summary>
    /// 清空之前的Rect信息，IsHorizontal初始化为false。
    /// </summary>
    public void Initialize(Vector2 orgin, float defaultWidth, float defaultHeight)
    {
        Initialize(orgin, defaultWidth);
        this.DefaultHeight = defaultHeight;
    }

    /// <summary>
    /// 清空之前的Rect信息，IsHorizontal初始化为false。
    /// </summary>
    public void Initialize(Rect rect)
    {
        Initialize(rect.position, rect.width, rect.height);
    }
}
