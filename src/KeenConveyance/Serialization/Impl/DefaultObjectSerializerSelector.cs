#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using System.Collections.Immutable;
using Microsoft.Extensions.Options;

namespace KeenConveyance.Serialization;

/// <summary>
/// 默认的 <see cref="IObjectSerializerSelector"/>
/// </summary>
/// <param name="optionsAccessor"></param>
public sealed class DefaultObjectSerializerSelector(IOptions<KeenConveyanceOptions> optionsAccessor) : IObjectSerializerSelector
{
    #region Private 字段

    private readonly ImmutableDictionary<SpecificMediaType, IObjectSerializer> _serializerDictionary = optionsAccessor.Value.ObjectSerializers.ToImmutableDictionary(m => m.SupportedMediaType);

    #endregion Private 字段

    #region Public 方法

    /// <inheritdoc/>
    public IObjectSerializer? Select(ObjectSerializerSelectContext context)
    {
        if (_serializerDictionary.TryGetValue(context.MediaType, out var objectSerializer))
        {
            return objectSerializer;
        }
        return null;
    }

    #endregion Public 方法
}
