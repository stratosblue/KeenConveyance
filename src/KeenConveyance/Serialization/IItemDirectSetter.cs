namespace KeenConveyance.Serialization;

/// <summary>
/// 元素直接设置器
/// </summary>
public interface IItemDirectSetter : IDisposable
{
    #region Public 索引器

    /// <summary>
    /// 设置 <paramref name="index"/> 的元素
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    object? this[int index] { set; }

    #endregion Public 索引器
}
