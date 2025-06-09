using _Core._interface;

namespace _Core.GameManager {
    public class GameStartState : IFSMState {
        public void OnEnter() {
            EventBus.Instance.Publish(GameState.GameStart, true);
        }
        public void OnUpdate() { }
        public void OnExit() {
            EventBus.Instance.Publish(GameState.GameStart, false);
        }
        public void OnFixedUpdate() { }
    }
}
