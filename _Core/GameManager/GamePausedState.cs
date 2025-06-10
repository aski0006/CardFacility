using _Core._interface;
using _Core.FSM;

namespace _Core.GameManager {
    public class GamePausedState : FSMState {
        public GamePausedState(FSM.FSM fsm) : base(fsm) { }
        public override void OnEnter() {
            EventBus.Instance.Publish(GameState.GamePaused, true);
        }
        public override void OnUpdate() { }
        public override void OnExit() {
            EventBus.Instance.Publish(GameState.GamePaused, false);
        }
        public override void OnFixedUpdate() { }
    }
}