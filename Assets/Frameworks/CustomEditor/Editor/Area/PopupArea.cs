using UnityEngine;

namespace Framework.CustomEditor
{
    public abstract class PopupArea : Area
    {
        public PopupArea() : base(0)
        {
        }

        public override float width { get { return Screen.width; } }

        public override float height { get { return Screen.height; } }

        public override Vector2 relativePosition { get { return Vector2.zero; } }

        public abstract Rect contentRect { get; }

        public PopupAreaMgr mgr;

        public bool isActived { get { return mgr.activedPopup == this; }}

        public override void DoInteraction(Event evt)
        {
            base.DoInteraction(evt);
            if (evt.type == EventType.MouseDown)
            {
                if (evt.button == 0 && !contentRect.Contains(evt.mousePosition))
                {
                    mgr.Close();
                    evt.Use();
                }
            }
        }

        public virtual void OnPopup()
        {
        }

        public virtual void OnClose()
        {
        }

        public override void Dispose()
        {
            if (isActived)
            {
                OnClose();
            }
            base.Dispose();
        }
    }
}
