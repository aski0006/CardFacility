using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Core {
    /// <summary>
    /// 全局事件总线系统（单例）
    /// 
    /// 功能特点：
    /// - 支持字符串和枚举类型的事件类型
    /// - 线程安全的订阅/发布机制
    /// - 自动清理无效订阅（基于MonoBehaviour生命周期）
    /// - 延迟清理机制避免频繁GC
    /// - 提供扩展方法方便MonoBehaviour使用
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class EventBus : SingletonBaseComponent<EventBus> {
        private static bool _applicationIsQuitting = false;

        #region 数据结构

        // 支持字符串和枚举类型的事件
        private Dictionary<string, List<Subscription>> _stringEventSubscribers;
        private Dictionary<Enum, List<Subscription>> _enumEventSubscribers;

        // 延迟清理队列
        private readonly Queue<Action> _pendingCleanupActions = new Queue<Action>();

        /// <summary>
        /// 订阅者信息类
        /// 存储订阅者的详细信息和回调委托
        /// </summary>
        private class Subscription {
            /// <summary>
            /// 订阅者对象
            /// </summary>
            public object Subscriber { get; }

            /// <summary>
            /// 事件回调函数
            /// </summary>
            public Action<object> Callback { get; }

            /// <summary>
            /// 自动清理目标组件
            /// </summary>
            public MonoBehaviour AutoCleanupTarget { get; }

            /// <summary>
            /// 事件类型名称
            /// </summary>
            public string EventType { get; }

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="subscriber">订阅者对象</param>
            /// <param name="callback">回调函数</param>
            /// <param name="autoCleanupTarget">自动清理目标组件</param>
            /// <param name="eventType">事件类型名称</param>
            public Subscription(object subscriber, Action<object> callback, MonoBehaviour autoCleanupTarget, string eventType) {
                Subscriber = subscriber;
                Callback = callback;
                AutoCleanupTarget = autoCleanupTarget;
                EventType = eventType;
            }
        }

        #endregion

        #region 初始化

        /// <summary>
        /// Unity Awake生命周期方法
        /// 处理单例初始化和应用退出状态检测
        /// </summary>
        protected override void Awake() {
            if (_applicationIsQuitting) {
#if UNITY_EDITOR
                Debug.LogWarning("[EventBus] Application is quitting. Instance won't be created.");
#endif
                return;
            }
            base.Awake();
            Initialize();
        }

        /// <summary>
        /// 初始化内部数据结构
        /// </summary>
        private void Initialize() {
            _stringEventSubscribers = new Dictionary<string, List<Subscription>>();
            _enumEventSubscribers = new Dictionary<Enum, List<Subscription>>();
            StartCoroutine(CleanupRoutine());
        }

        #endregion

        #region 核心功能

        /// <summary>
        /// 订阅事件（支持字符串和枚举类型）
        /// </summary>
        /// <param name="eventType">事件类型（字符串或枚举）</param>
        /// <param name="callback">回调函数</param>
        /// <param name="subscriber">订阅者对象</param>
        /// <param name="autoCleanupTarget">自动清理目标组件</param>
        public void Subscribe(object eventType, Action<object> callback, object subscriber, MonoBehaviour autoCleanupTarget = null) {
            if (eventType is string eventString) {
                SubscribeByString(eventString, callback, subscriber, autoCleanupTarget);
            } else if (eventType is Enum eventEnum) {
                SubscribeByEnum(eventEnum, callback, subscriber, autoCleanupTarget);
            } else {
#if UNITY_EDITOR
                Debug.LogError($"[EventBus] Unsupported event type: {eventType.GetType()}. Only string and Enum types are supported.");
#endif
            }
        }

        /// <summary>
        /// 发布事件（支持字符串和枚举类型）
        /// </summary>
        /// <param name="eventType">事件类型（字符串或枚举）</param>
        /// <param name="eventData">事件数据</param>
        public void Publish(object eventType, object eventData = null) {
            if (eventType is string eventString) {
                PublishByString(eventString, eventData);
            } else if (eventType is Enum eventEnum) {
                PublishByEnum(eventEnum, eventData);
            } else {
#if UNITY_EDITOR
                Debug.LogError($"[EventBus] Unsupported event type: {eventType.GetType()}. Only string and Enum types are supported.");
#endif
            }
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="subscriber">订阅者对象</param>
        /// <param name="eventType">事件类型（可选）</param>
        public void Unsubscribe(object subscriber, object eventType = null) {
            if (subscriber == null) {
#if UNITY_EDITOR
                Debug.LogWarning("[EventBus] Cannot unsubscribe with null subscriber.");
#endif
                return;
            }

            if (eventType != null) {
                if (eventType is string eventString) {
                    UnsubscribeFromString(eventString, subscriber);
                } else if (eventType is Enum eventEnum) {
                    UnsubscribeFromEnum(eventEnum, subscriber);
                }
                return;
            }

            UnsubscribeAll(subscriber);
        }

        /// <summary>
        /// 清理无效订阅
        /// </summary>
        public void CleanupInvalidSubscriptions() {
            lock (this) {
                CleanupStringSubscriptions();
                CleanupEnumSubscriptions();
            }
        }

        #endregion

        #region 字符串事件实现

        /// <summary>
        /// 订阅字符串事件
        /// </summary>
        /// <param name="eventType">事件类型字符串</param>
        /// <param name="callback">回调函数</param>
        /// <param name="subscriber">订阅者对象</param>
        /// <param name="autoCleanupTarget">自动清理目标组件</param>
        private void SubscribeByString(string eventType, Action<object> callback, object subscriber, MonoBehaviour autoCleanupTarget) {
            if (string.IsNullOrEmpty(eventType)) {
#if UNITY_EDITOR
                Debug.LogWarning("[EventBus] Cannot subscribe to empty or null event string.");
#endif
                return;
            }

            if (callback == null) {
#if UNITY_EDITOR
                Debug.LogWarning($"[EventBus] Cannot subscribe to '{eventType}' with null callback.");
#endif
                return;
            }

            lock (this) {
                if (!_stringEventSubscribers.ContainsKey(eventType)) {
                    _stringEventSubscribers[eventType] = new List<Subscription>();
                }

                var subscriptions = _stringEventSubscribers[eventType];
                var newSubscription = new Subscription(subscriber, callback, autoCleanupTarget, eventType);

                if (!subscriptions.Contains(newSubscription)) {
                    subscriptions.Add(newSubscription);
                }
            }
        }

        /// <summary>
        /// 发布字符串事件
        /// </summary>
        /// <param name="eventType">事件类型字符串</param>
        /// <param name="eventData">事件数据</param>
        private void PublishByString(string eventType, object eventData) {
            if (string.IsNullOrEmpty(eventType) || !_stringEventSubscribers.ContainsKey(eventType)) {
                return;
            }

            List<Subscription> subscriptionsToInvoke;
            lock (this) {
                subscriptionsToInvoke = new List<Subscription>(_stringEventSubscribers[eventType]);
            }

            foreach (var subscription in subscriptionsToInvoke) {
                try {
                    subscription.Callback?.Invoke(eventData);
                } catch (Exception e) {
#if UNITY_EDITOR
                    Debug.LogError($"[EventBus] Error invoking callback for event '{eventType}': {e}");
#endif
                }
            }
        }

        /// <summary>
        /// 从字符串事件取消订阅
        /// </summary>
        /// <param name="eventType">事件类型字符串</param>
        /// <param name="subscriber">订阅者对象</param>
        private void UnsubscribeFromString(string eventType, object subscriber) {
            if (string.IsNullOrEmpty(eventType) || !_stringEventSubscribers.ContainsKey(eventType)) {
                return;
            }

            lock (this) {
                var subscriptions = _stringEventSubscribers[eventType];
                for (int i = subscriptions.Count - 1; i >= 0; i--) {
                    if (subscriptions[i].Subscriber == subscriber) {
                        subscriptions.RemoveAt(i);
                    }
                }
            }
        }

        #endregion

        #region 枚举事件实现

        /// <summary>
        /// 订阅枚举事件
        /// </summary>
        /// <param name="eventType">枚举类型事件</param>
        /// <param name="callback">回调函数</param>
        /// <param name="subscriber">订阅者对象</param>
        /// <param name="autoCleanupTarget">自动清理目标组件</param>
        private void SubscribeByEnum(Enum eventType, Action<object> callback, object subscriber, MonoBehaviour autoCleanupTarget) {
            if (eventType == null) {
#if UNITY_EDITOR
                Debug.LogWarning("[EventBus] Cannot subscribe to null event enum.");
#endif
                return;
            }

            if (callback == null) {
#if UNITY_EDITOR
                Debug.LogWarning($"[EventBus] Cannot subscribe to '{eventType}' with null callback.");
#endif
                return;
            }

            lock (this) {
                if (!_enumEventSubscribers.ContainsKey(eventType)) {
                    _enumEventSubscribers[eventType] = new List<Subscription>();
                }

                var subscriptions = _enumEventSubscribers[eventType];
                var newSubscription = new Subscription(subscriber, callback, autoCleanupTarget, eventType.ToString());

                if (!subscriptions.Contains(newSubscription)) {
                    subscriptions.Add(newSubscription);
                }
            }
        }

        /// <summary>
        /// 发布枚举事件
        /// </summary>
        /// <param name="eventType">枚举类型事件</param>
        /// <param name="eventData">事件数据</param>
        private void PublishByEnum(Enum eventType, object eventData) {
            if (eventType == null || !_enumEventSubscribers.ContainsKey(eventType)) {
                return;
            }

            List<Subscription> subscriptionsToInvoke;
            lock (this) {
                subscriptionsToInvoke = new List<Subscription>(_enumEventSubscribers[eventType]);
            }

            foreach (var subscription in subscriptionsToInvoke) {
                try {
                    subscription.Callback?.Invoke(eventData);
                } catch (Exception e) {
#if UNITY_EDITOR
                    Debug.LogError($"[EventBus] Error invoking callback for event '{eventType}': {e}");
#endif
                }
            }
        }

        /// <summary>
        /// 从枚举事件取消订阅
        /// </summary>
        /// <param name="eventType">枚举类型事件</param>
        /// <param name="subscriber">订阅者对象</param>
        private void UnsubscribeFromEnum(Enum eventType, object subscriber) {
            if (eventType == null || !_enumEventSubscribers.ContainsKey(eventType)) {
                return;
            }

            lock (this) {
                var subscriptions = _enumEventSubscribers[eventType];
                for (int i = subscriptions.Count - 1; i >= 0; i--) {
                    if (subscriptions[i].Subscriber == subscriber) {
                        subscriptions.RemoveAt(i);
                    }
                }
            }
        }

        #endregion

        #region 订阅管理

        /// <summary>
        /// 移除指定对象的所有订阅
        /// </summary>
        /// <param name="subscriber">订阅者对象</param>
        private void UnsubscribeAll(object subscriber) {
            if (subscriber == null) return;

            lock (this) {
                foreach (var kvp in _stringEventSubscribers) {
                    var subscriptions = kvp.Value;
                    for (int i = subscriptions.Count - 1; i >= 0; i--) {
                        if (subscriptions[i].Subscriber == subscriber) {
                            subscriptions.RemoveAt(i);
                        }
                    }
                }

                foreach (var kvp in _enumEventSubscribers) {
                    var subscriptions = kvp.Value;
                    for (int i = subscriptions.Count - 1; i >= 0; i--) {
                        if (subscriptions[i].Subscriber == subscriber) {
                            subscriptions.RemoveAt(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清理字符串事件的无效订阅
        /// </summary>
        private void CleanupStringSubscriptions() {
            foreach (var eventKey in new List<string>(_stringEventSubscribers.Keys)) {
                var subscriptions = _stringEventSubscribers[eventKey];
                for (int i = subscriptions.Count - 1; i >= 0; i--) {
                    var subscription = subscriptions[i];

                    if (subscription.AutoCleanupTarget == null) {
                        subscriptions.RemoveAt(i);
                        continue;
                    }

                    if (subscription.Subscriber is UnityEngine.Object unityObj && !unityObj) {
                        subscriptions.RemoveAt(i);
                    }
                }

                if (subscriptions.Count == 0) {
                    _stringEventSubscribers.Remove(eventKey);
                }
            }
        }

        /// <summary>
        /// 清理枚举事件的无效订阅
        /// </summary>
        private void CleanupEnumSubscriptions() {
            foreach (var eventKey in new List<Enum>(_enumEventSubscribers.Keys)) {
                var subscriptions = _enumEventSubscribers[eventKey];
                for (int i = subscriptions.Count - 1; i >= 0; i--) {
                    var subscription = subscriptions[i];

                    if (subscription.AutoCleanupTarget == null) {
                        subscriptions.RemoveAt(i);
                        continue;
                    }

                    if (subscription.Subscriber is UnityEngine.Object unityObj && !unityObj) {
                        subscriptions.RemoveAt(i);
                    }
                }

                if (subscriptions.Count == 0) {
                    _enumEventSubscribers.Remove(eventKey);
                }
            }
        }

        /// <summary>
        /// 定期清理协程
        /// 每5秒将清理任务加入队列
        /// </summary>
        /// <returns>IEnumerator</returns>
        private IEnumerator CleanupRoutine() {
            while (true) {
                yield return new WaitForSeconds(5f);
                _pendingCleanupActions.Enqueue(CleanupInvalidSubscriptions);
            }
        }

        /// <summary>
        /// LateUpdate回调
        /// 执行挂起的清理任务
        /// </summary>
        private void LateUpdate() {
            while (_pendingCleanupActions.Count > 0) {
                _pendingCleanupActions.Dequeue()?.Invoke();
            }
        }

        #endregion

        /// <summary>
        /// 应用程序退出回调
        /// 设置退出标志位
        /// </summary>
        private void OnApplicationQuit() {
            _applicationIsQuitting = true;
        }

        /// <summary>
        /// 对象销毁回调
        /// 清理所有资源
        /// </summary>
        private void OnDestroy() {
            lock (this) {
                _stringEventSubscribers?.Clear();
                _enumEventSubscribers?.Clear();
                _pendingCleanupActions.Clear();
            }
        }
    }

    /// <summary>
    /// 事件总线静态扩展方法
    /// 提供便捷的扩展方法供MonoBehaviour使用
    /// </summary>
    public static class EventBusExtensions {
        /// <summary>
        /// 订阅字符串事件
        /// </summary>
        /// <param name="context">调用上下文</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="callback">回调函数</param>
        public static void SubscribeEvent(this MonoBehaviour context, string eventType, Action<object> callback) {
            EventBus.Instance?.Subscribe(eventType, callback, context, context);
        }

        /// <summary>
        /// 订阅枚举事件
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="context">调用上下文</param>
        /// <param name="eventType">枚举事件类型</param>
        /// <param name="callback">回调函数</param>
        public static void SubscribeEvent<T>(this MonoBehaviour context, T eventType, Action<object> callback) where T : Enum {
            EventBus.Instance?.Subscribe(eventType, callback, context, context);
        }

        /// <summary>
        /// 异步订阅字符串事件
        /// </summary>
        /// <param name="context">调用上下文</param>
        /// <param name="eventType">事件类型</param>
        /// <param name="callback">回调函数</param>
        public static async UniTaskVoid SubscribeEventAsync(this MonoBehaviour context, string eventType, Action<object> callback) {
            if (EventBus.Instance == null) await UniTask.WaitUntil(() => EventBus.Instance != null);
            EventBus.Instance?.Subscribe(eventType, callback, context, context);
        }

        /// <summary>
        /// 异步订阅枚举事件
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="context">调用上下文</param>
        /// <param name="eventType">枚举事件类型</param>
        /// <param name="callback">回调函数</param>
        public static async UniTaskVoid SubscribeEventAsync<T>(this MonoBehaviour context, T eventType, Action<object> callback) where T : Enum {
            if (EventBus.Instance == null) await UniTask.WaitUntil(() => EventBus.Instance != null);
            EventBus.Instance?.Subscribe(eventType, callback, context, context);
        }

        /// <summary>
        /// 发布字符串事件
        /// </summary>
        /// <param name="eventType">事件类型</param>
        /// <param name="eventData">事件数据</param>
        public static void PublishEvent(string eventType, object eventData = null) {
            EventBus.Instance?.Publish(eventType, eventData);
        }

        /// <summary>
        /// 发布枚举事件
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="eventType">枚举事件类型</param>
        /// <param name="eventData">事件数据</param>
        public static void PublishEvent<T>(T eventType, object eventData = null) where T : Enum {
            EventBus.Instance?.Publish(eventType, eventData);
        }

        /// <summary>
        /// 取消所有事件订阅
        /// </summary>
        /// <param name="subscriber">订阅者对象</param>
        public static void UnsubscribeAllEvents(this object subscriber) {
            EventBus.Instance?.Unsubscribe(subscriber);
        }

        /// <summary>
        /// 取消字符串事件订阅
        /// </summary>
        /// <param name="subscriber">订阅者对象</param>
        /// <param name="eventType">事件类型</param>
        public static void UnsubscribeEvent(this object subscriber, string eventType) {
            EventBus.Instance?.Unsubscribe(subscriber, eventType);
        }

        /// <summary>
        /// 取消枚举事件订阅
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="subscriber">订阅者对象</param>
        /// <param name="eventType">枚举事件类型</param>
        public static void UnsubscribeEvent<T>(this object subscriber, T eventType) where T : Enum {
            EventBus.Instance?.Unsubscribe(subscriber, eventType);
        }
    }
}