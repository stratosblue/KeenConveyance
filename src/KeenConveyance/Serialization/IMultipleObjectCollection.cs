namespace KeenConveyance.Serialization;

/// <summary>
/// 多对象集合
/// </summary>
public interface IMultipleObjectCollection : IDisposable
{
    #region Public 属性

    /// <summary>
    /// 元素个数
    /// </summary>
    int Count { get; }

    #endregion Public 属性

    #region Public 索引器

    /// <summary>
    /// 获取 <paramref name="index"/> 处的元素
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    object? this[int index] { get; }

    #endregion Public 索引器
}
