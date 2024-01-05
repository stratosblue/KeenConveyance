namespace KeenConveyance.Serialization;

/// <summary>
/// <see cref="IObjectSerializerSelector"/> 选择上下文
/// </summary>
/// <param name="MediaType">当前的媒体类型</param>
public record struct ObjectSerializerSelectContext(SpecificMediaType MediaType);
