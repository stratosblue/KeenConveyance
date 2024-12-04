using System.Collections;
using System.Runtime.CompilerServices;
using KeenConveyance.Serialization;
using Microsoft.Extensions.Options;

namespace KeenConveyance.Client;

/// <summary>
/// 代理客户端基础类
/// </summary>
public abstract class ProxyClientBase : IDisposable
{
    #region Protected 字段

    /// <inheritdoc cref="KeenConveyanceClientOptions.BufferInitialCapacity"/>
    protected readonly int BufferInitialCapacity;

    /// <summary>
    /// 客户端名称
    /// </summary>
    protected readonly string ClientName;

    /// <inheritdoc cref="IHttpRequestMessageConstructor"/>
    protected readonly IHttpRequestMessageConstructor HttpRequestMessageConstructor;

    /// <summary>
    /// 方法描述集合
    /// </summary>
    protected readonly MethodDescriptorCollection MethodDescriptors;

    /// <inheritdoc cref="IObjectSerializer"/>
    protected readonly IObjectSerializer ObjectSerializer;

    /// <inheritdoc cref="KeenConveyanceClientOptions.PrePreparePayloadData"/>
    protected readonly bool PrePreparePayloadData;

    /// <inheritdoc cref="IServiceAddressProvider"/>
    protected readonly IServiceAddressProvider ServiceAddressProvider;

    /// <inheritdoc cref="IServiceProvider"/>
    protected readonly IServiceProvider ServiceProvider;

    /// <summary>
    /// 底层使用的 <see cref="HttpClient"/>
    /// </summary>
    protected readonly HttpClient UnderlyingHttpClient;

    #endregion Protected 字段

    #region Protected 构造函数

    /// <inheritdoc cref="ProxyClientBase"/>
    protected ProxyClientBase(string clientName,
                              HttpClient httpClient,
                              IOptionsSnapshot<KeenConveyanceClientOptions> clientOptionsSnapshot,
                              MethodDescriptorCollection methodDescriptors,
                              IServiceProvider serviceProvider)
    {
        if (string.IsNullOrWhiteSpace(clientName))
        {
            throw new ArgumentException($"“{nameof(clientName)}”不能为 null 或空白。", nameof(clientName));
        }

        ArgumentNullException.ThrowIfNull(clientOptionsSnapshot);

        ClientName = clientName;

        UnderlyingHttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        MethodDescriptors = methodDescriptors ?? throw new ArgumentNullException(nameof(methodDescriptors));

        //便于客户端基类从DI获取更多服务，在此添加ServiceProvider，也可避免后续构造签名变更导致的运行错误
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        var globalClientOptions = clientOptionsSnapshot.Get(Options.DefaultName);

        var clientOptions = clientOptionsSnapshot.Get(clientName);

        ObjectSerializer = clientOptions.ObjectSerializer
                           ?? globalClientOptions.ObjectSerializer
                           ?? throw new KeenConveyanceException($"No available object serializer provider for client -> \"{clientName}\"");

        ServiceAddressProvider = clientOptions.ServiceAddressProvider
                                 ?? globalClientOptions.ServiceAddressProvider
                                 ?? throw new KeenConveyanceException($"No available service address provider for client -> \"{clientName}\"");

        HttpRequestMessageConstructor = clientOptions.HttpRequestMessageConstructor
                                        ?? globalClientOptions.HttpRequestMessageConstructor
                                        ?? throw new KeenConveyanceException($"No available http request message constructor for client -> \"{clientName}\"");

        PrePreparePayloadData = clientOptions.PrePreparePayloadData
                                ?? globalClientOptions.PrePreparePayloadData
                                ?? KeenConveyanceClientOptions.DefaultPrePreparePayloadData;

        BufferInitialCapacity = clientOptions.BufferInitialCapacity
                                ?? globalClientOptions.BufferInitialCapacity
                                ?? KeenConveyanceClientOptions.DefaultBufferInitialCapacity;

        if (BufferInitialCapacity < 0)
        {
            throw new ArgumentException($"{nameof(BufferInitialCapacity)} must be greater than 0");
        }
    }

    #endregion Protected 构造函数

    #region Protected 方法

    /// <summary>
    /// 执行请求
    /// </summary>
    /// <param name="entryKey"></param>
    /// <param name="httpContent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task ExecuteRequestAsync(string entryKey, HttpContent? httpContent, CancellationToken cancellationToken)
    {
        var serviceAddress = await ServiceAddressProvider.RequireUriAsync(ClientName, cancellationToken).ConfigureAwait(false);
        using var httpRequestMessage = HttpRequestMessageConstructor.CreateHttpRequestMessage(serviceAddress, entryKey, httpContent);

        using var httpResponseMessage = await SendHttpRequestMessageAsync(entryKey, httpRequestMessage, cancellationToken).ConfigureAwait(false);

        httpResponseMessage.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 执行请求并返回结果
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="entryKey"></param>
    /// <param name="httpContent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<TResult?> ExecuteRequestAsync<TResult>(string entryKey, HttpContent? httpContent, CancellationToken cancellationToken)
    {
        var serviceAddress = await ServiceAddressProvider.RequireUriAsync(ClientName, cancellationToken).ConfigureAwait(false);
        using var httpRequestMessage = HttpRequestMessageConstructor.CreateHttpRequestMessage(serviceAddress, entryKey, httpContent);

        using var httpResponseMessage = await SendHttpRequestMessageAsync(entryKey, httpRequestMessage, cancellationToken).ConfigureAwait(false);

        return await ProcessResponseAsync<TResult>(httpResponseMessage, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 执行请求并返回原始字符串
    /// </summary>
    /// <param name="entryKey"></param>
    /// <param name="httpContent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<string> ExecuteRequestWithRawResultAsync(string entryKey, HttpContent? httpContent, CancellationToken cancellationToken)
    {
        var serviceAddress = await ServiceAddressProvider.RequireUriAsync(ClientName, cancellationToken).ConfigureAwait(false);
        using var httpRequestMessage = HttpRequestMessageConstructor.CreateHttpRequestMessage(serviceAddress, entryKey, httpContent);

        using var httpResponseMessage = await SendHttpRequestMessageAsync(entryKey, httpRequestMessage, cancellationToken).ConfigureAwait(false);

        if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return string.Empty;
        }

        //HACK 字符串特殊处理？
        return (await ProcessResponseAsync<string>(httpResponseMessage, cancellationToken).ConfigureAwait(false))!;
    }

    /// <summary>
    /// 处理响应消息 <paramref name="httpResponseMessage"/> 生成返回对象
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="httpResponseMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task<TResult?> ProcessResponseAsync<TResult>(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
    {
        if (httpResponseMessage.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default;
        }

        httpResponseMessage.EnsureSuccessStatusCode();

        var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return await ObjectSerializer.DeserializeAsync<TResult>(responseStream, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 发送 <paramref name="entryKey"/> 的 <paramref name="httpRequestMessage"/>
    /// </summary>
    /// <param name="entryKey"></param>
    /// <param name="httpRequestMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task<HttpResponseMessage> SendHttpRequestMessageAsync(string entryKey, HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        httpRequestMessage.Headers.TryAddWithoutValidation("Accept", ObjectSerializer.SupportedMediaType);
        return UnderlyingHttpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    #endregion Protected 方法

    #region Dispose

    private bool _isDisposed;

    /// <summary>
    ///
    /// </summary>
    ~ProxyClientBase()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="Dispose()"/>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            UnderlyingHttpClient.Dispose();
        }
    }

    #endregion Dispose

    #region Protected 类

    /// <summary>
    /// 方法描述符集合
    /// </summary>
    /// <param name="methodDescriptors"></param>
    protected sealed class MethodDescriptorCollection(IEnumerable<MethodDescriptor> methodDescriptors) : IEnumerable<MethodDescriptor>
    {
        #region Private 字段

        private readonly MethodDescriptor[] _methodDescriptors = methodDescriptors.ToArray();

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// 方法总数
        /// </summary>
        public int Count => _methodDescriptors.Length;

        #endregion Public 属性

        #region Public 索引器

        /// <summary>
        /// 获取指定 <paramref name="index"/> 的 <see cref="MethodDescriptor"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public MethodDescriptor this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _methodDescriptors[index];
        }

        /// <summary>
        /// 获取指定 <paramref name="index"/> 的 <see cref="MethodDescriptor"/> 的指定 <paramref name="parameterIndex"/> 的 <see cref="ParameterDescriptor"/>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="parameterIndex"></param>
        /// <returns></returns>
        public ParameterDescriptor this[int index, int parameterIndex]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _methodDescriptors[index].Parameters[parameterIndex];
        }

        #endregion Public 索引器

        #region Public 方法

        /// <inheritdoc/>
        public IEnumerator<MethodDescriptor> GetEnumerator() => ((IEnumerable<MethodDescriptor>)_methodDescriptors).GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => _methodDescriptors.GetEnumerator();

        #endregion Public 方法
    }

    #endregion Protected 类
}
