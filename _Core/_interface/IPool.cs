namespace _Core {
    /// <summary>
    /// 池化对象的公共接口，提供获取/归还回调及调试启用方法。
    /// </summary>
    public interface IPool : IPoolResettable {
        void OnGetFromPool();
        void OnReturnToPool();

        void EnableDebug(bool enable);
    }
}
