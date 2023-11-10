using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

/// <summary>
/// KeenConveyance 客户端构造上下文
/// </summary>
public class KeenConveyanceClientBuilderContext
{
    #region Public 属性

    /// <summary>
    /// 客户端 <see cref="IHttpClientBuilder"/> 字典
    /// </summary>
    public Dictionary<Type, IHttpClientBuilder> ClientHttpClientBuilders { get; } = new();

    #endregion Public 属性
}
