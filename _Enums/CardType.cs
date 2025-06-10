namespace _Enums {
    /// <summary>
    /// 卡牌类型枚举，用于区分不同种类的卡牌功能和用途。
    /// </summary>
    public enum CardType {
        // 资源卡牌
        Resource,
        // 模因特征卡（异常特征卡）
        MemeTrait,
        // 收容流程卡
        Containment,
        // 设备卡（逻辑提取器/精炼矩阵等）
        Facility,
        // 战斗卡（净化卡）
        Combat,
        // 强化卡
        Enhancement,
        // 剧情卡
        Story,
        // 角色成长卡
        CharacterGrowth,
        // 科技卡
        Technology,
    }

    public static class CardTypeExtensions {
        public static string GetDescription(this CardType cardType) {
            return cardType switch {
                CardType.Resource => "资源",
                CardType.MemeTrait => "模因特征",
                CardType.Containment => "收容流程",
                CardType.Facility => "设备",
                CardType.Combat => "战斗",
                CardType.Enhancement => "强化",
                CardType.Story => "剧情",
                CardType.CharacterGrowth => "角色成长",
                CardType.Technology => "科技",
                _ => "未知",
            };
        }
    }
}
