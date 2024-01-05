namespace KeenConveyance.ModelBinding;

/// <summary>
/// 参数值提供器
/// </summary>
public interface IParameterValueProvider
{
    #region Public 方法

    /// <summary>
    /// 获取参数 <paramref name="parameter"/> 的值
    /// </summary>
    /// <param name="parameter"></param>
    /// <returns></returns>
    ValueTask<object?> GetAsync(ParameterDescriptor parameter);

    #endregion Public 方法
}
