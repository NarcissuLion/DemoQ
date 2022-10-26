using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Parallax2D
{
    public class ParallaxWorld : MonoBehaviour
    {
        public List<ParallaxLayer> layers = new List<ParallaxLayer>();

        public Transform observer;
        private float _lastX;

        void OnEnable()
        {
            if (observer != null) _lastX = observer.position.x;
        }

        void Update()
        {
            if (observer == null || layers == null || layers.Count == 0) return;

            float dx = observer.position.x - _lastX;
            foreach (ParallaxLayer layer in layers) layer.Move(dx);
            _lastX = observer.position.x;
        }
    }
}
