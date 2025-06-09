using System;

namespace _Utils {
    /// <summary>
    /// 不显示在数据库中
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NotShowInDatabaseAttribute : Attribute { }
}
