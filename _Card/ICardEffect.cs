namespace _Card {
    /// <summary>
    /// 卡牌效果接口
    /// </summary>
    public interface ICardEffect {
        /// <summary>
        /// 应用卡牌效果
        /// </summary>
        void ApplyCardEffect();
        
        /// <summary>
        /// 移除卡牌效果
        /// </summary>
        void RemoveCardEffect();

        /// <summary>
        /// 卡牌放置
        /// </summary>
        void OnCardPlace();

        /// <summary>
        /// 卡牌移除
        /// </summary>
        void OnCardRemove();
    }
}