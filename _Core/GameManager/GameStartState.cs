using _Core._interface;

namespace _Core.GameManager {
    public class GameStartState : FSMState {
        public GameStartState(FSM fsm) : base(fsm) { }
        public override void OnEnter() {
            EventBus.Instance.Publish(GameState.GameStart, true);
            // TEST: 
            fsm.ChangeState<GameRunningState>();
        }
        public override void OnUpdate() { }
        public override void OnExit() {
            EventBus.Instance.Publish(GameState.GameStart, false);
        }
        public override void OnFixedUpdate() { }
    }
}
