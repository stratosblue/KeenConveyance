using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace KeenConveyance.AspNetCore;

/// <summary>
/// 默认的 <inheritdoc cref="IMvcEndpointMatcher"/>
/// </summary>
public class DefaultMvcEndpointMatcher : IMvcEndpointMatcher, IDisposable
{
    #region Private 字段

    private readonly IDisposable _endpointDataSourceChangeTokenDisposer;

    #endregion Private 字段

    #region Protected 字段

    /// <inheritdoc cref="IEndpointEntryKeyGenerator"/>
    protected readonly IEndpointEntryKeyGenerator EndpointEntryKeyGenerator;

    /// <inheritdoc cref="IHttpRequestEntryKeyGenerator"/>
    protected readonly IHttpRequestEntryKeyGenerator HttpContextEntryKeyGenerator;

    /// <inheritdoc cref="ILogger"/>
    protected readonly ILogger Logger;

    /// <summary>
    /// <see cref="Endpoint"/> 项目字典
    /// </summary>
    protected ConcurrentDictionary<string, Endpoint> EndpointEntryMap = new();

    /// <summary>
    /// <see cref="Endpoint"/> 加载任务
    /// </summary>
    protected Task? EndpointLoadTask;

    #endregion Protected 字段

    #region Public 构造函数

    /// <inheritdoc cref="DefaultMvcEndpointMatcher"/>
    public DefaultMvcEndpointMatcher(IHttpRequestEntryKeyGenerator httpContextEntryKeyGenerator,
                                     IEndpointEntryKeyGenerator endpointEntryKeyGenerator,
                                     EndpointDataSource endpointDataSource,
                                     ILogger<DefaultMvcEndpointMatcher> logger)
    {
        HttpContextEntryKeyGenerator = httpContextEntryKeyGenerator ?? throw new ArgumentNullException(nameof(httpContextEntryKeyGenerator));
        EndpointEntryKeyGenerator = endpointEntryKeyGenerator ?? throw new ArgumentNullException(nameof(endpointEntryKeyGenerator));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        EndpointLoadTask = LoadEndpointAsync(endpointDataSource);

        _endpointDataSourceChangeTokenDisposer = endpointDataSource.GetChangeToken()
                                                                   .RegisterChangeCallback(state =>
                                                                   {
                                                                       LoadEndpointAsync((EndpointDataSource)state!);
                                                                   }, endpointDataSource);
    }

    #endregion Public 构造函数

    #region Protected 方法

    /// <summary>
    /// 从 <paramref name="endpointDataSource"/> 加载 <see cref="Endpoint"/>
    /// </summary>
    /// <param name="endpointDataSource"></param>
    /// <returns></returns>
    protected virtual Task LoadEndpointAsync(EndpointDataSource endpointDataSource)
    {
        Logger.LogDebug("Start load endpoint. {EndpointDataSource}.", endpointDataSource);

        var endpointEntryMap = new ConcurrentDictionary<string, Endpoint>();

        foreach (var endpoint in endpointDataSource.Endpoints)
        {
            var entryKey = EndpointEntryKeyGenerator.GenerateKey(endpoint);
            if (entryKey is null)
            {
                continue;
            }

            Logger.LogDebug("Load endpoint {Endpoint} with entry key {EntryKey}.", endpointDataSource, entryKey);

            if (!endpointEntryMap.TryAdd(entryKey, endpoint))
            {
                endpointEntryMap.TryGetValue(entryKey, out var existedEndpoint);
                Logger.LogCritical("Endpoint entry key conflict. {ExistedEndpoint} and {Endpoint} has the same key {EntryKey}.", existedEndpoint, endpoint, entryKey);
            }
        }

        EndpointEntryMap = endpointEntryMap;

        Logger.LogInformation("Load endpoint complete. Total count: {EndpointCount}.", endpointEntryMap.Count);

        return Task.CompletedTask;
    }

    #endregion Protected 方法

    #region Public 方法

    /// <inheritdoc/>
    public virtual ValueTask<Endpoint?> MatchEndpointAsync(HttpContext httpContext)
    {
        return EndpointLoadTask is null
               ? InnerMatchEndpointAsync(httpContext)
               : InnerMatchEndpointWithAwaitLoadEndpointAsync(httpContext);

        ValueTask<Endpoint?> InnerMatchEndpointAsync(HttpContext httpContext)
        {
            var entryKeyTask = HttpContextEntryKeyGenerator.GenerateKeyAsync(httpContext);
            return entryKeyTask.IsCompleted
                   ? ValueTask.FromResult(TryGetEndpointDirectly(entryKeyTask.Result))
                   : TryGetEndpointWithAwaitAsync(entryKeyTask);
        }

        async ValueTask<Endpoint?> InnerMatchEndpointWithAwaitLoadEndpointAsync(HttpContext httpContext)
        {
            if (EndpointLoadTask is Task endpointLoadTask)
            {
                await endpointLoadTask.ConfigureAwait(false);
                EndpointLoadTask = null;
            }
            return await InnerMatchEndpointAsync(httpContext).ConfigureAwait(false);
        }

        async ValueTask<Endpoint?> TryGetEndpointWithAwaitAsync(ValueTask<string?> entryKeyTask)
        {
            var entryKey = await entryKeyTask.ConfigureAwait(false);
            return TryGetEndpointDirectly(entryKey);
        }

        Endpoint? TryGetEndpointDirectly(string? entryKey)
        {
            if (entryKey is null)
            {
                return default;
            }
            if (!EndpointEntryMap.TryGetValue(entryKey, out var endpoint))
            {
                Logger.LogWarning("No matched endpoint for the entry key {EntryKey}", entryKey);
            }
            return endpoint;
        }
    }

    #endregion Public 方法

    #region Dispose

    private bool _isDisposed;

    /// <summary>
    ///
    /// </summary>
    ~DefaultMvcEndpointMatcher()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc cref="Dispose()"/>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            _endpointDataSourceChangeTokenDisposer.Dispose();
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
