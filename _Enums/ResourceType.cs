using UnityEngine;

namespace _Enums {
    /// <summary>
    /// 核心资源类型
    /// </summary>
    public enum ResourceType {
        ConceptShards, // 概念碎片 (CS)
        PurifiedLogic, // 纯化逻辑 (PL)
        RealityAnchors, // 现实稳定锚 (RA)
        EntropyPollution, // 熵污染 (EP)
        CognitiveLoad // 认知负荷 (CL)
    }
    
    public static class ResourceTypeExtensions {
        public static string GetResourceName(this ResourceType resourceType) {
            switch (resourceType) {
                case ResourceType.ConceptShards:
                    return "概念碎片";
                case ResourceType.PurifiedLogic:
                    return "纯化逻辑";
                case ResourceType.RealityAnchors:
                    return "现实稳定锚";
                case ResourceType.EntropyPollution:
                    return "熵污染";
                case ResourceType.CognitiveLoad:
                    return "认知负荷";
                default:
                    return "未知资源";
            }
        }
        
        public static Color GetResourceColor(this ResourceType resourceType) {
            switch (resourceType) {
                case ResourceType.ConceptShards:
                    return new Color32(255, 200, 0, 255); // 温暖的金色
                case ResourceType.PurifiedLogic:
                    return new Color32(0, 200, 150, 255); // 蓝绿色
                case ResourceType.RealityAnchors:
                    return new Color32(150, 150, 180, 255); // 柔和的蓝灰色
                case ResourceType.EntropyPollution:
                    return new Color32(220, 50, 50, 255); // 鲜艳的红色
                case ResourceType.CognitiveLoad:
                    return new Color32(180, 80, 220, 255); // 紫色
                default:
                    return Color.white;
            }
        }
    }
}
