using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Core {

    /// <summary>
    /// 对象池配置类，用于定义对象池初始化参数。
    /// </summary>
    [Serializable]
    public class PoolSettings {
        public int initialSize = 10;
        public bool useAutoDestroy = true;
        public float autoDestroyTime = 60f; // 默认60秒
        public int expansionThreshold = 5; // 扩容阈值
        public int expansionBatchSize = 10; // 扩容批量大小
    }

    /// <summary>
    /// 泛型对象池实现，支持线程安全操作、自动扩容、空闲销毁、泄漏检测等功能。
    /// </summary>
    /// <typeparam name="T">池中对象类型，必须继承自 Component</typeparam>
    public class ObjectPool<T> : IDisposable where T : Component {
        private bool _enableObjectPoolDebugLog = false; // 控制调试信息输出

        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _pool = new Stack<T>();
        private readonly float _autoDestroyTime;
        private readonly int _initialSize;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly HashSet<T> _activeObjects = new HashSet<T>();
        private readonly HashSet<T> _allObjects = new HashSet<T>(); // 用于泄露检测
        private readonly int _expansionThreshold;
        private readonly int _expansionBatchSize;

        /// <summary>
        /// 启用或禁用调试日志输出。
        /// </summary>
        /// <param name="enable">true 启用；false 禁用</param>
        public void EnableDebug(bool enable) {
            _enableObjectPoolDebugLog = enable;
        }
        
        /// <summary>
        /// 获取当前池中可用对象数量（线程安全）。
        /// </summary>
        public int PoolCount {
            get {
                lock (_pool) {
                    return _pool.Count;
                }
            }
        }

        /// <summary>
        /// 获取当前活跃使用中的对象数量（线程安全）。
        /// </summary>
        public int ActiveCount {
            get {
                lock (_pool) {
                    return _activeObjects.Count;
                }
            }
        }

        /// <summary>
        /// 获取池中所有对象总数量（包含活跃与闲置，线程安全）。
        /// </summary>
        public int AllCount {
            get {
                lock (_pool) {
                    return _allObjects.Count;
                }
            }
        }

        /// <summary>
        /// 构造函数，初始化一个泛型对象池。
        /// </summary>
        /// <param name="prefab">预设体，不可为 null。</param>
        /// <param name="initialSize">初始池容量。</param>
        /// <param name="parent">对象池父级变换（可选）。</param>
        /// <param name="useAutoDestroy">是否启用自动销毁空闲对象。</param>
        /// <param name="autoDestroyTime">空闲对象销毁间隔时间（秒）。</param>
        /// <param name="expansionThreshold">池容量低于该值时触发扩容。</param>
        /// <param name="expansionBatchSize">每次扩容增加的对象数量。</param>
        public ObjectPool(T prefab, int initialSize, Transform parent = null,
            bool useAutoDestroy = false, float autoDestroyTime = 60f,
            int expansionThreshold = 5, int expansionBatchSize = 10) {
            _prefab = prefab ?? throw new ArgumentNullException(nameof(prefab));
            _parent = parent;
            _autoDestroyTime = autoDestroyTime;
            _initialSize = initialSize;
            _cancellationTokenSource = new CancellationTokenSource();
            _expansionThreshold = expansionThreshold;
            _expansionBatchSize = expansionBatchSize;

#if UNITY_EDITOR
            if (_enableObjectPoolDebugLog)
                Debug.Log($"[ObjectPool<{typeof(T).Name}>] 初始化池，初始容量: {initialSize}");
#endif

            // 批量初始化对象池
            for (int i = 0; i < initialSize; i++) {
                AddToPool();
            }

            if (useAutoDestroy) {
                _monitorIdleObjectsAsync(_cancellationTokenSource.Token).Forget();
            }
        }

        /// <summary>
        /// 创建并添加一个新的实例到对象池。
        /// </summary>
        /// <returns>新创建的对象实例</returns>
        private T AddToPool() {
            T instance = Object.Instantiate(_prefab, _parent);
            instance.gameObject.SetActive(false);
            _pool.Push(instance);
            _allObjects.Add(instance);
#if UNITY_EDITOR
            if (_enableObjectPoolDebugLog)
                Debug.Log($"[ObjectPool<{typeof(T).Name}>] 新增对象到池，当前池大小: {_pool.Count}");
#endif
            return instance;
        }

        /// <summary>
        /// 根据配置批量扩容对象池。
        /// </summary>
        private void ExpandPool() {
#if UNITY_EDITOR
            if (_enableObjectPoolDebugLog)
                Debug.Log($"[ObjectPool<{typeof(T).Name}>] 扩容池，批量: {_expansionBatchSize}");
#endif
            for (int i = 0; i < _expansionBatchSize; i++) {
                AddToPool();
            }
        }

        /// <summary>
        /// 获取一个默认位置和旋转的对象实例。
        /// </summary>
        /// <returns>激活后的对象实例</returns>
        public T GetDefault() {
            return Get(Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// 获取一个指定位置和旋转的对象实例。
        /// </summary>
        /// <param name="position">世界坐标系下的位置</param>
        /// <param name="rotation">世界坐标系下的旋转</param>
        /// <returns>激活后的对象实例</returns>
        public T Get(Vector3 position, Quaternion rotation) {
            lock (_pool) {
                if (_pool.Count < _expansionThreshold) {
#if UNITY_EDITOR
                    if (_enableObjectPoolDebugLog)
                        Debug.Log($"[ObjectPool<{typeof(T).Name}>] 低于扩容阈值({_expansionThreshold})，自动扩容");
#endif
                    ExpandPool();
                }

                if (_pool.Count == 0) {
#if UNITY_EDITOR
                    if (_enableObjectPoolDebugLog)
                        Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] 池已空，强制新增对象");
#endif
                    AddToPool();
                }

                T instance = _pool.Pop();
                instance.transform.SetPositionAndRotation(position, rotation);
                instance.gameObject.SetActive(true);
                _activeObjects.Add(instance);

#if UNITY_EDITOR
                if (_enableObjectPoolDebugLog)
                    Debug.Log($"[ObjectPool<{typeof(T).Name}>] 取出对象，池剩余: {_pool.Count}，活跃: {_activeObjects.Count}");
#endif

                if (instance is IPool poolImpl) {
                    poolImpl.OnGetFromPool();
                }

                return instance;
            }
        }

        /// <summary>
        /// 将使用完毕的对象返回到对象池。
        /// </summary>
        /// <param name="instance">需要归还的对象</param>
        public void ReturnToPool(T instance) {
            lock (_pool) {
                if (!instance || !_allObjects.Contains(instance)) {
#if UNITY_EDITOR
                    if (_enableObjectPoolDebugLog)
                        Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] 归还无效对象或对象不属于本池");
#endif
                    return;
                }
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                if (instance is IPool poolImpl) {
                    poolImpl.OnReturnToPool();
                    poolImpl.ResetState();
                }
                instance.gameObject.SetActive(false);
                instance.transform.SetParent(_parent);
                _pool.Push(instance);
                _activeObjects.Remove(instance);
#if UNITY_EDITOR
                if (_enableObjectPoolDebugLog)
                    Debug.Log($"[ObjectPool<{typeof(T).Name}>] 对象归还池，池剩余: {_pool.Count}，活跃: {_activeObjects.Count}");
#endif
            }
        }

        /// <summary>
        /// 销毁指定的对象实例，并从池中移除。
        /// </summary>
        /// <param name="instance">要销毁的对象</param>
        private void DestroyInstance(T instance) {
            _allObjects.Remove(instance);
            Object.Destroy(instance.gameObject);
#if UNITY_EDITOR
            if (_enableObjectPoolDebugLog)
                Debug.Log($"[ObjectPool<{typeof(T).Name}>] 销毁对象，池总数: {_allObjects.Count}");
#endif
        }

        /// <summary>
        /// 异步监控空闲对象，根据设定的时间间隔销毁多余对象。
        /// </summary>
        private async UniTaskVoid _monitorIdleObjectsAsync(CancellationToken token) {
            while (!token.IsCancellationRequested) {
                await UniTask.Delay(TimeSpan.FromSeconds(_autoDestroyTime), cancellationToken: token);

                lock (_pool) {
                    var toDestroy = new List<T>();
                    foreach (T instance in _pool) {
                        if (!_activeObjects.Contains(instance) && _pool.Count > _initialSize) {
                            toDestroy.Add(instance);
                        }
                    }
                    if (toDestroy.Count > 0) {
                        // 先移除要销毁的对象
                        var tempList = _pool.ToList();
                        foreach (T instance in toDestroy) {
                            tempList.Remove(instance);
                            DestroyInstance(instance);
                        }
                        _pool.Clear();
                        for (int i = tempList.Count - 1; i >= 0; i--) {
                            _pool.Push(tempList[i]);
                        }
#if UNITY_EDITOR
                        if (_enableObjectPoolDebugLog)
                            Debug.Log($"[ObjectPool<{typeof(T).Name}>] 自动销毁空闲对象: {toDestroy.Count} 个");
#endif
                    }
                }
            }
        }

        /// <summary>
        /// 检测对象池是否存在泄漏（池内外对象数量不一致视为泄漏）。
        /// </summary>
        /// <returns>true 存在泄漏；false 无泄漏</returns>
        public bool DetectLeaks() {
            lock (_pool) {
                int totalCount = _pool.Count + _activeObjects.Count;
                bool leak = totalCount != _allObjects.Count;
#if UNITY_EDITOR
                if (_enableObjectPoolDebugLog) {
                    if (leak) {
                        Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] 检测到对象池泄漏！池内+活跃: {totalCount}，总对象: {_allObjects.Count}");
                    } else {
                        Debug.Log($"[ObjectPool<{typeof(T).Name}>] 未检测到对象池泄漏。");
                    }
                }
#endif
                return leak;
            }
        }

        /// <summary>
        /// 释放资源，清空对象池并销毁所有对象。
        /// </summary>
        public void Dispose() {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();

            lock (_pool) {
                var allObjectsCopy = new List<T>(_allObjects);
                _activeObjects.Clear();
                _pool.Clear();
                _allObjects.Clear();

                // 销毁所有对象（操作副本，不涉及内部集合）
                foreach (T instance in allObjectsCopy.Where(instance => instance != null)) {
                    Object.Destroy(instance.gameObject);
                }
#if UNITY_EDITOR
                if (_enableObjectPoolDebugLog)
                    Debug.Log($"[ObjectPool<{typeof(T).Name}>] 池已释放并销毁所有对象。");
#endif
            }
        }

        /// <summary>
        /// 获取父级对象的父级变换组件。
        /// </summary>
        /// <returns>父级变换</returns>
        public Transform GetParentTransform() {
            return _parent.parent.transform;
        }
    }
}