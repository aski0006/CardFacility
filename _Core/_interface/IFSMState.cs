namespace _Core._interface {
    /// <summary>
    /// 状态机状态接口
    /// </summary>
    public interface IFSMState {
        void OnEnter();
        void OnUpdate();
        void OnExit();
        void OnFixedUpdate();
    }
}
