using UnityEngine;

namespace _Enums {
    /// <summary>
    /// 卡牌稀有度枚举，表示卡牌在游戏中的稀有程度。
    /// </summary>
    public enum CardRarity {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3,
    }

    public static class CardRarityExtensions {
        public static string GetRarityName(this CardRarity cardRarity) {
            return cardRarity switch {
                CardRarity.Common => "普通",
                CardRarity.Rare => "稀有",
                CardRarity.Epic => "史诗",
                CardRarity.Legendary => "传说",
                _ => "未知",
            };
        }

        public static Color GetRarityColor(this CardRarity cardRarity) {
            return cardRarity switch {
                CardRarity.Common => Color.white, // 使用Unity内置的Color.white
                CardRarity.Rare => new Color(0.2f, 0.8f, 0), // 绿色 (0, 255, 0)
                CardRarity.Epic => new Color(0, 0.6f, 1f), // 蓝色 (0, 0, 255)
                CardRarity.Legendary => new Color(1, 0.84f, 0), // 金色近似值 (255, 220, 0)
                _ => Color.white, // 默认白色
            };
        }
    }
}
