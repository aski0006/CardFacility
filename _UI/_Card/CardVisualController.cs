using _Card;
using _Enums;
using _UI._CommonUIComponents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _UI._Card {
    /// <summary>
    /// 卡片视觉控制器，用于管理卡片在UI中的视觉表现
    /// 包括设置卡片基本信息、显示/隐藏卡片等功能
    /// </summary>
    public class CardVisualController : MonoBehaviour {
        /// <summary>
        /// 卡片的RectTransform组件，用于控制卡片的位置和大小
        /// </summary>
        [SerializeField] private RectTransform cardRectTransform;

        /// <summary>
        /// 卡片的默认尺寸，默认为60x80
        /// </summary>
        [SerializeField] private Vector2 cardSize = new Vector2(60, 80);

        /// <summary>
        /// 显示卡片名称的文本组件
        /// </summary>
        [SerializeField] private TextMeshProUGUI cardNameText;

        /// <summary>
        /// 显示卡片描述的文本组件
        /// </summary>
        [SerializeField] private TextMeshProUGUI cardDescriptionText;

        /// <summary>
        /// 显示卡片类型的文本组件
        /// </summary>
        [SerializeField] private TextMeshProUGUI cardTypeText;

        /// <summary>
        /// 卡片稀有度边框的图像组件
        /// </summary>
        [SerializeField] private Image cardRarityBorderImage;

        /// <summary>
        /// 卡片图像的精灵组件
        /// </summary>
        [SerializeField] private Image cardImage;

        /// <summary>
        /// 卡片的拖拽组件，用于实现卡片的拖拽功能
        /// </summary>
        [SerializeField] private UIDraggable draggable;

        /// <summary>
        /// 在Start方法中初始化拖拽组件并设置卡片尺寸
        /// </summary>
        private void Start() {
            draggable = GetComponent<UIDraggable>();
            cardRectTransform.sizeDelta = cardSize;
        }

        /// <summary>
        /// 设置卡片的基本信息
        /// 包括名称、描述、类型、稀有度颜色和卡片图像
        /// </summary>
        /// <param name="card">包含卡片信息的BaseCard对象</param>
        public void SetBasicCardInfo(BaseCard card) {
            cardNameText.text = card.CardName;
            cardDescriptionText.text = card.CardDescription;
            cardTypeText.text = card.CardType.GetDescription();
            cardRarityBorderImage.color = card.CardRarity.GetRarityColor();
            cardImage.sprite = card.CardImage;
        }

        /// <summary>
        /// 清空卡片的信息
        /// 将名称、描述、类型设为空，并将颜色和精灵设为默认值
        /// </summary>
        public void ClearCardInfo() {
            cardNameText.text = string.Empty;
            cardDescriptionText.text = string.Empty;
            cardTypeText.text = string.Empty;
            cardRarityBorderImage.color = Color.clear;
            cardImage.sprite = null;
        }

        /// <summary>
        /// 显示卡片
        /// 激活游戏对象
        /// </summary>
        public void Show() {
            transform.gameObject.SetActive(true);
        }

        /// <summary>
        /// 隐藏卡片
        /// 禁用游戏对象
        /// </summary>
        public void Hide() {
            transform.gameObject.SetActive(false);
        }
    }
}
