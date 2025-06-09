using _Core._interface;

namespace _Core.GameManager {
    public class GameLoadingState : IFSMState {
        public void OnEnter() {
            EventBus.Instance.Publish(GameState.GameLoading, true);
        }
        public void OnUpdate() { }
        public void OnExit() {
            EventBus.Instance.Publish(GameState.GameLoading, false);
        }
        public void OnFixedUpdate() { }
    }
}