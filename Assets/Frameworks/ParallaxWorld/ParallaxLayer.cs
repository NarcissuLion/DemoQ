using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Parallax2D
{
    public class ParallaxLayer : MonoBehaviour
    {
        public float scale;

        public void Move(float dx)
        {
            transform.position += Vector3.right * dx * scale;
        }
    }
}
