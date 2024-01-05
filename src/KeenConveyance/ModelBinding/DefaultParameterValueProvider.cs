using KeenConveyance.Serialization;

namespace KeenConveyance.ModelBinding;

/// <summary>
/// 默认的 <see cref="IParameterValueProvider"/>
/// </summary>
public sealed class DefaultParameterValueProvider
    : IParameterValueProvider
{
    #region Private 字段

    private readonly IMultipleObjectCollection _multipleObjectCollection;

    #endregion Private 字段

    #region Public 构造函数

    private DefaultParameterValueProvider(IMultipleObjectCollection multipleObjectCollection)
    {
        _multipleObjectCollection = multipleObjectCollection ?? throw new ArgumentNullException(nameof(multipleObjectCollection));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 使用 <paramref name="multipleObjectCollection"/> 创建一个 <see cref="IParameterValueProvider"/>
    /// </summary>
    /// <param name="multipleObjectCollection"></param>
    /// <returns></returns>
    public static IParameterValueProvider Create(IMultipleObjectCollection multipleObjectCollection)
    {
        if (multipleObjectCollection is IParameterValueProvider parameterValueProvider)
        {
            return parameterValueProvider;
        }
        return new DefaultParameterValueProvider(multipleObjectCollection);
    }

    /// <inheritdoc/>
    public ValueTask<object?> GetAsync(ParameterDescriptor parameter)
    {
        var value = _multipleObjectCollection[parameter.ParameterIndex];
        return ValueTask.FromResult(value);
    }

    #endregion Public 方法
}
