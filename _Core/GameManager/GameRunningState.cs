using _Core._interface;

namespace _Core.GameManager {
    public class GameRunningState : IFSMState {
        public void OnEnter() {
            EventBus.Instance.Publish(GameState.GameRunning, true);
        }
        public void OnUpdate() { }
        public void OnExit() {
            EventBus.Instance.Publish(GameState.GameRunning, false);
        }
        public void OnFixedUpdate() { }
    }
}