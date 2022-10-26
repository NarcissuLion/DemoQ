using UnityEngine;
using UnityEngine.EventSystems;
using System;

//简单的封装一下EventTrigger，方便lua层注册回调
public class SpriteRendererEventTrigger : EventTrigger
{
    public Action pointClickCallback;
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (pointClickCallback != null) pointClickCallback();
    }
}
