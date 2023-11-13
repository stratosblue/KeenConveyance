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
        if (context.BindingInfo.BindingSource?.IsFromRequest == false)
        {
            //明确不是从请求绑定的参数，返回null，由框架进行处理
            return null;
        }
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

        //已遍历mvc的所有已配置的IModelBinderProvider，返回默认的ModelBinderProvider，中断mvc框架内部的遍历
        return KeenConveyanceMvcModelBinder.Shared;
    }

    #endregion Public 方法
}
