using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace KeenConveyance;

/// <summary>
/// KeenConveyance 客户端选项
/// </summary>
public class KeenConveyanceClientOptions
{
    #region SystemTextJson

    private JsonSerializerOptions? _serializerOptions = null;

    /// <summary>
    /// see https://github.com/dotnet/aspnetcore/blob/release/8.0/src/Http/Http.Extensions/src/JsonOptions.cs
    /// </summary>
    internal static readonly JsonSerializerOptions DefaultSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        // Web defaults don't use the relaxed JSON escaping encoder.
        //
        // Because these options are for producing content that is written directly to the request
        // (and not embedded in an HTML page for example), we can use UnsafeRelaxedJsonEscaping.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

        // The JsonSerializerOptions.GetTypeInfo method is called directly and needs a defined resolver
        // setting the default resolver (reflection-based) but the user can overwrite it directly or by modifying
        // the TypeInfoResolverChain. Use JsonTypeInfoResolver.Combine() to produce an empty TypeInfoResolver.
#if NET8_0_OR_GREATER
        TypeInfoResolver = JsonSerializer.IsReflectionEnabledByDefault ? new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver() : System.Text.Json.Serialization.Metadata.JsonTypeInfoResolver.Combine()
#endif
    };

    /// <summary>
    /// Json序列化选项
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get => _serializerOptions ??= GetDefaultSerializerOptions(); set => _serializerOptions = value; }

    /// <summary>
    /// 获取默认的 Json序列化选项
    /// </summary>
    /// <returns></returns>
    public static JsonSerializerOptions GetDefaultSerializerOptions() => new(DefaultSerializerOptions);

    #endregion SystemTextJson

    #region Public 属性

    /// <inheritdoc cref="IHttpRequestMessageConstructor"/>
    [Required]
    public IHttpRequestMessageConstructor? HttpRequestMessageConstructor { get; set; } = new QueryBaseHttpRequestMessageConstructor(KeenConveyanceConstants.DefaultEntryPath, QueryBaseEntryKeyOptions.DefaultQueryKey);

    /// <inheritdoc cref="IServiceAddressProvider"/>
    [Required]
    public IServiceAddressProvider? ServiceAddressProvider { get; set; }

    #endregion Public 属性
}
