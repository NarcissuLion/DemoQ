using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Framework.CustomEditor
{
    public class SerializableAnimCurve : IXmlElement
    {
        public AnimationCurve curve = new AnimationCurve();

        public void ExportData(XmlElement elem)
        {
            elem.SetAttribute("preWrapMode", (int)curve.preWrapMode+"");
            elem.SetAttribute("postWrapMode", (int)curve.postWrapMode+"");
            for (int i = 0; i < curve.keys.Length; ++i)
            {
                Keyframe key = curve.keys[i];
                XmlElement e = elem.OwnerDocument.CreateElement("KeyFrame");
                e.SetAttribute("time", key.time.ToString());
                e.SetAttribute("value", key.value.ToString());
                e.SetAttribute("inTangent", key.inTangent.ToString());
                e.SetAttribute("outTangent", key.outTangent.ToString());
                e.SetAttribute("inWeight", key.inWeight.ToString());
                e.SetAttribute("outWeight", key.outWeight.ToString());
                e.SetAttribute("weightedMode", (int)key.weightedMode+"");
                elem.AppendChild(e);
            }
        }

        public void LoadData(XmlElement elem)
        {
            curve.preWrapMode = (WrapMode)int.Parse(elem.GetAttribute("preWrapMode"));
            curve.postWrapMode = (WrapMode)int.Parse(elem.GetAttribute("postWrapMode"));
            for (int i = 0; i < elem.ChildNodes.Count; ++i)
            {
                XmlElement e = elem.ChildNodes[i] as XmlElement;
                Keyframe key = new Keyframe();
                key.time = float.Parse(e.GetAttribute("time"));
                key.value = float.Parse(e.GetAttribute("value"));
                key.inTangent = float.Parse(e.GetAttribute("inTangent"));
                key.outTangent = float.Parse(e.GetAttribute("outTangent"));
                key.inWeight = float.Parse(e.GetAttribute("inWeight"));
                key.outWeight = float.Parse(e.GetAttribute("outWeight"));
                key.weightedMode = (WeightedMode)int.Parse(e.GetAttribute("weightedMode"));
                curve.AddKey(key);
            }
        }
    }
}
