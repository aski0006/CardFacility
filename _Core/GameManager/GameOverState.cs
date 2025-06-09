using _Core._interface;

namespace _Core.GameManager {
    public class GameOverState : IFSMState {
        public void OnEnter() {
            EventBus.Instance.Publish(GameState.GameOver, true);
        }
        public void OnUpdate() { }
        public void OnExit() {
            EventBus.Instance.Publish(GameState.GameOver, false);
        }
        public void OnFixedUpdate() { }
    }
}