using _Core;
using _Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace _UI._CommonUIComponents
{
    /// <summary>
    /// 可拖拽的UI组件，支持网格对齐和屏幕边界约束功能。
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        /// <summary>
        /// 自定义的PointerEventData事件类型，用于传递拖拽事件数据。
        /// </summary>
        [System.Serializable]
        public class DragEvent : UnityEvent<PointerEventData> { }

        // 事件回调
        /// <summary>
        /// 在开始拖拽时触发的事件。
        /// </summary>
        public DragEvent onBeginDrag = new DragEvent();

        /// <summary>
        /// 在拖拽过程中持续触发的事件。
        /// </summary>
        public DragEvent onDragging = new DragEvent();

        /// <summary>
        /// 在结束拖拽时触发的事件。
        /// </summary>
        public DragEvent onEndDrag = new DragEvent();

        // 拖拽设置
        /// <summary>
        /// 是否将拖拽限制在屏幕范围内。
        /// </summary>
        public bool constrainToScreen = true;

        /// <summary>
        /// 是否在禁用对象时重置位置到初始位置。
        /// </summary>
        public bool resetPositionOnDisable = true;

        /// <summary>
        /// 是否启用网格对齐功能。
        /// </summary>
        public bool snapToGrid = false; // 新增：网格对齐开关

        private RectTransform rectTransform;
        private Canvas canvas;
        private Vector2 originalPosition;
        private GridSystem gridSystem; // 新增：网格系统引用

        /// <summary>
        /// 初始化组件并获取必要的引用。
        /// </summary>
        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            originalPosition = rectTransform.anchoredPosition;
            gridSystem = FindFirstObjectByType<GridSystem>();
        }

        /// <summary>
        /// 在组件被禁用时，根据配置决定是否重置UI元素的位置。
        /// </summary>
        void OnDisable()
        {
            if (resetPositionOnDisable)
            {
                rectTransform.anchoredPosition = originalPosition;
            }
        }

        /// <summary>
        /// 开始拖拽时调用，触发onBeginDrag事件。
        /// </summary>
        /// <param name="eventData">包含拖拽事件的数据。</param>
        public void OnBeginDrag(PointerEventData eventData)
        {
            onBeginDrag?.Invoke(eventData);
        }

        /// <summary>
        /// 拖拽过程中调用，处理坐标转换、网格对齐和屏幕边界约束逻辑。
        /// </summary>
        /// <param name="eventData">包含拖拽事件的数据。</param>
        public void OnDrag(PointerEventData eventData)
        {
            // 转换坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out Vector2 newPos);

            // 如果启用了网格对齐
            if (snapToGrid && gridSystem != null)
            {
                // 将UI坐标转换为世界坐标
                Vector3 worldPos = canvas.transform.TransformPoint(newPos);

                InfoBuilder.Create().AppendLine("UI坐标：")
                    .Append($"x: {worldPos.x}, y: {worldPos.y}")
                    .Log();

                // 使用网格系统对齐位置
                Grid grid = gridSystem.GetGrid();
                Vector3Int gridPosition = grid.WorldToCell(worldPos);
                if (IsWithinGrid(gridPosition, Vector2Int.one))
                {
                    Vector3 snappedWorldPos = grid.GetCellCenterWorld(gridPosition);

                    // 将对齐后的世界坐标转回UI坐标
                    newPos = canvas.transform.InverseTransformPoint(snappedWorldPos);
                }
                // 如果超出网格边界，使用原始位置
                else
                {
                    newPos = rectTransform.anchoredPosition;
                }
            }


            // 屏幕边界约束
            if (constrainToScreen && (!snapToGrid || !IsWithinGridBounds(newPos)))
            {
                Rect canvasRect = canvas.pixelRect;
                Vector3[] corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);

                // 计算UI元素的边界
                Bounds uiBounds = new Bounds(corners[0], Vector3.zero);
                foreach (Vector3 corner in corners) uiBounds.Encapsulate(corner);

                // 转换为屏幕坐标进行约束
                Vector2 minScreenPos = Camera.main.WorldToScreenPoint(uiBounds.min);
                Vector2 maxScreenPos = Camera.main.WorldToScreenPoint(uiBounds.max);

                newPos.x = Mathf.Clamp(
                    newPos.x,
                    canvasRect.xMin - minScreenPos.x,
                    canvasRect.xMax - maxScreenPos.x
                );
                newPos.y = Mathf.Clamp(
                    newPos.y,
                    canvasRect.yMin - minScreenPos.y,
                    canvasRect.yMax - maxScreenPos.y
                );
            }

            rectTransform.anchoredPosition = newPos;
            onDragging?.Invoke(eventData);
        }

        /// <summary>
        /// 结束拖拽时调用，触发onEndDrag事件。
        /// </summary>
        /// <param name="eventData">包含拖拽事件的数据。</param>
        public void OnEndDrag(PointerEventData eventData)
        {
            onEndDrag?.Invoke(eventData);
        }

        /// <summary>
        /// 公共方法，用于将UI元素重置到初始位置。
        /// </summary>
        public void ResetPosition()
        {
            rectTransform.anchoredPosition = originalPosition;
        }

        /// <summary>
        /// 检查指定的网格位置是否在网格范围内。
        /// </summary>
        /// <param name="gridPosition">要检查的网格坐标。</param>
        /// <param name="size">需要检查的区域大小。</param>
        /// <returns>如果在网格范围内返回true，否则返回false。</returns>
        private bool IsWithinGrid(Vector3Int gridPosition, Vector2Int size)
        {
            return gridSystem != null &&
                gridSystem.IsWithinGrid(gridPosition, size);
        }

        /// <summary>
        /// 检查给定的UI坐标是否对应于网格内的世界坐标。
        /// </summary>
        /// <param name="uiPosition">要检查的UI坐标。</param>
        /// <returns>如果对应的网格位置有效返回true，否则返回false。</returns>
        private bool IsWithinGridBounds(Vector2 uiPosition)
        {
            if (gridSystem == null) return false;

            // 将UI坐标转换为世界坐标
            Vector3 worldPos = canvas.transform.TransformPoint(uiPosition);
            Vector3Int gridPos = gridSystem.GetGrid().WorldToCell(worldPos);

            return IsWithinGrid(gridPos, Vector2Int.one);
        }

        /// <summary>
        /// 动态添加或移除事件监听器的方法集合。
        /// </summary>

        /// <summary>
        /// 添加一个监听器到开始拖拽事件。
        /// </summary>
        /// <param name="action">要添加的监听方法。</param>
        public void AddBeginListener(UnityAction<PointerEventData> action) => onBeginDrag.AddListener(action);

        /// <summary>
        /// 添加一个监听器到拖拽过程事件。
        /// </summary>
        /// <param name="action">要添加的监听方法。</param>
        public void AddDragListener(UnityAction<PointerEventData> action) => onDragging.AddListener(action);

        /// <summary>
        /// 添加一个监听器到结束拖拽事件。
        /// </summary>
        /// <param name="action">要添加的监听方法。</param>
        public void AddEndListener(UnityAction<PointerEventData> action) => onEndDrag.AddListener(action);

        /// <summary>
        /// 从开始拖拽事件中移除一个监听器。
        /// </summary>
        /// <param name="action">要移除的监听方法。</param>
        public void RemoveBeginListener(UnityAction<PointerEventData> action) => onBeginDrag.RemoveListener(action);

        /// <summary>
        /// 从拖拽过程事件中移除一个监听器。
        /// </summary>
        /// <param name="action">要移除的监听方法。</param>
        public void RemoveDragListener(UnityAction<PointerEventData> action) => onDragging.RemoveListener(action);

        /// <summary>
        /// 从结束拖拽事件中移除一个监听器。
        /// </summary>
        /// <param name="action">要移除的监听方法。</param>
        public void RemoveEndListener(UnityAction<PointerEventData> action) => onEndDrag.RemoveListener(action);
    }
}
