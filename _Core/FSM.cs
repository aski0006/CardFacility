using _Core._interface;
using _Utils;
using System;
using System.Collections.Generic;

namespace _Core {
    /// <summary>
    /// 状态机管理类，用于管理不同状态之间的切换与更新。
    /// </summary>
    public class FSM : IDisposable {
        private bool debug;
        private IFSMState currentState;
        private Dictionary<Type, IFSMState> states;

        /// <summary>
        /// 初始化一个新的FSM实例。
        /// </summary>
        /// <param name="debug">是否启用调试模式，默认为false。</param>
        public FSM(bool debug = false) {
            this.debug = debug;
            states = new Dictionary<Type, IFSMState>();
        }

        /// <summary>
        /// 注册一个状态到状态机中。
        /// </summary>
        /// <typeparam name="T">要注册的状态类型。</typeparam>
        /// <param name="state">要注册的状态实例。</param>
        public void RegisterState<T>(IFSMState state) where T : IFSMState {
            if (state == null || states.ContainsKey(typeof(T))) {
                #if UNITY_EDITOR
                if (debug)
                    InfoBuilder.Create(LogLevel.Warning)
                        .AppendLine("状态机信息：")
                        .Append("状态已存在或为空，无法注册状态：").Log();
                #endif
                return;
            }
            states.Add(typeof(T), state);
        }

        /// <summary>
        /// 从状态机中注销一个状态。
        /// </summary>
        /// <typeparam name="T">要注销的状态类型。</typeparam>
        public void UnRegisterState<T>() where T : IFSMState {
            if (!states.ContainsKey(typeof(T))) {
                #if UNITY_EDITOR
                if (debug)
                    InfoBuilder.Create(LogLevel.Warning)
                        .AppendLine("状态机信息：")
                        .Append("状态不存在，无法注销状态：").Log();
                #endif
                return;
            }
            states.Remove(typeof(T));
        }

        /// <summary>
        /// 切换当前状态到指定状态。
        /// </summary>
        /// <typeparam name="T">要切换的目标状态类型。</typeparam>
        public void ChangeState<T>() where T : IFSMState {
            if (!states.ContainsKey(typeof(T))) {
                #if UNITY_EDITOR
                if (debug)
                    InfoBuilder.Create(LogLevel.Warning)
                        .AppendLine("状态机信息：")
                        .Append("状态不存在，无法切换状态：").Log();
                #endif
                return;
            }
            currentState?.OnExit();
            currentState = states[typeof(T)];
            currentState.OnEnter();
        }

        /// <summary>
        /// 每帧更新当前状态。
        /// </summary>
        public void Update() {
            if (currentState == null) {
                #if UNITY_EDITOR
                if (debug)
                    InfoBuilder.Create(LogLevel.Warning)
                        .AppendLine("状态机信息：")
                        .Append("当前状态为空，无法更新状态机。").Log();
                #endif
                return;
            }
            currentState.OnUpdate();
        }

        /// <summary>
        /// 固定时间间隔更新当前状态（适用于物理更新）。
        /// </summary>
        public void FixedUpdate() {
            if (currentState == null) {
                #if UNITY_EDITOR
                if (debug)
                    InfoBuilder.Create(LogLevel.Warning)
                        .AppendLine("状态机信息：")
                        .Append("当前状态为空，无法更新状态机。").Log();
                #endif
                return;
            }
            currentState.OnFixedUpdate();
        }
        
        /// <summary>
        /// 清理状态机资源。
        /// </summary>
        public void Dispose() {
            currentState = null;
            states.Clear();
        }
        
        /// <summary>
        /// 设置调试模式。
        /// </summary>
        /// <param name="enable">是否启用调试模式。</param>
        public void SetDebug(bool enable) {
            debug = enable;
        }
    }
}
