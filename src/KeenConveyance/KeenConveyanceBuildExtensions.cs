using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class KeenConveyanceBuildExtensions
{
    #region Public 方法

    /// <summary>
    /// 添加客户端
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="builder"></param>
    /// <param name="httpClientSetupAction">客户端配置委托</param>
    /// <returns></returns>
    public static IKeenConveyanceClientBuilder<TClient> AddClient<TClient>(this IKeenConveyanceClientBuilder builder,
                                                                           Action<IKeenConveyanceHttpClientBuilder<TClient>>? httpClientSetupAction = null)
        where TClient : class
    {
        var httpClientBuilder = new KeenConveyanceHttpClientBuilder<TClient>(builder.Services.AddHttpClient<TClient>());
        builder.Context.ClientHttpClientBuilders.Add(typeof(TClient), httpClientBuilder);

        httpClientSetupAction?.Invoke(httpClientBuilder);

        return new KeenConveyanceClientBuilder<TClient>(builder.Services, builder.Context);
    }

    /// <summary>
    /// 添加 KeenConveyance
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IKeenConveyanceBuilder AddKeenConveyance(this IServiceCollection services)
    {
        services.AddLogging();
        return new KeenConveyanceBuilder(services);
    }

    /// <summary>
    /// 配置客户端
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="setupAction"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IKeenConveyanceBuilder ConfigureClient(this IKeenConveyanceBuilder builder, Action<IKeenConveyanceClientBuilder> setupAction)
    {
        if (setupAction is null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        setupAction(new KeenConveyanceClientBuilder(builder.Services, new KeenConveyanceClientBuilderContext()));

        return builder;
    }

    #endregion Public 方法

    #region ServiceAddressProvider

    /// <summary>
    /// 配置服务地址
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="builder"></param>
    /// <param name="serviceAddress"></param>
    /// <returns></returns>
    public static IKeenConveyanceHttpClientBuilder<TClient> ConfigureServiceAddress<TClient>(this IKeenConveyanceHttpClientBuilder<TClient> builder, string serviceAddress)
        where TClient : class
    {
        return ConfigureServiceAddress(builder, new Uri(serviceAddress));
    }

    /// <summary>
    /// 配置服务地址
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="builder"></param>
    /// <param name="serviceAddress"></param>
    /// <returns></returns>
    public static IKeenConveyanceHttpClientBuilder<TClient> ConfigureServiceAddress<TClient>(this IKeenConveyanceHttpClientBuilder<TClient> builder, Uri serviceAddress)
        where TClient : class
    {
        return ConfigureServiceAddress(builder, new KeenConveyanceFixedServiceAddressProvider(serviceAddress));
    }

    /// <summary>
    /// 配置服务地址
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="builder"></param>
    /// <param name="serviceAddressProvider"></param>
    /// <returns></returns>
    public static IKeenConveyanceHttpClientBuilder<TClient> ConfigureServiceAddress<TClient>(this IKeenConveyanceHttpClientBuilder<TClient> builder, IServiceAddressProvider serviceAddressProvider)
        where TClient : class
    {
        builder.Services.AddOptions<KeenConveyanceClientOptions>(CachedTypeNameAccessor<TClient>.DisplayName)
                        .Configure(options =>
                        {
                            options.ServiceAddressProvider = serviceAddressProvider;
                        });

        return builder;
    }

    /// <summary>
    /// 配置服务地址
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="builder"></param>
    /// <param name="serviceAddressProviderType"></param>
    /// <returns></returns>
    public static IKeenConveyanceHttpClientBuilder<TClient> ConfigureServiceAddress<TClient>(this IKeenConveyanceHttpClientBuilder<TClient> builder, Type serviceAddressProviderType)
        where TClient : class
    {
        builder.Services.AddOptions<KeenConveyanceClientOptions>(CachedTypeNameAccessor<TClient>.DisplayName)
                        .Configure<IServiceProvider>((options, serviceProvider) =>
                        {
                            options.ServiceAddressProvider = (IServiceAddressProvider)serviceProvider.GetRequiredService(serviceAddressProviderType);
                        });

        return builder;
    }

    #endregion ServiceAddressProvider

    #region HttpRequestMessageConstructor

    /// <summary>
    /// 配置 <typeparamref name="TClient"/> 的 <see cref="IHttpRequestMessageConstructor"/>
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="builder"></param>
    /// <param name="httpRequestMessageConstructor"></param>
    /// <returns></returns>
    public static IKeenConveyanceHttpClientBuilder<TClient> ConfigureHttpRequestMessageConstructor<TClient>(this IKeenConveyanceHttpClientBuilder<TClient> builder, IHttpRequestMessageConstructor httpRequestMessageConstructor)
        where TClient : class
    {
        builder.Services.AddOptions<KeenConveyanceClientOptions>(CachedTypeNameAccessor<TClient>.DisplayName)
                        .Configure(options =>
                        {
                            options.HttpRequestMessageConstructor = httpRequestMessageConstructor;
                        });

        return builder;
    }

    /// <summary>
    /// 配置 <typeparamref name="TClient"/> 的 <see cref="IHttpRequestMessageConstructor"/>
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="builder"></param>
    /// <param name="httpRequestMessageConstructorType"></param>
    /// <returns></returns>
    public static IKeenConveyanceHttpClientBuilder<TClient> ConfigureHttpRequestMessageConstructor<TClient>(this IKeenConveyanceHttpClientBuilder<TClient> builder, Type httpRequestMessageConstructorType)
        where TClient : class
    {
        builder.Services.AddOptions<KeenConveyanceClientOptions>(CachedTypeNameAccessor<TClient>.DisplayName)
                        .Configure<IServiceProvider>((options, serviceProvider) =>
                        {
                            options.HttpRequestMessageConstructor = (IHttpRequestMessageConstructor)serviceProvider.GetRequiredService(httpRequestMessageConstructorType);
                        });

        return builder;
    }

    /// <summary>
    /// 使用基于 Path 的 <see cref="IHttpRequestMessageConstructor"/>
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="builder"></param>
    /// <param name="serviceEntryPath"></param>
    /// <returns></returns>
    public static IKeenConveyanceHttpClientBuilder<TClient> ConfigurePathBaseHttpRequestMessageConstructor<TClient>(this IKeenConveyanceHttpClientBuilder<TClient> builder, string serviceEntryPath = KeenConveyanceConstants.DefaultEntryPath)
                where TClient : class
    {
        return ConfigureHttpRequestMessageConstructor(builder, new PathBaseHttpRequestMessageConstructor(serviceEntryPath));
    }

    /// <summary>
    /// 使用基于 Query 的 <see cref="IHttpRequestMessageConstructor"/>
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="builder"></param>
    /// <param name="serviceEntryPath"></param>
    /// <param name="queryKey"></param>
    /// <returns></returns>
    public static IKeenConveyanceHttpClientBuilder<TClient> ConfigureQueryBaseHttpRequestMessageConstructor<TClient>(this IKeenConveyanceHttpClientBuilder<TClient> builder, string serviceEntryPath = KeenConveyanceConstants.DefaultEntryPath, string queryKey = QueryBaseEntryKeyOptions.DefaultQueryKey)
        where TClient : class
    {
        return ConfigureHttpRequestMessageConstructor(builder, new QueryBaseHttpRequestMessageConstructor(serviceEntryPath, queryKey));
    }

    #endregion HttpRequestMessageConstructor
}
