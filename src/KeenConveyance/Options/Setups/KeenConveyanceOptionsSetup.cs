using KeenConveyance.Serialization;
using Microsoft.Extensions.Options;

namespace KeenConveyance;

internal sealed class KeenConveyanceOptionsSetup(IOptions<KeenConveyanceJsonOptions> jsonOptions) : IConfigureOptions<KeenConveyanceOptions>
{
    #region Public 方法

    public void Configure(KeenConveyanceOptions options)
    {
        var systemTextJsonObjectSerializer = new SystemTextJsonObjectSerializer(jsonOptions.Value.JsonSerializerOptions);
        options.DefaultObjectSerializer = systemTextJsonObjectSerializer;
        options.ObjectSerializers.Add(systemTextJsonObjectSerializer);
    }

    #endregion Public 方法
}
