#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

namespace KeenConveyance;

/// <summary>
/// 参数描述符
/// </summary>
/// <param name="ParameterIndex">参数索引</param>
/// <param name="ParameterName">参数名称</param>
/// <param name="ParameterType">参数类型</param>
public record struct ParameterDescriptor(int ParameterIndex, string ParameterName, Type ParameterType);
