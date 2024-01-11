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
    public List<IObjectSerializer> ObjectSerializers { get; } = new();

    #endregion Public 属性
}
