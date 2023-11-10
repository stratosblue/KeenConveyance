using System.ComponentModel;
using KeenConveyance.AspNetCore.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KeenConveyance;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class KeenConveyanceClientEnhanceBuildExtensions
{
    #region Public 方法

    /// <summary>
    /// 转发Http请求的所有Header
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IKeenConveyanceHttpClientBuilder<TClient> ForwardedRequestHeaders<TClient>(this IKeenConveyanceHttpClientBuilder<TClient> builder)
        where TClient : class
    {
        builder.Services.TryAddTransient<HttpContextHeaderForwardedHttpMessageHandler>();
        builder.ConfigureHttpMessageHandlerBuilder(handlerBuilder => handlerBuilder.AdditionalHandlers.Add(handlerBuilder.Services.GetRequiredService<HttpContextHeaderForwardedHttpMessageHandler>()));
        return builder;
    }

    /// <summary>
    /// 转发Http请求的Header
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="builder"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public static IKeenConveyanceHttpClientBuilder<TClient> ForwardedRequestHeaders<TClient>(this IKeenConveyanceHttpClientBuilder<TClient> builder, params string[] headers)
        where TClient : class
    {
        if (headers is null
            || headers.Length == 0)
        {
            return ForwardedRequestHeaders(builder);
        }

        builder.Services.TryAddTransient<FixedHttpContextHeaderForwardedHttpMessageHandler>();
        builder.ConfigureHttpMessageHandlerBuilder(handlerBuilder => handlerBuilder.AdditionalHandlers.Add(handlerBuilder.Services.GetRequiredService<FixedHttpContextHeaderForwardedHttpMessageHandler>().SetHeaders(headers)));
        return builder;
    }

    #endregion Public 方法
}
