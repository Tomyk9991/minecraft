using UnityEngine;

namespace Extensions
{
    public class SingletonBehaviour<T> : MonoBehaviour where T: SingletonBehaviour<T>
    {
        public static T Instance { get; protected set; }
     
        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            else
                Instance = (T)this;
        }
    }
}
