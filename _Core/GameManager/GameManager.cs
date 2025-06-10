using _Core._interface;
using _Utils;
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

        [SerializeField] private bool DEBUG;
        [ShowInInspector] public GameState CurrentGameState { get; private set; }
        private FSM.FSM fsm;

        #region Debug 按钮组

        [Button("切换到开始状态"), ShowIf("DEBUG")]
        public void ChangeToGameStartState() {
            ChangeState<GameStartState>();
        }
        [Button("切换到加载状态"), ShowIf("DEBUG")]
        public void ChangeToGameLoadingState() {
            ChangeState<GameLoadingState>();
        }
        [Button("切换到运行状态"), ShowIf("DEBUG")]
        public void ChangeToGameRunningState() {
            ChangeState<GameRunningState>();
        }
        [Button("切换到暂停状态"), ShowIf("DEBUG")]
        public void ChangeToGamePausedState() {
            ChangeState<GamePausedState>();
        }
        [Button("切换到结束状态"), ShowIf("DEBUG")]
        public void ChangeToGameOverState() {
            ChangeState<GameOverState>();
        }

        #endregion
        protected override void Awake() {
            base.Awake();
            fsm = new FSM.FSM(DEBUG);
            fsm.RegisterState<GameStartState>(new GameStartState(fsm));
            fsm.RegisterState<GameLoadingState>(new GameLoadingState(fsm));
            fsm.RegisterState<GameRunningState>(new GameRunningState(fsm));
            fsm.RegisterState<GamePausedState>(new GamePausedState(fsm));
            fsm.RegisterState<GameOverState>(new GameOverState(fsm));
            fsm.ChangeState<GameStartState>();

            #if UNITY_EDITOR
            if (DEBUG)
                InfoBuilder.Create()
                    .AppendLine("游戏管理器初始化....")
                    .AppendLine($"FSM状态机调试启用：{fsm.GetDebug()}").Log();
            #endif
        }

        private void Update() {
            fsm?.Update();
        }

        private void FixedUpdate() {
            fsm?.FixedUpdate();
        }

        public void ChangeState<T>() where T : IFSMState {
            fsm?.ChangeState<T>();
        }

    }
}
