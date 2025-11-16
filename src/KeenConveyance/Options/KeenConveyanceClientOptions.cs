#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using System.ComponentModel.DataAnnotations;
using KeenConveyance.Serialization;

namespace KeenConveyance.Client;

/// <summary>
/// KeenConveyance 客户端选项
/// </summary>
public class KeenConveyanceClientOptions
{
    #region Public 字段

    /// <summary>
    /// <see cref="BufferInitialCapacity"/> 的默认值
    /// </summary>
    public const int DefaultBufferInitialCapacity = 1024 * 4;

    /// <summary>
    /// 是否预准备请求数据选项 <see cref="PrePreparePayloadData"/> 的默认值
    /// </summary>
    public const bool DefaultPrePreparePayloadData = true;

    #endregion Public 字段

    #region Public 属性

    /// <summary>
    /// 使用 <see cref="PrePreparePayloadData"/> 时，buffer 的初始化大小<br/>
    /// 默认值为 <see cref="DefaultBufferInitialCapacity"/>
    /// </summary>
    public int? BufferInitialCapacity { get; set; }

    /// <inheritdoc cref="IHttpRequestMessageConstructor"/>
    [Required]
    public IHttpRequestMessageConstructor? HttpRequestMessageConstructor { get; set; } = new QueryBaseHttpRequestMessageConstructor(KeenConveyanceConstants.DefaultEntryPath, QueryBaseEntryKeyOptions.DefaultQueryKey);

    /// <summary>
    /// <inheritdoc cref="IObjectSerializer"/> <br/>
    /// 设置此选项以替代默认的 json 传输
    /// </summary>
    public IObjectSerializer? ObjectSerializer { get; set; }

    /// <summary>
    /// 是否预准备请求数据<br/>
    /// 该值为 <see langword="true"/> 时，数据将在请求前组装完毕，请求时直接发送已组装的数据（此选项需要申请内存存放组装的数据，可能导致内存使用增加）<br/>
    /// 该值为 <see langword="false"/> 时，数据将在请求时流式写入请求流（此选项可能导致 Http 请求无有效的 Content-Length 头）<br/>
    /// 默认值为 <see cref="DefaultPrePreparePayloadData"/>
    /// </summary>
    public bool? PrePreparePayloadData { get; set; }

    /// <inheritdoc cref="IServiceAddressProvider"/>
    [Required]
    public IServiceAddressProvider? ServiceAddressProvider { get; set; }

    #endregion Public 属性
}
