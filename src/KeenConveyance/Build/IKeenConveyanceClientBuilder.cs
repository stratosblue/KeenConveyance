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
}

/// <summary>
/// KeenConveyance 客户端构造器组构造器
/// </summary>
public interface IKeenConveyanceClientBuilderGroupBuilder
{
    #region Public 属性

    /// <summary>
    /// 客户端构造上下文
    /// </summary>
    public KeenConveyanceClientBuilderContext Context { get; }

    /// <summary>
    /// 客户端构造上下文当前构造的组
    /// </summary>
    public KeenConveyanceClientBuilderContext GroupContext { get; }

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

    /// <summary>
    /// 完成客户端组的配置
    /// </summary>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void CompleteClientGroupSetup();

    #endregion Public 方法
}
