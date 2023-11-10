using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance.AspNetCore;

internal class KeenConveyanceWrappedMvcModelBinder : IModelBinder
{
    #region Private 字段

    private readonly IModelBinder _innerModelBinder;

    #endregion Private 字段

    #region Public 构造函数

    public KeenConveyanceWrappedMvcModelBinder(IModelBinder innerModelBinder)
    {
        _innerModelBinder = innerModelBinder ?? throw new ArgumentNullException(nameof(innerModelBinder));
    }

    #endregion Public 构造函数

    #region Private 方法

    private async Task BindModelByKeenConveyanceModelBinderAsync(ModelBindingContext bindingContext)
    {
        var keenConveyanceModelBinder = bindingContext.HttpContext.RequestServices.GetRequiredService<IKeenConveyanceModelBinder>();
        await keenConveyanceModelBinder.BindModelAsync(bindingContext).ConfigureAwait(false);

        if (!bindingContext.Result.IsModelSet)
        {
            await _innerModelBinder.BindModelAsync(bindingContext).ConfigureAwait(false);
        }
    }

    #endregion Private 方法

    #region Public 方法

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (!bindingContext.HttpContext.IsKeenConveyanceRequest()
            || bindingContext.BindingSource?.IsFromRequest == false)
        {
            return _innerModelBinder.BindModelAsync(bindingContext) ?? Task.CompletedTask;
        }
        return BindModelByKeenConveyanceModelBinderAsync(bindingContext);
    }

    #endregion Public 方法
}
