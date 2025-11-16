#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

namespace KeenConveyance.Serialization;

/// <summary>
/// <see cref="IObjectSerializerSelector"/> 选择上下文
/// </summary>
/// <param name="MediaType">当前的媒体类型</param>
public record struct ObjectSerializerSelectContext(SpecificMediaType MediaType);
