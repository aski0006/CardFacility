using _Core._interface;

namespace _Core.GameManager {
    public class GamePausedState : IFSMState {
        public void OnEnter() {
            EventBus.Instance.Publish(GameState.GamePaused, true);
        }
        public void OnUpdate() { }
        public void OnExit() {
            EventBus.Instance.Publish(GameState.GamePaused, false);
        }
        public void OnFixedUpdate() { }
    }
}