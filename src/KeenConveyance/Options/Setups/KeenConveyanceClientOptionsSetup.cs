using KeenConveyance.Client;
using Microsoft.Extensions.Options;

namespace KeenConveyance;

internal sealed class KeenConveyanceClientOptionsSetup(IOptions<KeenConveyanceOptions> optionsAccessor) : IConfigureNamedOptions<KeenConveyanceClientOptions>
{
    #region Public 方法

    public void Configure(KeenConveyanceClientOptions options)
    {
    }

    public void Configure(string? name, KeenConveyanceClientOptions options)
    {
        if (name is null || string.Equals(Options.DefaultName, name))
        {
            options.ObjectSerializer ??= optionsAccessor.Value.DefaultObjectSerializer ?? throw new KeenConveyanceException($"The \"{nameof(KeenConveyanceOptions)}.{nameof(KeenConveyanceOptions.DefaultObjectSerializer)}\" not configured");
        }
    }

    #endregion Public 方法
}
