using UnityEngine;

namespace _Core {
    
    /// <summary>
    /// 泛型单例基类，用于创建和管理Unity中的单例组件。
    /// 继承此类的子类会在场景中确保只有一个实例存在，并在切换场景时不被销毁。
    /// </summary>
    /// <typeparam name="T">继承自MonoBehaviour的具体单例组件类型</typeparam>
    public abstract class SingletonBaseComponent<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object Lock = new object();
    
        /// <summary>
        /// 获取当前单例的实例。如果实例不存在，则会自动创建一个新的实例。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Lock)
                    {
                        if (_instance == null) {
                            _instance = FindFirstObjectByType<T>();
                            if (_instance == null)
                            {
                                GameObject singletonObj = new GameObject(typeof(T).Name);
                                _instance = singletonObj.AddComponent<T>();
                            }
                        }
                    }
                }
                return _instance;
            }
        }
    
        /// <summary>
        /// 在对象唤醒时调用。用于初始化单例实例，并确保只有一个实例存在。
        /// </summary>
        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
