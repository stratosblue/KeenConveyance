using System.ComponentModel;
using KeenConveyance.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

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
    public static IKeenConveyanceClientBuilderGroupBuilder AddClient<TClient>(this IKeenConveyanceClientBuilderGroupBuilder builder,
                                                                              Action<IKeenConveyanceHttpClientBuilder<TClient>>? httpClientSetupAction = null)
        where TClient : class
    {
        if (builder is not KeenConveyanceClientBuilderGroupBuilder groupBuilder)
        {
            throw new ArgumentException($"Unsupported builder type -> \"{builder.GetType()}\"");
        }

        var httpClientBuilder = new KeenConveyanceHttpClientBuilder<TClient>(builder.Services.AddHttpClient<TClient>());

        //直接添加，以在重复配置时抛出异常
        groupBuilder.Context.ClientHttpClientBuilders.Add(typeof(TClient), httpClientBuilder);
        groupBuilder.GroupContext.ClientHttpClientBuilders.Add(typeof(TClient), httpClientBuilder);

        //先进行组配置，私有配置将覆盖组配置
        if (groupBuilder.GroupOptionsSetupAction is not null)
        {
            builder.Services.AddOptions<KeenConveyanceClientOptions>(CachedTypeNameAccessor<TClient>.FullDisplayName)
                            .Configure(groupBuilder.GroupOptionsSetupAction);
        }

        groupBuilder.GroupHttpClientSetupAction?.Invoke(httpClientBuilder);

        httpClientSetupAction?.Invoke(httpClientBuilder);

        return groupBuilder;
    }

    /// <summary>
    /// 添加客户端
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TClientBase">客户端基类</typeparam>
    /// <param name="builder"></param>
    /// <param name="httpClientSetupAction">客户端配置委托</param>
    /// <returns></returns>
    public static IKeenConveyanceClientBuilderGroupBuilder AddClient<TClient, TClientBase>(this IKeenConveyanceClientBuilderGroupBuilder builder,
                                                                                           Action<IKeenConveyanceHttpClientBuilder<TClient>>? httpClientSetupAction = null)
        where TClient : class
        where TClientBase : ProxyClientBase
    {
        return builder.AddClient<TClient>(httpClientSetupAction);
    }

    /// <summary>
    /// 添加 KeenConveyance
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IKeenConveyanceBuilder AddKeenConveyance(this IServiceCollection services, Action<KeenConveyanceOptions>? configureOptions = null)
    {
        services.AddLogging();

        services.AddOptions<KeenConveyanceJsonOptions>();

        services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<KeenConveyanceOptions>, KeenConveyanceOptionsSetup>());

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }
        return new KeenConveyanceBuilder(services);
    }

    /// <summary>
    /// 开始进行设置客户端组配置
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="groupOptionsSetupAction">当前组的选项设置委托，将在客户端的私有配置前配置</param>
    /// <param name="groupHttpClientSetupAction">当前组的 <see cref="IHttpClientBuilder"/> 设置委托，将在客户端的私有配置前配置</param>
    /// <returns></returns>
    public static IKeenConveyanceClientBuilderGroupBuilder BeginSetupClients(this IKeenConveyanceClientBuilder builder, Action<KeenConveyanceClientOptions>? groupOptionsSetupAction = null, Action<IHttpClientBuilder>? groupHttpClientSetupAction = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var groupBuilder = new KeenConveyanceClientBuilderGroupBuilder(builder.Services, builder.Context, groupOptionsSetupAction, groupHttpClientSetupAction);
        return groupBuilder;
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
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(setupAction);

        builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<KeenConveyanceClientOptions>, KeenConveyanceClientOptionsSetup>());

        setupAction(new KeenConveyanceClientBuilder(builder.Services, new KeenConveyanceClientBuilderContext()));

        return builder;
    }

    #endregion Public 方法

    /// <summary>
    /// 配置应用于当前客户端的配置
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <param name="builder"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IKeenConveyanceHttpClientBuilder<TClient> ConfigureOptions<TClient>(this IKeenConveyanceHttpClientBuilder<TClient> builder, Action<KeenConveyanceClientOptions> configureOptions)
        where TClient : class
    {
        builder.Services.AddOptions<KeenConveyanceClientOptions>(CachedTypeNameAccessor<TClient>.FullDisplayName)
                        .Configure(configureOptions);

        return builder;
    }

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
        return ConfigureServiceAddress(builder, new FixedServiceAddressProvider(serviceAddress));
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
        return builder.ConfigureOptions(options =>
               {
                   options.ServiceAddressProvider = serviceAddressProvider;
               });
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
        builder.Services.AddOptions<KeenConveyanceClientOptions>(CachedTypeNameAccessor<TClient>.FullDisplayName)
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
        return builder.ConfigureOptions(options =>
               {
                   options.HttpRequestMessageConstructor = httpRequestMessageConstructor;
               });
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
        builder.Services.AddOptions<KeenConveyanceClientOptions>(CachedTypeNameAccessor<TClient>.FullDisplayName)
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

    #region Global Options

    /// <summary>
    /// 配置全局配置
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static IKeenConveyanceClientBuilder GlobalOptions(this IKeenConveyanceClientBuilder builder, Action<KeenConveyanceClientOptions> configureOptions)
    {
        builder.Services.AddOptions<KeenConveyanceClientOptions>(Options.DefaultName)
                        .Configure(configureOptions);

        return builder;
    }

    #endregion Global Options
}
