using _Core._interface;
using _Core.FSM;

namespace _Core.GameManager {
    public class GameRunningState: FSMState {
        public GameRunningState(FSM.FSM fsm) : base(fsm) { }
        public override void OnEnter() {
            EventBus.Instance.Publish(GameState.GameRunning, true);
        }
        public override void OnUpdate() { }
        public override void OnExit() {
            EventBus.Instance.Publish(GameState.GameRunning, false);
        }
        public override void OnFixedUpdate() { }
    }
}