#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using KeenConveyance.Serialization;

namespace KeenConveyance;

/// <summary>
/// KeenConveyance 选项
/// </summary>
public class KeenConveyanceOptions
{
    #region Public 属性

    /// <summary>
    /// 默认使用的 <see cref="IObjectSerializer"/>
    /// </summary>
    public IObjectSerializer DefaultObjectSerializer { get; set; } = null!;

    /// <summary>
    ///  <see cref="IObjectSerializer"/> 列表
    /// </summary>
    public List<IObjectSerializer> ObjectSerializers { get; } = [];

    #endregion Public 属性
}
