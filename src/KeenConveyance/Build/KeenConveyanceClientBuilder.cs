#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using KeenConveyance.Client;
using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

internal class KeenConveyanceClientBuilder(IServiceCollection services,
                                           KeenConveyanceClientBuilderContext context)
    : IKeenConveyanceClientBuilder
{
    #region Public 属性

    public KeenConveyanceClientBuilderContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));

    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    #endregion Public 属性
}

internal class KeenConveyanceClientBuilderGroupBuilder(IServiceCollection services,
                                                       KeenConveyanceClientBuilderContext context,
                                                       Action<KeenConveyanceClientOptions>? groupOptionsSetupAction,
                                                       Action<IHttpClientBuilder>? groupHttpClientSetupAction)
    : IKeenConveyanceClientBuilderGroupBuilder
{
    #region Public 属性

    public KeenConveyanceClientBuilderContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));

    public KeenConveyanceClientBuilderContext GroupContext { get; } = new();

    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    #region Group Setup

    public Action<IHttpClientBuilder>? GroupHttpClientSetupAction { get; } = groupHttpClientSetupAction;

    public Action<KeenConveyanceClientOptions>? GroupOptionsSetupAction { get; } = groupOptionsSetupAction;

    #endregion Group Setup

    #endregion Public 属性

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
