using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace _ScriptableObject {
    /// <summary>
    /// 基础 ScriptableObject 类，用于派生其他自定义 ScriptableObject 类型。
    /// 提供了获取所有子类的功能，并定义了菜单名称设置的抽象接口。
    /// </summary>
    public abstract class ScriptableObjectBase : ScriptableObject {
        protected string menuName;

        /// <summary>
        /// 获取当前程序集中所有继承自 ScriptableObjectBase 的子类类型数组。
        /// </summary>
        /// <returns>所有子类的 Type 数组</returns>
        public static Type[] GetAllSubclasses() {
            return Assembly.GetAssembly(typeof(ScriptableObjectBase))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ScriptableObjectBase)))
                .ToArray();
        }

        /// <summary>
        /// 子类必须实现的方法，用于设置菜单名称。
        /// </summary>
        protected abstract void SetMenuName();

        /// <summary>
        /// 获取已设置的菜单名称。
        /// </summary>
        /// <returns>菜单名称字符串</returns>
        public string GetMenuName() {
            return menuName;
        }
    }
}
