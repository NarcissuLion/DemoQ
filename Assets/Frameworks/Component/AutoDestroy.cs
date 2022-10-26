using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float delay = 1f;
    private float _timer;

    void OnEnable()
    {
        _timer = 0f;
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= delay)
        {
            GameObject.Destroy(gameObject);
        }
    }
}
