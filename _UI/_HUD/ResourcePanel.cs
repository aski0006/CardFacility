using _Core;
using _Enums;
using _Utils;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _UI._HUD {
    /// <summary>
    /// 使用数据绑定机制与ResourceManager同步
    /// </summary>
    [Serializable]
    public class TextWithColor {
        [SerializeField] public TextMeshProUGUI text;
        [SerializeField, ColorPalette] public Color singleColor;
    }

    /// <summary>
    /// 全局资源信息显示面板
    /// 实时显示ResourceManager中的全局资源数据
    /// 使用数据绑定机制与ResourceManager同步
    /// </summary>
    public class ResourcePanel : MonoBehaviour {
        [Header("全局资源显示")]
        [SerializeField] private TextWithColor _csText;
        [SerializeField] private TextWithColor _plText;
        [SerializeField] private TextWithColor _raText;
        [SerializeField] private TextWithColor _epText;
        [SerializeField] private TextWithColor _clText;

        [Header("样式设置")]
        [SerializeField] private Color _criticalColor = Color.red;
        [SerializeField] private float _criticalThreshold = 30f;

        private ResourceManager _resourceManager;
        private Dictionary<ResourceType, TextWithColor> _resourceTextMap;

        #region Unity生命周期

        private void Awake() {
            // 初始化资源与UI元素的映射关系
            InitializeResourceMappings();
        }

        private void Start() {
            // 获取ResourceManager单例实例
            _resourceManager = ResourceManager.Instance;

            if (_resourceManager == null) {
                #if UNITY_EDITOR
                InfoBuilder.Create(LogLevel.Error)
                    .AppendLine($"{gameObject.name}")
                    .Append("ResourceManager单例实例为空！").Log();
                #endif
                return;
            }

            // 初始更新UI显示
            UpdateAllResourceDisplays();

            // 订阅资源变更事件
            SubscribeToResourceEvents();
        }

        #endregion

        #region 初始化方法

        /// <summary>
        /// 初始化资源类型与UI文本的映射关系
        /// </summary>
        private void InitializeResourceMappings() {
            // 全局资源映射
            _resourceTextMap = new Dictionary<ResourceType, TextWithColor> {
                { ResourceType.ConceptShards, _csText },
                { ResourceType.PurifiedLogic, _plText },
                { ResourceType.RealityAnchors, _raText },
                { ResourceType.EntropyPollution, _epText },
                { ResourceType.CognitiveLoad, _clText },
            };
        }

        #endregion

        #region 事件订阅管理

        /// <summary>
        /// 订阅资源变更事件
        /// </summary>
        private void SubscribeToResourceEvents() {
            // 订阅全局资源变更事件
            this.SubscribeEvent(EventEnums.ResourceChanged, OnResourceChanged);
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 全局资源变更事件处理
        /// </summary>
        /// <param name="data">资源变更数据字典</param>
        private void OnResourceChanged(object data) {
            if (data is Dictionary<string, object> changeData) {
                // 从事件数据中提取资源类型和新的资源值
                ResourceType resourceType = (ResourceType)changeData["Type"];
                float newValue = (float)changeData["NewValue"];

                // 更新对应的UI元素
                UpdateResourceDisplay(resourceType, newValue);
            }
        }

        #endregion

        #region UI更新方法

        /// <summary>
        /// 更新所有全局资源显示
        /// </summary>
        private void UpdateAllResourceDisplays() {
            foreach (var resourceType in _resourceTextMap.Keys) {
                float value = _resourceManager.GetResource(resourceType);
                UpdateResourceDisplay(resourceType, value);
            }
        }

        /// <summary>
        /// 更新单个全局资源显示
        /// </summary>
        /// <param name="resourceType">资源类型</param>
        /// <param name="value">资源值</param>
        private void UpdateResourceDisplay(ResourceType resourceType, float value) {
            if (_resourceTextMap.TryGetValue(resourceType, out TextWithColor textElement)) {
                // 根据资源类型确定显示格式
                string formattedValue = FormatResourceValue(resourceType, value);
                string displayText = $"{GetResourceName(resourceType)}: {formattedValue}";

                // 设置文本内容
                textElement.text.text = displayText;
                // 特殊资源类型设置颜色警告
                if (resourceType == ResourceType.CognitiveLoad) {
                    textElement.text.color = value > _criticalThreshold ? _criticalColor : textElement.singleColor;
                } else if (resourceType == ResourceType.EntropyPollution) {
                    textElement.text.color = value > _criticalThreshold * 10 ? _criticalColor : textElement.singleColor;
                } else {
                    textElement.text.color = textElement.singleColor;
                }
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取资源类型的友好名称
        /// </summary>
        private string GetResourceName(ResourceType type) {
            switch (type) {
                case ResourceType.ConceptShards: return "概念碎片";
                case ResourceType.PurifiedLogic: return "纯化逻辑";
                case ResourceType.RealityAnchors: return "稳定锚";
                case ResourceType.EntropyPollution: return "熵污染";
                case ResourceType.CognitiveLoad: return "认知负荷";
                default: return type.ToString();
            }
        }

        /// <summary>
        /// 获取资源类型的缩写
        /// </summary>
        private string GetResourceAbbreviation(ResourceType type) {
            switch (type) {
                case ResourceType.ConceptShards: return "(CS)";
                case ResourceType.PurifiedLogic: return "(PL)";
                case ResourceType.RealityAnchors: return "(RA)";
                case ResourceType.EntropyPollution: return "(EP)";
                case ResourceType.CognitiveLoad: return "(CL)";
                default: return "";
            }
        }

        /// <summary>
        /// 格式化资源值显示
        /// </summary>
        private string FormatResourceValue(ResourceType type, float value) {
            // 根据资源类型确定显示格式
            return type switch {
                // 概念碎片和稳定锚显示为整数
                ResourceType.ConceptShards or ResourceType.RealityAnchors =>
                    $"{Mathf.RoundToInt(value)}{GetResourceAbbreviation(type)}",

                // 纯化逻辑显示为1位小数
                ResourceType.PurifiedLogic =>
                    $"{value:F1}{GetResourceAbbreviation(type)}",

                // 熵污染显示为2位小数
                ResourceType.EntropyPollution =>
                    $"{value:F2}{GetResourceAbbreviation(type)}",

                // 认知负荷显示为整数百分比
                ResourceType.CognitiveLoad =>
                    $"{Mathf.RoundToInt(value)}/{Mathf.RoundToInt(_criticalThreshold)}{GetResourceAbbreviation(type)}",

                // 默认显示为1位小数
                _ => $"{value:F1}{GetResourceAbbreviation(type)}"
            };
        }

        #endregion
    }
}
