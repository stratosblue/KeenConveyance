using System.Buffers;
using System.Buffers.Binary;
using KeenConveyance.Serialization;

namespace KeenConveyance.TestWebAPI.MemoryPackSupportedTest;

public sealed class MemoryPackMultipleObjectStreamSerializer : IMultipleObjectStreamSerializer
{
    // 仅做可行性验证，可优化内存使用

    #region Private 字段

    private readonly IBufferWriter<byte> _bufferWriter;

    #endregion Private 字段

    #region Public 构造函数

    public MemoryPackMultipleObjectStreamSerializer(IBufferWriter<byte> bufferWriter)
    {
        _bufferWriter = bufferWriter ?? throw new ArgumentNullException(nameof(bufferWriter));
    }

    #endregion Public 构造函数

    #region Public 方法

    public void Dispose()
    {
    }

    public void Finish()
    {
    }

    public void Start()
    {
    }

    public void Write<T>(T? value)
    {
        var data = MemoryPack.MemoryPackSerializer.Serialize(value);
        BinaryPrimitives.WriteInt32LittleEndian(_bufferWriter.GetSpan(sizeof(int)), data.Length);
        _bufferWriter.Advance(sizeof(int));
        _bufferWriter.Write(data);
    }

    public void Write(object? value, Type type)
    {
        var data = MemoryPack.MemoryPackSerializer.Serialize(type, value);
        BinaryPrimitives.WriteInt32LittleEndian(_bufferWriter.GetSpan(sizeof(int)), data.Length);
        _bufferWriter.Advance(sizeof(int));
        _bufferWriter.Write(data);
    }

    #endregion Public 方法
}
