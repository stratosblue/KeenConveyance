#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using System.Buffers;

namespace KeenConveyance.Serialization;

/// <summary>
/// <see cref="IObjectSerializer"/> 基类
/// </summary>
public abstract class ObjectSerializer(string supportedMediaType)
    : IObjectSerializer
{
    #region Public 属性

    /// <inheritdoc/>
    public SpecificMediaType SupportedMediaType { get; } = supportedMediaType;

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public abstract IMultipleObjectAsyncStreamSerializer CreateObjectStreamSerializer(Stream stream);

    /// <inheritdoc/>
    public abstract IMultipleObjectStreamSerializer CreateObjectStreamSerializer(IBufferWriter<byte> bufferWriter);

    /// <inheritdoc/>
    public virtual async ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken)
    {
        return (T?)await DeserializeAsync(typeof(T), stream, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public abstract ValueTask<object?> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public abstract Task<IMultipleObjectCollection> DeserializeMultipleAsync(IReadOnlyList<Type> types, Stream stream, CancellationToken cancellationToken);

    /// <inheritdoc/>
    public virtual void Serialize<T>(T? value, IBufferWriter<byte> bufferWriter)
    {
        Serialize(value, typeof(T), bufferWriter);
    }

    /// <inheritdoc/>
    public abstract void Serialize(object? value, Type type, IBufferWriter<byte> bufferWriter);

    /// <inheritdoc/>
    public virtual Task SerializeAsync<T>(T? value, Stream stream, CancellationToken cancellationToken)
    {
        return SerializeAsync(value, typeof(T), stream, cancellationToken);
    }

    /// <inheritdoc/>
    public abstract Task SerializeAsync(object? value, Type type, Stream stream, CancellationToken cancellationToken);

    #endregion Public 方法
}
