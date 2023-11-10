using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KeenConveyance.AspNetCore;

/// <summary>
/// KeenConveyance 使用的 <inheritdoc cref="IModelBinderProvider"/>
/// </summary>
public class DefaultKeenConveyanceMvcModelBinderProvider : IModelBinderProvider
{
    #region Private 字段

    private readonly IList<IModelBinderProvider> _providers;

    #endregion Private 字段

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="DefaultKeenConveyanceMvcModelBinderProvider"/>
    /// </summary>
    /// <param name="modelBinderProviders">原始的 Mvc 配置的 <see cref="IModelBinderProvider"/> 集合</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DefaultKeenConveyanceMvcModelBinderProvider(IList<IModelBinderProvider> modelBinderProviders)
    {
        _providers = modelBinderProviders ?? throw new ArgumentNullException(nameof(modelBinderProviders));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        for (var i = 0; i < _providers.Count; i++)
        {
            var provider = _providers[i];
            if (provider is DefaultKeenConveyanceMvcModelBinderProvider)
            {
                continue;
            }
            if (provider.GetBinder(context) is { } innerModelBinder)
            {
                return new KeenConveyanceWrappedMvcModelBinder(innerModelBinder);
            }
        }

        return KeenConveyanceMvcModelBinder.Shared;
    }

    #endregion Public 方法
}
