using System.Buffers;
using System.Text.Json;

namespace KeenConveyance.Serialization;

/// <summary>
/// 基于 System.Text.Json 的 <see cref="IObjectSerializer"/>
/// </summary>
public sealed class SystemTextJsonObjectSerializer : ObjectSerializer
{
    #region Private 字段

    private readonly JsonDocumentOptions _jsonDocumentOptions;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private readonly JsonWriterOptions _jsonWriterOptions;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="SystemTextJsonObjectSerializer"/>
    public SystemTextJsonObjectSerializer(JsonSerializerOptions jsonSerializerOptions)
        : base(WellknownMediaTypes.Json)
    {
        _jsonSerializerOptions = jsonSerializerOptions;

        _jsonWriterOptions = new()
        {
            Encoder = jsonSerializerOptions.Encoder,
            Indented = jsonSerializerOptions.WriteIndented,
            SkipValidation = true,
        };

        _jsonDocumentOptions = new()
        {
            AllowTrailingCommas = _jsonSerializerOptions.AllowTrailingCommas,
            CommentHandling = JsonCommentHandling.Skip,
            MaxDepth = _jsonSerializerOptions.MaxDepth
        };
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override IMultipleObjectStreamSerializer CreateObjectStreamSerializer(Stream stream)
    {
        var jsonWriter = new Utf8JsonWriter(stream, _jsonWriterOptions);
        return new SystemTextJsonMultipleObjectStreamSerializer(jsonWriter, _jsonSerializerOptions);
    }

    /// <inheritdoc/>
    public override ValueTask<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken) where T : default
    {
        return JsonSerializer.DeserializeAsync<T>(stream, _jsonSerializerOptions, cancellationToken);
    }

    /// <inheritdoc/>
    public override ValueTask<object?> DeserializeAsync(Type type, Stream stream, CancellationToken cancellationToken)
    {
        return JsonSerializer.DeserializeAsync(stream, type, _jsonSerializerOptions, cancellationToken);
    }

    /// <inheritdoc/>

    public override async Task<IMultipleObjectCollection> DeserializeMultipleAsync(IReadOnlyList<Type> types, Stream stream, CancellationToken cancellationToken)
    {
        var jsonDocument = await JsonDocument.ParseAsync(utf8Json: stream,
                                                         options: _jsonDocumentOptions,
                                                         cancellationToken: cancellationToken).ConfigureAwait(false);

        var rootElement = jsonDocument.RootElement;
        if (rootElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("The input json not a array");
        }

        var result = DefaultMultipleObjectCollection.Create(rootElement.GetArrayLength(), out _);
        var index = 0;

        foreach (var type in types)
        {
            result[index] = rootElement[index].Deserialize(type, _jsonSerializerOptions);
            index++;
        }

        return result;
    }

    /// <inheritdoc/>
    public override void Serialize<T>(T? value, IBufferWriter<byte> bufferWriter) where T : default
    {
        using var utf8JsonWriter = new Utf8JsonWriter(bufferWriter, _jsonWriterOptions);
        JsonSerializer.Serialize(utf8JsonWriter, value, _jsonSerializerOptions);
    }

    /// <inheritdoc/>
    public override void Serialize(object? value, Type type, IBufferWriter<byte> bufferWriter)
    {
        using var utf8JsonWriter = new Utf8JsonWriter(bufferWriter, _jsonWriterOptions);
        JsonSerializer.Serialize(utf8JsonWriter, value, type, _jsonSerializerOptions);
    }

    /// <inheritdoc/>
    public override Task SerializeAsync<T>(T? value, Stream stream, CancellationToken cancellationToken) where T : default
    {
        return JsonSerializer.SerializeAsync(stream, value, _jsonSerializerOptions, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task SerializeAsync(object? value, Type type, Stream stream, CancellationToken cancellationToken)
    {
        return JsonSerializer.SerializeAsync(stream, value, type, _jsonSerializerOptions, cancellationToken);
    }

    #endregion Public 方法

    #region Private 类

    private sealed class SystemTextJsonMultipleObjectStreamSerializer : IMultipleObjectStreamSerializer
    {
        #region Private 字段

        private readonly JsonSerializerOptions _jsonSerializerOptions;

        private readonly Utf8JsonWriter _utf8JsonWriter;

        /// <summary>
        /// 状态 0 初始化，1:开始写入，2:写入完成，3:Disposed
        /// </summary>
        private byte _state = 0;

        #endregion Private 字段

        #region Public 构造函数

        public SystemTextJsonMultipleObjectStreamSerializer(Utf8JsonWriter utf8JsonWriter, JsonSerializerOptions jsonSerializerOptions)
        {
            _utf8JsonWriter = utf8JsonWriter ?? throw new ArgumentNullException(nameof(utf8JsonWriter));
            _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        }

        #endregion Public 构造函数

        #region Public 方法

        public async ValueTask FinishAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (_state < 1)
            {
                throw new InvalidOperationException($"Must invoke \"{nameof(StartAsync)}\" first");
            }
            else if (_state > 1)
            {
                throw new InvalidOperationException("The serializer was finished");
            }

            _state = 2;

            _utf8JsonWriter.WriteEndArray();
            await _utf8JsonWriter.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        public ValueTask StartAsync(CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            if (_state != 0)
            {
                throw new InvalidOperationException("The serializer was started");
            }

            _state = 1;

            _utf8JsonWriter.WriteStartArray();
            return ValueTask.CompletedTask;
        }

        public ValueTask WriteAsync<T>(T? value, CancellationToken cancellationToken)
        {
            CheckCanWrite();

            JsonSerializer.Serialize(_utf8JsonWriter, value, _jsonSerializerOptions);
            return ValueTask.CompletedTask;
        }

        public ValueTask WriteAsync(object? value, Type type, CancellationToken cancellationToken)
        {
            CheckCanWrite();

            JsonSerializer.Serialize(_utf8JsonWriter, value, type, _jsonSerializerOptions);
            return ValueTask.CompletedTask;
        }

        #endregion Public 方法

        #region Private 方法

        private void CheckCanWrite()
        {
            if (_state != 1)
            {
                throw new InvalidOperationException("The serializer can not write now");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_state == 3)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        #endregion Private 方法

        #region IDisposable

        ~SystemTextJsonMultipleObjectStreamSerializer()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_state != 3)
            {
                _state = 3;
                _utf8JsonWriter.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        #endregion IDisposable
    }

    #endregion Private 类
}
