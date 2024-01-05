using System.ComponentModel.DataAnnotations;
using KeenConveyance.Serialization;

namespace KeenConveyance.Client;

/// <summary>
/// KeenConveyance 客户端选项
/// </summary>
public class KeenConveyanceClientOptions
{
    #region Public 属性

    /// <inheritdoc cref="IHttpRequestMessageConstructor"/>
    [Required]
    public IHttpRequestMessageConstructor? HttpRequestMessageConstructor { get; set; } = new QueryBaseHttpRequestMessageConstructor(KeenConveyanceConstants.DefaultEntryPath, QueryBaseEntryKeyOptions.DefaultQueryKey);

    /// <summary>
    /// <inheritdoc cref="IObjectSerializer"/> <br/>
    /// 设置此选项以替代默认的 json 传输
    /// </summary>
    public IObjectSerializer? ObjectSerializer { get; set; }

    /// <inheritdoc cref="IServiceAddressProvider"/>
    [Required]
    public IServiceAddressProvider? ServiceAddressProvider { get; set; }

    #endregion Public 属性
}
