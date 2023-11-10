using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

/// <summary>
/// KeenConveyance 构造器
/// </summary>
public interface IKeenConveyanceBuilder
{
    #region Public 属性

    /// <inheritdoc cref="IServiceCollection"/>
    public IServiceCollection Services { get; }

    #endregion Public 属性
}
