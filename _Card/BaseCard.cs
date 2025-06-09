using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace _Card {

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

    /// <summary>
    /// 卡牌稀有度枚举，表示卡牌在游戏中的稀有程度。
    /// </summary>
    public enum CardRarity {
        Common,
        Rare,
        Epic,
        Legendary,
    }

    /// <summary>
    /// BaseCard 是所有卡牌类的基类，提供通用的卡牌属性和抽象方法。
    /// 该类实现了 ICardEffect 接口，定义了卡牌效果的基本行为。
    /// </summary>
    [Serializable]
    public abstract class BaseCard : MonoBehaviour, ICardEffect {
        /// <summary>
        /// 卡牌的唯一标识符。
        /// </summary>
        [SerializeField] private int cardId;

        /// <summary>
        /// 卡牌的名称。
        /// </summary>
        [SerializeField] private string cardName;

        /// <summary>
        /// 卡牌的描述文本。
        /// </summary>
        [SerializeField] private string cardDescription;

        /// <summary>
        /// 卡牌的类型，由 CardType 枚举定义。
        /// </summary>
        [SerializeField] private CardType cardType;

        /// <summary>
        /// 卡牌的稀有度，由 CardRarity 枚举定义。
        /// </summary>
        [SerializeField] private CardRarity cardRarity;

        #region 属性

        /// <summary>
        /// 获取卡牌的唯一标识符。
        /// </summary>
        public int CardId {
            get => cardId;
            set => cardId = value;
        }

        /// <summary>
        /// 获取卡牌的名称。
        /// </summary>
        public string CardName {
            get => cardName;
            set => cardName = value;
        }

        /// <summary>
        /// 获取卡牌的描述文本。
        /// </summary>
        public string CardDescription {
            get => cardDescription;
            set => cardDescription = value;
        }

        /// <summary>
        /// 获取卡牌的类型。
        /// </summary>
        public CardType CardType {
            get => cardType;
            set => cardType = value;
        }

        /// <summary>
        /// 获取卡牌的稀有度。
        /// </summary>
        public CardRarity CardRarity {
            get => cardRarity;
            set => cardRarity = value;
        }

        #endregion

        #region Unity 生命周期方法

        protected void Awake() { }
        protected void Start() { }
        protected void Update() { }
        protected void OnDestroy() { }
        protected void OnEnable() { }
        protected void OnDisable() { }

        #endregion

        /// <summary>
        /// 应用卡牌效果的抽象方法，子类必须实现该方法。
        /// </summary>
        public abstract void ApplyCardEffect();

        /// <summary>
        /// 移除卡牌效果的抽象方法，子类必须实现该方法。
        /// </summary>
        public abstract void RemoveCardEffect();

        /// <summary>
        /// 当卡牌被放置时调用的抽象方法，子类可以重写该方法以执行特定的操作。
        /// </summary>
        public abstract void OnCardPlace();

        /// <summary>
        /// 当卡牌被移除时调用的抽象方法，子类可以重写该方法以执行特定的操作。
        /// </summary>
        public abstract void OnCardRemove();
    }
}
