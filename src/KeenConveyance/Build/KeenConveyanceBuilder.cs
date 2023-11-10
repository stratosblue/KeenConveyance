using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

internal class KeenConveyanceBuilder : IKeenConveyanceBuilder
{
    #region Public 属性

    public IServiceCollection Services { get; }

    #endregion Public 属性

    #region Public 构造函数

    public KeenConveyanceBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    #endregion Public 构造函数
}
