namespace KeenConveyance;

/// <summary>
/// 参数描述符
/// </summary>
/// <param name="ParameterIndex">参数索引</param>
/// <param name="ParameterName">参数名称</param>
/// <param name="ParameterType">参数类型</param>
public record struct ParameterDescriptor(int ParameterIndex, string ParameterName, Type ParameterType);
