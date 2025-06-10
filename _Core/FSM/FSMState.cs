using _Core._interface;

namespace _Core.FSM {
    /// <summary>
    /// 状态机基类，用于定义状态的基本行为和依赖注入方法。
    /// 所有具体的状态类都应继承此类并实现相应的状态行为方法。
    /// </summary>
    public abstract class FSMState : IFSMState {
        protected _Core.FSM.FSM fsm;

        /// <summary>
        /// 状态构造函数，用于初始化状态机实例。
        /// </summary>
        /// <param name="fsm">状态机实例</param>
        protected FSMState(_Core.FSM.FSM fsm) { this.fsm = fsm; }
        /// <summary>
        /// 当进入此状态时调用，用于初始化逻辑。
        /// </summary>
        public abstract void OnEnter();

        /// <summary>
        /// 每帧更新逻辑，用于处理状态的持续行为。
        /// </summary>
        public abstract void OnUpdate();

        /// <summary>
        /// 当退出此状态时调用，用于清理资源或保存状态信息。
        /// </summary>
        public abstract void OnExit();

        /// <summary>
        /// 每物理帧调用一次，用于处理与物理引擎相关的逻辑。
        /// </summary>
        public abstract void OnFixedUpdate();
    }
}
