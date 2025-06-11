using _Card._RealCard._Facility;
using System.Collections.Generic;
using System.Linq;
using _Enums;
using UnityEngine;

namespace _Core {
    /// <summary>
    /// 网格系统单例类，管理游戏世界的网格布局和设施放置
    /// </summary>
    [RequireComponent(typeof(Grid))]
    public class GridSystem : SingletonBaseComponent<GridSystem> {
        [Header("Grid Configuration")]
        [SerializeField] private Grid _grid;
        [SerializeField] private int _gridWidth = 100;
        [SerializeField] private int _gridHeight = 100;

        private Dictionary<Vector3Int, BaseFacility> _occupiedCells = new Dictionary<Vector3Int, BaseFacility>();
        private Dictionary<RegionType, List<Vector3Int>> _regionCells = new Dictionary<RegionType, List<Vector3Int>>();
        private Foundation _activeFoundation;
        public int GridWidth => _gridWidth;
        public int GridHeight => _gridHeight;
        protected override void Awake() {
            base.Awake();
            InitializeGrid();
        }

        private void InitializeGrid() {
            if (_grid == null) _grid = GetComponent<Grid>();
            _regionCells = new Dictionary<RegionType, List<Vector3Int>>() {
                { RegionType.Containment, new List<Vector3Int>() },
                { RegionType.Processing, new List<Vector3Int>() },
                { RegionType.Purification, new List<Vector3Int>() }
            };
        }

        /// <summary>
        /// 拓展整个网格
        /// </summary>
        /// <param name="deltaX">横向扩展量</param>
        /// <param name="deltaY">纵向扩展量</param>
        public void ExpandGrid(int deltaX, int deltaY) {
            _gridWidth += deltaX;
            _gridHeight += deltaY;
        }





        /// <summary>
        /// 放置地基设施
        /// </summary>
        public bool PlaceFoundation(Vector3 worldPosition, Foundation foundation) {
            Vector3Int gridPosition = _grid.WorldToCell(worldPosition);
            Vector3 alignedPosition = _grid.GetCellCenterWorld(gridPosition);

            // 检查是否可以放置（地基可以重叠）
            if (!CanPlaceFacility(gridPosition, foundation))
                return false;

            // 设置地基位置
            foundation.transform.position = _grid.CellToWorld(gridPosition);
            foundation.CurrentFacilityZoneCenter = new Vector2Int(gridPosition.x, gridPosition.y);

            // 如果是新的地基平面，设置为活动地基
            if (_activeFoundation == null)
                _activeFoundation = foundation;

            // 更新占用信息
            MarkCellsAsOccupied(gridPosition, foundation, false);
            foundation.transform.position = alignedPosition;
            return true;
        }

        /// <summary>
        /// 扩建地基区域
        /// </summary>
        public void ExpandFoundation(int deltaX, int deltaY) {
            if (_activeFoundation == null) return;

            _activeFoundation.ResetFacilityZoneSize(deltaX, deltaY);
            // 更新网格占用状态
            RefreshFoundationGrid();
        }

        /// <summary>
        /// 放置其他设施
        /// </summary>
        public bool PlaceFacility(Vector3 worldPosition, BaseFacility facility) {
            Vector3Int gridPosition = _grid.WorldToCell(worldPosition);

            // 检查是否可以放置（必须在地基上且不重叠）
            if (!CanPlaceFacility(gridPosition, facility))
                return false;

            // 设置设施位置
            facility.transform.position = _grid.CellToWorld(gridPosition);
            facility.CurrentFacilityZoneCenter = new Vector2Int(gridPosition.x, gridPosition.y);

            // 更新占用信息
            MarkCellsAsOccupied(gridPosition, facility, true);

            return true;
        }

        /// <summary>
        /// 定义区域（收容区、加工区等）
        /// </summary>
        public void DefineRegion(Vector3 startWorldPos, Vector3 endWorldPos, RegionType regionType) {
            Vector3Int startCell = _grid.WorldToCell(startWorldPos);
            Vector3Int endCell = _grid.WorldToCell(endWorldPos);

            // 清除该区域原有定义
            _regionCells[regionType].Clear();

            // 定义矩形区域
            int minX = Mathf.Min(startCell.x, endCell.x);
            int maxX = Mathf.Max(startCell.x, endCell.x);
            int minY = Mathf.Min(startCell.y, endCell.y);
            int maxY = Mathf.Max(startCell.y, endCell.y);

            for (int x = minX; x <= maxX; x++) {
                for (int y = minY; y <= maxY; y++) {
                    Vector3Int cell = new Vector3Int(x, y, 0);

                    // 确保区域在地基范围内
                    if (IsFoundationCell(cell)) {
                        _regionCells[regionType].Add(cell);
                    }
                }
            }
        }

        private bool CanPlaceFacility(Vector3Int gridPosition, BaseFacility facility) {
            // 检查是否超出网格边界
            if (!IsWithinGrid(gridPosition, facility.FacilitySize))
                return false;

            // 对于地基，允许重叠放置
            if (facility is Foundation)
                return true;

            // 对于其他设施，检查是否在地基上且没有冲突
            return IsFoundationCell(gridPosition) && !IsCellOccupied(gridPosition);
        }
        public void RecycleFoundation(Foundation foundation) {
            if (foundation == null || _activeFoundation != foundation) return;

            // 清除地基关联的网格区域
            Vector2Int center = _activeFoundation.CurrentFacilityZoneSize;
            Vector2Int size = _activeFoundation.FacilitySize;
            Vector2Int offset = size / 2;

            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    Vector3Int cell = new Vector3Int(
                        center.x - offset.x + x,
                        center.y - offset.y + y,
                        0);

                    // 移除此地基的所有区域标记
                    foreach (var region in _regionCells.Keys.ToList()) {
                        _regionCells[region].Remove(cell);
                    }
                }
            }

            // 清除占用信息
            _activeFoundation = null;

            // 回收游戏对象
            Destroy(foundation.gameObject);
        }
        private void MarkCellsAsOccupied(Vector3Int center, BaseFacility facility, bool isExclusive) {
            Vector2Int size = facility.FacilitySize;
            Vector2Int offset = size / 2;

            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    Vector3Int cell = new Vector3Int(
                        center.x - offset.x + x,
                        center.y - offset.y + y,
                        0);

                    if (isExclusive) {
                        _occupiedCells[cell] = facility;
                    }
                    // 地基不占用网格，只标记为可建造区域
                }
            }
        }

        private void RefreshFoundationGrid() {
            // 清除所有地基标记
            ClearFoundationGrid();

            if (_activeFoundation == null) return;

            // 重新标记活动地基区域
            Vector2Int center = _activeFoundation.CurrentFacilityZoneSize;
            Vector2Int size = _activeFoundation.FacilitySize;
            Vector2Int offset = size / 2;

            for (int x = 0; x < size.x; x++) {
                for (int y = 0; y < size.y; y++) {
                    Vector3Int cell = new Vector3Int(
                        center.x - offset.x + x,
                        center.y - offset.y + y,
                        0);

                    // 标记为地基单元格
                    // （在实际实现中，这里可能需要一个专门的数据结构来存储地基信息）
                }
            }
        }

        private void ClearFoundationGrid() {
            // 清除地基网格标记
            // （在实际实现中，这里可能需要清除地基数据结构）
        }

        public bool IsWithinGrid(Vector3Int position, Vector2Int size) {
            Vector2Int offset = size / 2;
            int minX = position.x - offset.x;
            int maxX = position.x + offset.x;
            int minY = position.y - offset.y;
            int maxY = position.y + offset.y;

            return minX >= -_gridWidth && maxX < _gridWidth &&
                minY >= -_gridHeight && maxY < _gridHeight;
        }

        private bool IsCellOccupied(Vector3Int cell) {
            return _occupiedCells.ContainsKey(cell);
        }

        private bool IsFoundationCell(Vector3Int cell) {
            // 在实际实现中，这里需要检查该单元格是否在地基范围内
            return _activeFoundation != null &&
                IsCellInFacilityRange(cell, _activeFoundation);
        }

        private bool IsCellInFacilityRange(Vector3Int cell, BaseFacility facility) {
            Vector2Int center = facility.CurrentFacilityZoneSize;
            Vector2Int size = facility.FacilitySize;
            Vector2Int offset = size / 2;

            return cell.x >= center.x - offset.x &&
                cell.x <= center.x + offset.x &&
                cell.y >= center.y - offset.y &&
                cell.y <= center.y + offset.y;
        }

        /// <summary>
        /// 获取指定区域类型的所有单元格
        /// </summary>
        public List<Vector3Int> GetRegionCells(RegionType regionType) {
            return _regionCells.ContainsKey(regionType) ? _regionCells[regionType] : new List<Vector3Int>();
        }

        #region Gizmo

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(_gridWidth, _gridHeight, 1));
        }

        #endregion
        public Grid GetGrid() => _grid;
    }
}
