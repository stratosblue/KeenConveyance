using System.Buffers;

namespace KeenConveyance.Serialization;

/// <summary>
/// 对象序列化器
/// </summary>
public interface IObjectSerializer : ISupportedMediaType
{
    #region Public 方法

    #region Single

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken);

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="type"></param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<object?> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken);

    /// <summary>
    /// 序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="bufferWriter"></param>
    void Serialize<T>(T? value, IBufferWriter<byte> bufferWriter);

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <param name="bufferWriter"></param>
    void Serialize(object? value, Type type, IBufferWriter<byte> bufferWriter);

    /// <summary>
    /// 序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SerializeAsync<T>(T? value, Stream stream, CancellationToken cancellationToken);

    /// <summary>
    /// 序列化
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SerializeAsync(object? value, Type type, Stream stream, CancellationToken cancellationToken);

    #endregion Single

    #region Multiple

    /// <summary>
    /// 创建流式多对象序列化器
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    IMultipleObjectAsyncStreamSerializer CreateObjectStreamSerializer(Stream stream);

    /// <summary>
    /// 创建流式多对象序列化器
    /// </summary>
    /// <param name="bufferWriter"></param>
    /// <returns></returns>
    IMultipleObjectStreamSerializer CreateObjectStreamSerializer(IBufferWriter<byte> bufferWriter);

    /// <summary>
    /// 反序列化多个对象
    /// </summary>
    /// <param name="types"></param>
    /// <param name="stream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IMultipleObjectCollection> DeserializeMultipleAsync(IReadOnlyList<Type> types, Stream stream, CancellationToken cancellationToken);

    #endregion Multiple

    #endregion Public 方法
}
