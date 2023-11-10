using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

/// <summary>
/// KeenConveyance 客户端构造器
/// </summary>
public interface IKeenConveyanceClientBuilder
{
    #region Public 属性

    /// <summary>
    /// 客户端构造上下文
    /// </summary>
    public KeenConveyanceClientBuilderContext Context { get; }

    /// <inheritdoc cref="IServiceCollection"/>
    public IServiceCollection Services { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 应用客户端实现类
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TImplementation"></typeparam>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool ApplyClientImplementation<TClient, TImplementation>()
        where TClient : class
        where TImplementation : class, TClient;

    #endregion Public 方法
}

/// <summary>
/// <typeparamref name="TClient"/> 的 KeenConveyance 客户端构造器
/// </summary>
public interface IKeenConveyanceClientBuilder<TClient> : IKeenConveyanceClientBuilder
{
}
