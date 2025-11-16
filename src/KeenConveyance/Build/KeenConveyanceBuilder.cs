#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

internal class KeenConveyanceBuilder(IServiceCollection services)
    : IKeenConveyanceBuilder
{
    #region Public 属性

    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));

    #endregion Public 属性
}
