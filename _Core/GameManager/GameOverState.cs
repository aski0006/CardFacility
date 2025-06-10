using _Core._interface;
using _Core.FSM;

namespace _Core.GameManager {
    public class GameOverState : FSMState {
        public GameOverState(FSM.FSM fsm) : base(fsm) { }
        public override void OnEnter() {
            EventBus.Instance.Publish(GameState.GameOver, true);
        }
        public override void OnUpdate() { }
        public override void OnExit() {
            EventBus.Instance.Publish(GameState.GameOver, false);
        }
        public override void OnFixedUpdate() { }
    }
}