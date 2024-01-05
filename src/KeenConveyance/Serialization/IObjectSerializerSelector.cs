namespace KeenConveyance.Serialization;

/// <summary>
/// <see cref="IObjectSerializer"/> 选择器
/// </summary>
public interface IObjectSerializerSelector
{
    #region Public 方法

    /// <summary>
    /// 根据 <paramref name="context"/> 选择一个 <see cref="IObjectSerializer"/>
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    IObjectSerializer? Select(ObjectSerializerSelectContext context);

    #endregion Public 方法
}
