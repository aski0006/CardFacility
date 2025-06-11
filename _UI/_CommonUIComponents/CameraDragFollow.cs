using _Core;
using UnityEngine;
using DG.Tweening; // 引入DOTween命名空间

namespace _UI._CommonUIComponents {
    [RequireComponent(typeof(Camera))]
    public class CameraDragFollow : MonoBehaviour {
        [Header("跟随设置")]
        [Tooltip("摄像机跟随速度（秒）")]
        [SerializeField] private float followDuration = 0.3f;

        [Tooltip("最大跟随距离（避免过度移动）")]
        [SerializeField] private float maxFollowDistance = 10f;

        private Camera _mainCamera;
        private bool _isDragging;
        private Tween _moveTween; // DOTween动画实例
        private Vector3 _dragStartPosition; // 拖拽开始时的位置

        private void Awake() {
            _mainCamera = GetComponent<Camera>();

            DOTween.Init();
            DOTween.SetTweensCapacity(200, 50);

        }

        public void OnBeginDrag() {
            _isDragging = true;
            _dragStartPosition = transform.position;

            // 停止任何正在进行的动画
            _moveTween?.Kill();
        }

        public void OnEndDrag() {
            _isDragging = false;
        }

        private void Update() {
            if (!_isDragging) return;

            // 将鼠标位置转换为世界坐标
            Vector2 mouseScreenPos = Input.mousePosition;
            Vector3 targetWorldPosition = _mainCamera.ScreenToWorldPoint(
                new Vector3(mouseScreenPos.x, mouseScreenPos.y, transform.position.z - _mainCamera.transform.position.z)
            );

            // 限制跟随距离（避免拖拽时摄像机移动过大）
            float distance = Vector3.Distance(_dragStartPosition, targetWorldPosition);
            if (distance > maxFollowDistance) {
                targetWorldPosition = _dragStartPosition +
                    (targetWorldPosition - _dragStartPosition).normalized * maxFollowDistance;
            }

            // 使用DOTween实现平滑跟随
            FollowTarget(targetWorldPosition);
        }

        /// <summary>
        /// 使用DOTween平滑跟随目标位置
        /// </summary>
        private void FollowTarget(Vector3 targetPosition) {
            // 如果已经有动画在运行，则停止它
            if (_moveTween != null && _moveTween.IsActive()) {
                _moveTween.Kill();
            }

            // 创建新的平滑移动动画
            _moveTween = transform.DOMove(
                    new Vector3(targetPosition.x, targetPosition.y, transform.position.z),
                    followDuration
                )
                .SetEase(Ease.OutQuad) // 使用缓动函数使移动更自然
                .SetUpdate(UpdateType.Manual); // 手动更新以精确控制

            // 手动更新DOTween（确保在每帧渲染前更新）
            _moveTween.ManualUpdate(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy() {
            // 清理DOTween实例
            _moveTween?.Kill();
        }
    }
}
