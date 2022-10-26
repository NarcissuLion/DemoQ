using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Framework.Noti;
using Framework.CustomEditor;

namespace Framework.Asset.Profiler
{
    public class RootArea : Area
    {
        // private ToolsbarArea _toolsbarArea;
        // private QuestListArea _questListArea;
        // private QuestDetailArea _questDetailArea;
        // private ActorDetailArea _actorDetailArea;
        // private QuestResultArea _questResultArea;

        public RootArea(int layer) : base (layer)
        {
            // _toolsbarArea = new ToolsbarArea(0);
            // _questListArea = new QuestListArea(1);
            // _questDetailArea = new QuestDetailArea(2);
            // _actorDetailArea = new ActorDetailArea(3);
            // _questResultArea = new QuestResultArea(4);
            // AddChild(_toolsbarArea);
            // AddChild(_questListArea);
            // AddChild(_questDetailArea);
            // AddChild(_actorDetailArea);

            // NotiCenter.AddStaticListener(NotiDef.DISPLAY_MODE_CHANGED, OnDisplayModeChanged);
        }

        public override void Dispose()
        {
            // NotiCenter.RemoveStaticListener(NotiDef.DISPLAY_MODE_CHANGED, OnDisplayModeChanged);
        }

        private void OnDisplayModeChanged(INotification noti)
        {
            // if (Context.showingResult)
            // {
            //     RemoveChild(_questDetailArea);
            //     RemoveChild(_actorDetailArea);
            //     AddChild(_questResultArea);
            // }
            // else
            // {
            //     AddChild(_questDetailArea);
            //     AddChild(_actorDetailArea);
            //     RemoveChild(_questResultArea);
            // }
        }

        override public float width { get { return ProfilerWindow.s_window.position.x; } }
		override public float height { get { return ProfilerWindow.s_window.position.y; } }
		override public Vector2 relativePosition{ get { return Vector2.zero; } }

        protected override void OnDrawGUI(Event evt)
        {
        }

        protected override void OnDoInteraction(Event evt)
        {
        }
    }
}
