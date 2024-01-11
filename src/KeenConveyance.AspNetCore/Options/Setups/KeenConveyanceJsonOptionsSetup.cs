using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace KeenConveyance;

internal sealed class KeenConveyanceJsonOptionsSetup(IOptions<JsonOptions> optionsAccessor) : IConfigureNamedOptions<KeenConveyanceJsonOptions>
{
    //自动同步 KeenConveyanceJsonOptions 为服务端配置的 JsonOptions

    #region Public 方法

    public void Configure(KeenConveyanceJsonOptions options)
    {
        options.JsonSerializerOptions = optionsAccessor.Value.JsonSerializerOptions;
    }

    public void Configure(string? name, KeenConveyanceJsonOptions options)
    {
        options.JsonSerializerOptions = optionsAccessor.Value.JsonSerializerOptions;
    }

    #endregion Public 方法
}
