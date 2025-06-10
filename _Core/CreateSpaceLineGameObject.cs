using _Utils;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;

namespace _Core
{
    /// <summary>
    /// 空间线对象创建工具类
    /// 提供在编辑器中创建空GameObject和空间线标识对象的功能
    /// </summary>
    [ExecuteInEditMode]
    public class CreateSpaceLineGameObject : MonoBehaviour
    {
        [SerializeField] private int spaceLineCount = 10;
        private const string Begin = "[begin]";
        private const string End = "[end]";
        private const string SpaceLine = "-";

        /// <summary>
        /// 创建一个空的游戏对象
        /// 对象名称为"Empty GameObject"
        /// 继承当前对象的父级和局部位置
        /// </summary>
        [Button("创建空游戏对象")]
        public void CreateEmptyGameObject()
        {
            GameObject go = new GameObject("Empty GameObject")
            {
                transform =
                {
                    parent = transform.parent,
                    localPosition = Vector3.zero,
                },
            };
        }

        /// <summary>
        /// 创建空间线标识对象
        /// 生成带有连字符线的开始和结束标识对象
        /// 日志记录创建过程
        /// </summary>
        [Button("创建空间线")]
        public void CreateSpaceLine()
        {
            InfoBuilder.Create().AppendLine($"{gameObject.name} 创建空间线").Log();
            string spaceLine = string.Concat(Enumerable.Repeat(SpaceLine, spaceLineCount));
            // 创建开始空物体
            string objName = Begin + spaceLine + End;
            GameObject beginObject = new GameObject(objName)
            {
                transform =
                {
                    parent = transform.parent,
                    localPosition = Vector3.zero,
                },
            };
        }
    }
}
