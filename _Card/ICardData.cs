
using _Enums;
using UnityEngine;

namespace _Card {
    /// <summary>
    /// 接口定义了卡牌数据的基本结构和属性。
    /// </summary>
    public interface ICardData {
        int CardId { get; }
        string CardName { get; }
        string CardDescription { get; }
        CardType CardType { get; }
        CardRarity CardRarity { get; }
        Sprite CardImage { get; }
    }
}
