using KeenConveyance.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance.AspNetCore.Mvc.ModelBinding;

/// <summary>
/// KeenConveyance 使用的 <inheritdoc cref="IModelBinderProvider"/>
/// </summary>
/// <param name="modelBinderProviders">原始的 Mvc 配置的 <see cref="IModelBinderProvider"/> 集合</param>
public class KeenConveyanceModelBinderProvider(IList<IModelBinderProvider> modelBinderProviders)
    : IModelBinderProvider
{
    #region Private 字段

    private readonly IList<IModelBinderProvider> _providers = modelBinderProviders ?? throw new ArgumentNullException(nameof(modelBinderProviders));

    #endregion Private 字段

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
            if (provider is KeenConveyanceModelBinderProvider)
            {
                continue;
            }
            //尝试获取mvc的binder，以便在绑定失败时作为回退项
            if (provider.GetBinder(context) is { } innerModelBinder)
            {
                return new KeenConveyanceWrappedModelBinder(innerModelBinder, context.Services.GetRequiredService<IObjectSerializerSelector>());
            }
        }

        //已遍历mvc的所有已配置的IModelBinderProvider，返回 null ，不支持框架不支持绑定的Model
        return null;
    }

    #endregion Public 方法
}
