using UnityEngine;

namespace Framework.Utils
{    
    public class CoroutineProxy : MonoBehaviour
    {
        private static CoroutineProxy m_Instance;
        public static CoroutineProxy Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    GameObject gameObject = new GameObject("coroutine_proxy");
                    m_Instance = gameObject.AddComponent<CoroutineProxy>();
                    Object.DontDestroyOnLoad(gameObject);
                }
                return m_Instance;
            }
        }

        void OnDestroy()
        {
            m_Instance = null;
        }
    }
}
