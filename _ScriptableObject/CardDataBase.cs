using _Card;
using _Enums;
using _Utils;
using UnityEngine;

namespace _ScriptableObject {
    /// <summary>
    /// 卡牌数据基类，用于存储卡牌的基础信息和属性。
    /// </summary>
    [NotShowInDatabase]
    public abstract class CardDataBase : ScriptableObjectBase , ICardData{
        [SerializeField] protected int _cardId;
        [SerializeField] protected string _cardName;
        [SerializeField] protected string _cardDescription;
        [SerializeField] protected CardType _cardType;
        [SerializeField] protected CardRarity _cardRarity;
        [SerializeField] protected Sprite _cardImage;

        #region 属性

        /// <summary>
        /// 卡牌的唯一标识符。
        /// </summary>
        public int CardId => _cardId;

        /// <summary>
        /// 卡牌的名称。
        /// </summary>
        public string CardName => _cardName;

        /// <summary>
        /// 卡牌的描述信息。
        /// </summary>
        public string CardDescription => _cardDescription;

        /// <summary>
        /// 卡牌的类型。
        /// </summary>
        public CardType CardType => _cardType;

        /// <summary>
        /// 卡牌的稀有度。
        /// </summary>
        public CardRarity CardRarity => _cardRarity;

        /// <summary>
        /// 卡牌的图像资源。
        /// </summary>
        public Sprite CardImage => _cardImage;

    #endregion

        protected override void SetMenuName() {
            menuName = $"CardData/{GetType().Name}";
        }
    }
}
