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

        #region Debug 按钮组

        [Button("切换到开始状态")]
        public void ChangeToGameStartState() {
            ChangeState<GameStartState>();
        }
        [Button("切换到加载状态")]
        public void ChangeToGameLoadingState() {
            ChangeState<GameLoadingState>();
        }
        [Button("切换到运行状态")]
        public void ChangeToGameRunningState() {
            ChangeState<GameRunningState>();
        }
        [Button("切换到暂停状态")]
        public void ChangeToGamePausedState() {
            ChangeState<GamePausedState>();
        }
        [Button("切换到结束状态")]
        public void ChangeToGameOverState() {
            ChangeState<GameOverState>();
        }
        [Button("切换到开始状态")]

        #endregion
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
            fsm?.SetDebug(DEBUG);
        }
    }
}
