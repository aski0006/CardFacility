using _Card._RealCard._Facility;
using System.Collections.Generic;
using UnityEngine;

namespace _ScriptableObject {
    /// <summary>
    /// 设施卡数据类，用于存储设施卡相关的属性和配置信息。
    /// 继承自 CardDataBase，提供具体的实现。
    /// </summary>
    [CreateAssetMenu(menuName = "CardData/FacilityCard")]
    public class FacilityCardData : CardDataBase {
        /// <summary>
        /// 设施的尺寸（以Vector2Int表示）。
        /// 通常用于定义设施在地图上的占用空间。
        /// </summary>
        [SerializeField] private Vector2Int _facilitySize;

        /// <summary>
        /// 构建设施所需的资源成本列表。
        /// 每个元素是一个FacilityResourceCost对象，包含资源类型和数量。
        /// </summary>
        [SerializeField] private List<FacilityResourceCost> _buildCost = new List<FacilityResourceCost>();

        /// <summary>
        /// 获取设施的尺寸。
        /// </summary>
        public Vector2Int FacilitySize => _facilitySize;

        /// <summary>
        /// 获取构建设施所需的资源成本的只读集合。
        /// 防止外部修改内部列表数据。
        /// </summary>
        public IReadOnlyCollection<FacilityResourceCost> BuildCost => _buildCost.AsReadOnly();

        /// <summary>
        /// 设置卡片在Unity编辑器中的菜单名称。
        /// 覆盖基类方法，为设施卡动态生成特定路径的菜单项。
        /// </summary>
        protected override void SetMenuName() {
            menuName = $"CardData/Facility/{_cardName}";
        }
    }
}
