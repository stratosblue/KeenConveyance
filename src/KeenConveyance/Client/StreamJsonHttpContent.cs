using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace KeenConveyance;

/// <summary>
/// 流式写入的基于 Json 的 <see cref="HttpContent"/>
/// </summary>
public abstract class StreamJsonHttpContent : HttpContent
{
    #region Private 字段

    private static readonly MediaTypeHeaderValue s_contentType = MediaTypeHeaderValue.Parse("application/json");

    private static readonly JsonWriterOptions s_jsonWriterOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Indented = false,
        SkipValidation = true
    };

    #endregion Private 字段

    #region Protected 字段

    /// <summary>
    /// 用于取消写入的 <see cref="System.Threading.CancellationToken"/>
    /// </summary>
    protected readonly CancellationToken CancellationToken;

    /// <summary>
    /// 用于序列化Json的 <see cref="System.Text.Json.JsonSerializerOptions"/>
    /// </summary>
    protected readonly JsonSerializerOptions JsonSerializerOptions;

    #endregion Protected 字段

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="StreamJsonHttpContent"/>
    /// </summary>
    /// <param name="jsonSerializerOptions"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public StreamJsonHttpContent(JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken)
    {
        JsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
        CancellationToken = cancellationToken;

        Headers.ContentType = s_contentType;
    }

    #endregion Public 构造函数

    #region Protected 方法

    /// <inheritdoc/>
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        CancellationToken.ThrowIfCancellationRequested();

        await using var jsonWriter = new Utf8JsonWriter(stream, s_jsonWriterOptions);
        jsonWriter.WriteStartObject();
        await WriteContentAsync(jsonWriter).ConfigureAwait(false);
        jsonWriter.WriteEndObject();
        await jsonWriter.FlushAsync(CancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    protected override bool TryComputeLength(out long length)
    {
        length = -1;
        return false;
    }

    /// <summary>
    /// 将 Json 写入请求
    /// </summary>
    /// <param name="jsonWriter"></param>
    /// <returns></returns>
    protected abstract ValueTask WriteContentAsync(Utf8JsonWriter jsonWriter);

    #endregion Protected 方法
}
