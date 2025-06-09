namespace _Core {
    /// <summary>
    /// 可重置对象接口，用于对象池中对象的状态重置。
    /// </summary>
    public interface IPoolResettable {
        void ResetState();
    }
}
