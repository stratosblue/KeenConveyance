#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

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
