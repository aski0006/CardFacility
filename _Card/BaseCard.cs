using _Enums;
using _ScriptableObject;
using UnityEngine;

namespace _Card {

    public abstract class BaseCard : MonoBehaviour, ICardEffect {
        [SerializeField] private CardDataBase _cardData;

        #region 属性

        public int CardId => _cardData.CardId;
        public string CardName => _cardData.CardName;
        public string CardDescription => _cardData.CardDescription;
        public CardType CardType => _cardData.CardType;
        public CardRarity CardRarity => _cardData.CardRarity;
        public Sprite CardImage => _cardData.CardImage;
        public CardDataBase CardData => _cardData;

        #endregion

        #region Unity 生命周期方法

        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void Update() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }

        #endregion

        public abstract void ApplyCardEffect();
        public abstract void RemoveCardEffect();
        public abstract void OnCardPlace();
        public abstract void OnCardRemove();
    }
}
