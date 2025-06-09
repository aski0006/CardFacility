using _Core._interface;

namespace _Core.GameManager {
    public class GameLoadingState : FSMState {

        public GameLoadingState(FSM fsm) : base(fsm) { }
        public override void OnEnter() {
            EventBus.Instance.Publish(GameState.GameLoading, true);
        }
        public override void OnUpdate() { }
        public override void OnExit() {
            EventBus.Instance.Publish(GameState.GameLoading, false);
        }
        public override void OnFixedUpdate() { }
    }
}
