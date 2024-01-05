using System.Buffers.Binary;
using KeenConveyance.Serialization;

namespace KeenConveyance.TestWebAPI.MemoryPackSupportedTest;

public sealed class MemoryPackMultipleObjectStreamSerializer : IMultipleObjectStreamSerializer
{
    // 仅做可行性验证，可优化内存使用

    #region Private 字段

    private readonly Stream _stream;

    #endregion Private 字段

    #region Public 构造函数

    public MemoryPackMultipleObjectStreamSerializer(Stream stream, int bufferSize = 4096)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Dispose()
    {
    }

    public async ValueTask FinishAsync(CancellationToken cancellationToken)
    {
        await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public ValueTask StartAsync(CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask WriteAsync<T>(T? value, CancellationToken cancellationToken)
    {
        var data = MemoryPack.MemoryPackSerializer.Serialize(value);
        var lengthData = new byte[sizeof(int)];

        BinaryPrimitives.WriteInt32LittleEndian(lengthData, data.Length);

        await _stream.WriteAsync(lengthData, cancellationToken).ConfigureAwait(false);
        await _stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask WriteAsync(object? value, Type type, CancellationToken cancellationToken)
    {
        var data = MemoryPack.MemoryPackSerializer.Serialize(value);
        var lengthData = new byte[sizeof(int)];

        BinaryPrimitives.WriteInt32LittleEndian(lengthData, data.Length);

        await _stream.WriteAsync(lengthData, cancellationToken).ConfigureAwait(false);
        await _stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
    }

    #endregion Public 方法
}
