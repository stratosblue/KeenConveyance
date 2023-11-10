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

    #endregion Public 方法
}

internal class KeenConveyanceClientBuilder<TClient> : KeenConveyanceClientBuilder, IKeenConveyanceClientBuilder<TClient>
{
    #region Public 构造函数

    public KeenConveyanceClientBuilder(IServiceCollection services, KeenConveyanceClientBuilderContext context) : base(services, context)
    {
    }

    #endregion Public 构造函数
}
