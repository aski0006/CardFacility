using System.Text;
using UnityEngine;

namespace _Utils {
    public enum LogLevel {
        Info,
        Warning,
        Error,
    }
    /// <summary>
    /// 
    /// <p>InfoBuilder.Create(LogLevel.Info)</p>
    /// <p>.AppendLine("游戏状态:")</p>
    /// <p>    .Append("玩家生命值：").Append(playerHealth.ToString())</p>
    /// <p>    .AppendLine()</p>
    /// <p>    .Append("当前关卡：").Append(currentLevelName)</p>
    /// <p>    .Log();</p>
    /// 
    /// </summary>
    public class InfoBuilder {
        private readonly StringBuilder _builder = new StringBuilder();
        private LogLevel _level = LogLevel.Info;

        public InfoBuilder(LogLevel level = LogLevel.Info) {
            _level = level;
            // 设置前缀颜色
            AppendColorTag(GetColorForLevel(level));
        }

        private string GetColorForLevel(LogLevel level) {
            switch (level) {
                case LogLevel.Info: return "white";
                case LogLevel.Warning: return "yellow";
                case LogLevel.Error: return "red";
                default: return "white";
            }
        }

        private void AppendColorTag(string color) {
            _builder.Append($"<color={color}>");
        }

        private void AppendEndColorTag() {
            _builder.Append("</color>");
        }

        // 添加文本
        public InfoBuilder Append(string text) {
            _builder.Append(text);
            return this;
        }

        // 添加换行
        public InfoBuilder AppendLine(string text = "") {
            _builder.AppendLine(text);
            return this;
        }

        // 构建最终字符串
        public string Build() {
            AppendEndColorTag();
            return _builder.ToString();
        }

        // 快捷方法：直接输出到 Unity 控制台
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

        // 静态快捷方法
        public static InfoBuilder Create(LogLevel level = LogLevel.Info) {
            return new InfoBuilder(level);
        }
    }
}
