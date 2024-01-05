using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using KeenConveyance.Serialization;

namespace KeenConveyance.TestWebAPI.MemoryPackSupportedTest;

public class MemoryPackObjectSerializer : ObjectSerializer
{
    #region Public 构造函数

    public MemoryPackObjectSerializer() : base("application/memorypack")
    {
    }

    #endregion Public 构造函数

    #region Public 方法

    public override IMultipleObjectStreamSerializer CreateObjectStreamSerializer(Stream stream)
    {
        return new MemoryPackMultipleObjectStreamSerializer(stream);
    }

    public override ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken) where T : default
    {
        return MemoryPack.MemoryPackSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
    }

    public override ValueTask<object?> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken)
    {
        return MemoryPack.MemoryPackSerializer.DeserializeAsync(type, stream);
    }

    public override async Task<IMultipleObjectCollection> DeserializeMultipleAsync(IReadOnlyList<Type> types, Stream stream, CancellationToken cancellationToken)
    {
        var reader = PipeReader.Create(stream);
        var rawItemLengthBuffer = new byte[sizeof(int)];

        var result = DefaultMultipleObjectCollection.Create(types.Count, out var itemDirectSetter);

        var index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var readResult = await reader.ReadAtLeastAsync(sizeof(int), cancellationToken).ConfigureAwait(false);
            if (readResult.Buffer.IsEmpty
                && readResult.IsCompleted)
            {
                break;
            }

            readResult.Buffer.Slice(0, sizeof(int))
                             .CopyTo(rawItemLengthBuffer);
            reader.AdvanceTo(readResult.Buffer.GetPosition(sizeof(int)));

            var length = BinaryPrimitives.ReadInt32LittleEndian(rawItemLengthBuffer);

            readResult = await reader.ReadAtLeastAsync(length, cancellationToken).ConfigureAwait(false);

            itemDirectSetter[index] = MemoryPack.MemoryPackSerializer.Deserialize(types[index], readResult.Buffer.Slice(0, length));

            reader.AdvanceTo(readResult.Buffer.GetPosition(length));

            if (readResult.IsCompleted)
            {
                break;
            }

            index++;
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (index != types.Count)
        {
            result.Dispose();
            throw new InvalidOperationException("The number of input values not match the parameter count");
        }
        return result;
    }

    public override void Serialize(object? value, Type type, IBufferWriter<byte> bufferWriter)
    {
        MemoryPack.MemoryPackSerializer.Serialize(type, bufferWriter, value);
    }

    public override async Task SerializeAsync(object? value, Type type, Stream stream, CancellationToken cancellationToken)
    {
        await MemoryPack.MemoryPackSerializer.SerializeAsync(type, stream, value);
    }

    #endregion Public 方法
}
