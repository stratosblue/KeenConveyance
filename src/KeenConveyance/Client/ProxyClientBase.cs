﻿using System.Text.Json;
using Microsoft.Extensions.Options;

namespace KeenConveyance;

/// <summary>
/// 代理客户端基础类
/// </summary>
public abstract class ProxyClientBase : IDisposable
{
    #region Protected 字段

    /// <summary>
    /// 客户端名称
    /// </summary>
    protected readonly string ClientName;

    /// <inheritdoc cref="IHttpRequestMessageConstructor"/>
    protected readonly IHttpRequestMessageConstructor HttpRequestMessageConstructor;

    /// <inheritdoc cref="System.Text.Json.JsonSerializerOptions"/>
    protected readonly JsonSerializerOptions JsonSerializerOptions;

    /// <inheritdoc cref="IServiceAddressProvider"/>
    protected readonly IServiceAddressProvider ServiceAddressProvider;

    /// <summary>
    /// 底层使用的 <see cref="HttpClient"/>
    /// </summary>
    protected readonly HttpClient UnderlyingHttpClient;

    #endregion Protected 字段

    #region Public 构造函数

    /// <inheritdoc cref="ProxyClientBase"/>
    public ProxyClientBase(string clientName, HttpClient httpClient, IOptionsSnapshot<KeenConveyanceClientOptions> clientOptionsSnapshot)
    {
        if (string.IsNullOrWhiteSpace(clientName))
        {
            throw new ArgumentException($"“{nameof(clientName)}”不能为 null 或空白。", nameof(clientName));
        }

        if (clientOptionsSnapshot is null)
        {
            throw new ArgumentNullException(nameof(clientOptionsSnapshot));
        }

        ClientName = clientName;

        UnderlyingHttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        var globalClientOptions = clientOptionsSnapshot.Get(Options.DefaultName);

        var clientOptions = clientOptionsSnapshot.Get(clientName);

        JsonSerializerOptions = clientOptions.SerializerOptions
                                ?? globalClientOptions.SerializerOptions;

        ServiceAddressProvider = clientOptions.ServiceAddressProvider
                                 ?? globalClientOptions.ServiceAddressProvider
                                 ?? throw new ArgumentException("must configure the service address provider before use client.");

        HttpRequestMessageConstructor = clientOptions.HttpRequestMessageConstructor
                                        ?? globalClientOptions.HttpRequestMessageConstructor
                                        ?? throw new ArgumentException("must configure the http request message constructor before use client.");
    }

    #endregion Public 构造函数

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

        httpResponseMessage.EnsureSuccessStatusCode();

        return await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
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

        using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<TResult>(responseStream, options: JsonSerializerOptions, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 发送 <paramref name="entryKey"/> 的 <paramref name="httpRequestMessage"/>
    /// </summary>
    /// <param name="entryKey"></param>
    /// <param name="httpRequestMessage"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual Task<HttpResponseMessage> SendHttpRequestMessageAsync(string entryKey, HttpRequestMessage httpRequestMessage, in CancellationToken cancellationToken)
    {
        return UnderlyingHttpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    #endregion Protected 方法

    #region Dispose

    private bool _disposedValue;

    /// <summary>
    ///
    /// </summary>
    ~ProxyClientBase()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc cref="Dispose()"/>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _disposedValue = true;
            UnderlyingHttpClient.Dispose();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion Dispose
}
