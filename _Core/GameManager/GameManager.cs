using _Core._interface;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Core.GameManager {
    public enum GameState {
        GameStart,
        GameLoading,
        GameRunning,
        GamePaused,
        GameOver,
    }
    public class GameManager : SingletonBaseComponent<GameManager> {

        [OnValueChanged(nameof(_fsmDebugCallback))]
        [SerializeField]
        private bool DEBUG;
        private FSM fsm;
        public GameState CurrentGameState { get; private set; }

        protected override void Awake() {
            base.Awake();
            fsm = new FSM(DEBUG);
            fsm.RegisterState<GameStartState>(new GameStartState(fsm));
            fsm.RegisterState<GameLoadingState>(new GameLoadingState(fsm));
            fsm.RegisterState<GameRunningState>(new GameRunningState(fsm));
            fsm.RegisterState<GamePausedState>(new GamePausedState(fsm));
            fsm.RegisterState<GameOverState>(new GameOverState(fsm));
            fsm.ChangeState<GameStartState>();
        }

        private void Update() {
            fsm.Update();
        }

        private void FixedUpdate() {
            fsm.FixedUpdate();
        }

        public void ChangeState<T>() where T : IFSMState {
            fsm.ChangeState<T>();
        }

        private void _fsmDebugCallback(object obj) {
            fsm.SetDebug(DEBUG);
        }
    }
}
