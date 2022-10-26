using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 mask = Vector3.one;
    public Vector3 offset = Vector3.zero;
    public float maxSpeed = 10f;
    public float smoothness = 3f;
    private float _speed = 0f;

    void Start()
    {
        
    }

    void OnEnable()
    {
        _speed = 0f;
    }

    void LateUpdate()
    {
        // if (target == null) return;

        // Vector3 src = transform.position;
        // Vector3 dst = target.position + offset;
        // Vector3 diff = dst - src;
        // diff.x *= mask.x;
        // diff.y *= mask.y;
        // diff.z *= mask.z;

        // float strength = diff.magnitude - (smoothness * smoothness);
        // _speed = Mathf.Clamp(_speed + strength * Time.deltaTime, 0f, maxSpeed);

        // Vector3 pos = src + diff.normalized * _speed * Time.deltaTime;
        // transform.position = pos;
    }
}
