using KeenConveyance.Client;
using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

internal class KeenConveyanceClientBuilder : IKeenConveyanceClientBuilder
{
    #region Public 属性

    public KeenConveyanceClientBuilderContext Context { get; }

    public IServiceCollection Services { get; }

    #endregion Public 属性

    #region Public 构造函数

    public KeenConveyanceClientBuilder(IServiceCollection services, KeenConveyanceClientBuilderContext context)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #endregion Public 构造函数
}

internal class KeenConveyanceClientBuilderGroupBuilder : IKeenConveyanceClientBuilderGroupBuilder
{
    #region Public 属性

    public KeenConveyanceClientBuilderContext Context { get; }

    public KeenConveyanceClientBuilderContext GroupContext { get; }

    public IServiceCollection Services { get; }

    #region Group Setup

    public Action<IHttpClientBuilder>? GroupHttpClientSetupAction { get; }

    public Action<KeenConveyanceClientOptions>? GroupOptionsSetupAction { get; }

    #endregion Group Setup

    #endregion Public 属性

    #region Public 构造函数

    public KeenConveyanceClientBuilderGroupBuilder(IServiceCollection services,
                                                   KeenConveyanceClientBuilderContext context,
                                                   Action<KeenConveyanceClientOptions>? groupOptionsSetupAction,
                                                   Action<IHttpClientBuilder>? groupHttpClientSetupAction)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
        Context = context ?? throw new ArgumentNullException(nameof(context));
        GroupContext = new();
        GroupOptionsSetupAction = groupOptionsSetupAction;
        GroupHttpClientSetupAction = groupHttpClientSetupAction;
    }

    #endregion Public 构造函数

    #region Public 方法

    public bool ApplyClientImplementation<TClient, TImplementation>()
        where TClient : class
        where TImplementation : class, TClient
    {
        if (Context.ClientHttpClientBuilders.TryGetValue(typeof(TClient), out var builder))
        {
            builder.AddTypedClient<TClient, TImplementation>();
            return true;
        }

        return false;
    }

    public void CompleteClientGroupSetup()
    {
    }

    #endregion Public 方法
}
