namespace KeenConveyance.Serialization;

/// <summary>
/// 多对象流式序列化器
/// </summary>
public interface IMultipleObjectStreamSerializer : IDisposable
{
    #region Public 方法

    /// <summary>
    /// 完成写入
    /// </summary>
    void Finish();

    /// <summary>
    /// 开始写入
    /// </summary>
    void Start();

    /// <summary>
    /// 写入 <paramref name="value"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    void Write<T>(T? value);

    /// <summary>
    /// 写入 <paramref name="value"/>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    void Write(object? value, Type type);

    #endregion Public 方法
}
