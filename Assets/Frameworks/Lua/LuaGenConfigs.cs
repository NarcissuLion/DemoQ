using System.Collections.Generic;
using System;
using XLua;
using System.Reflection;
using System.Linq;

public static class LuaGenConfigs
{
    //黑名单
    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()
    {
    };

    [CSharpCallLua]
    public static List<Type> CSharpCallLuaList = new List<Type>()
    {
        typeof(System.Action),
        typeof(System.Action<int>),
        typeof(System.Action<float>),
        typeof(System.Action<string>),
        typeof(DG.Tweening.Core.DOGetter<UnityEngine.Vector3>),
        typeof(DG.Tweening.Core.DOSetter<UnityEngine.Vector3>),
    };

    [LuaCallCSharp]
    [ReflectionUse]
    public static List<Type> LuaCallCSharpList = new List<Type>()
    {
        // Unity
        typeof(UnityEngine.GameObject),
        typeof(UnityEngine.Resources),
        typeof(UnityEngine.Vector3),
        typeof(UnityEngine.Vector2),
        typeof(UnityEngine.Quaternion),
        typeof(UnityEngine.SpriteRenderer),
        typeof(UnityEngine.Color),

        // DOTween
        typeof(DG.Tweening.AutoPlay),
        typeof(DG.Tweening.AxisConstraint),
        typeof(DG.Tweening.Ease),
        typeof(DG.Tweening.LogBehaviour),
        typeof(DG.Tweening.LoopType),
        typeof(DG.Tweening.PathMode),
        typeof(DG.Tweening.PathType),
        typeof(DG.Tweening.RotateMode),
        typeof(DG.Tweening.ScrambleMode),
        typeof(DG.Tweening.TweenType),
        typeof(DG.Tweening.UpdateType),

        typeof(DG.Tweening.DOTween),
        typeof(DG.Tweening.DOVirtual),
        typeof(DG.Tweening.EaseFactory),
        typeof(DG.Tweening.Tweener),
        typeof(DG.Tweening.Tween),
        typeof(DG.Tweening.Sequence),
        typeof(DG.Tweening.TweenParams),
        typeof(DG.Tweening.Core.ABSSequentiable),

        typeof(DG.Tweening.Core.TweenerCore<UnityEngine.Vector3, UnityEngine.Vector3, DG.Tweening.Plugins.Options.VectorOptions>),

        typeof(DG.Tweening.TweenCallback),
        typeof(DG.Tweening.TweenExtensions),
        typeof(DG.Tweening.TweenSettingsExtensions),
        typeof(DG.Tweening.ShortcutExtensions),
        typeof(DG.Tweening.DOTweenModuleSprite),
    };
}
