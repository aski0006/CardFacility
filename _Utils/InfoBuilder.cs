using System.Text;
using UnityEngine;

namespace _Utils {
    /// <summary>
    /// 定义日志输出等级，用于控制信息显示的严重程度。
    /// </summary>
    public enum LogLevel {
        /// <summary>信息级别</summary>
        Info,
        /// <summary>警告级别</summary>
        Warning,
        /// <summary>错误级别</summary>
        Error,
    }

    /// <summary>
    /// 提供一个构建带格式文本消息的工具类，支持链式调用和日志输出。
    /// 
    /// 使用示例:
    /// <code>
    /// InfoBuilder.Create(LogLevel.Info)
    ///     .AppendLine("游戏状态:")
    ///     .Append("玩家生命值：").Append(playerHealth.ToString())
    ///     .AppendLine()
    ///     .Append("当前关卡：").Append(currentLevelName)
    ///     .Log();
    /// </code>
    /// </summary>
    public class InfoBuilder {
        private readonly StringBuilder _builder = new StringBuilder();
        private LogLevel _level = LogLevel.Info;

        /// <summary>
        /// 构造函数，初始化一个新的 <see cref="InfoBuilder"/> 实例。
        /// </summary>
        /// <param name="level">日志等级，默认为 Info。</param>
        public InfoBuilder(LogLevel level = LogLevel.Info) {
            _level = level;
            // 设置前缀颜色
            AppendColorTag(GetColorForLevel(level));
        }

        /// <summary>
        /// 根据日志等级返回对应的颜色字符串。
        /// </summary>
        /// <param name="level">指定的日志等级。</param>
        /// <returns>颜色名称字符串。</returns>
        private string GetColorForLevel(LogLevel level) {
            switch (level) {
                case LogLevel.Info: return "white";
                case LogLevel.Warning: return "yellow";
                case LogLevel.Error: return "red";
                default: return "white";
            }
        }

        /// <summary>
        /// 在构建的文本中添加颜色标签以改变Unity控制台输出的颜色。
        /// </summary>
        /// <param name="color">要设置的颜色名称。</param>
        private void AppendColorTag(string color) {
            _builder.Append($"<color={color}>");
        }

        /// <summary>
        /// 添加结束颜色标签，确保颜色作用范围正确结束。
        /// </summary>
        private void AppendEndColorTag() {
            _builder.Append("</color>");
        }

        /// <summary>
        /// 添加指定的文本到构建的消息中。
        /// </summary>
        /// <param name="text">要添加的文本内容。</param>
        /// <returns>当前实例以便继续链式调用。</returns>
        public InfoBuilder Append(string text) {
            _builder.Append(text);
            return this;
        }

        /// <summary>
        /// 添加带有前后空格的文本，用于格式化输出。
        /// </summary>
        /// <param name="text">要添加的文本内容。</param>
        /// <returns>当前实例以便继续链式调用。</returns>
        public InfoBuilder AppendSpaceText(string text) {
            string spaceText = " " + text + " ";
            _builder.Append(spaceText);
            return this;
        }

        /// <summary>
        /// 添加一行文本，并可选地在末尾加上换行符。
        /// </summary>
        /// <param name="text">要添加的文本内容，默认为空字符串。</param>
        /// <returns>当前实例以便继续链式调用。</returns>
        public InfoBuilder AppendLine(string text = "") {
            _builder.AppendLine(text);
            return this;
        }

        /// <summary>
        /// 构建最终的字符串消息，包括关闭之前打开的颜色标签。
        /// </summary>
        /// <returns>完成构建的字符串。</returns>
        public string Build() {
            AppendEndColorTag();
            return _builder.ToString();
        }

        /// <summary>
        /// 将构建好的消息根据日志等级输出到Unity控制台。
        /// </summary>
        public void Log() {
            string message = Build();
            switch (_level) {
                case LogLevel.Info:
                    Debug.Log(message);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogLevel.Error:
                    Debug.LogError(message);
                    break;
            }
        }

        /// <summary>
        /// 静态方法，创建一个新的 <see cref="InfoBuilder"/> 实例。
        /// </summary>
        /// <param name="level">日志等级，默认为 Info。</param>
        /// <returns>新的 InfoBuilder 实例。</returns>
        public static InfoBuilder Create(LogLevel level = LogLevel.Info) {
            return new InfoBuilder(level);
        }
    }
}
