using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance.AspNetCore;

internal sealed class KeenConveyanceMvcModelBinder : IModelBinder
{
    #region Public 属性

    public static KeenConveyanceMvcModelBinder Shared { get; } = new();

    #endregion Public 属性

    #region Public 方法

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (!bindingContext.HttpContext.IsKeenConveyanceRequest()
            || bindingContext.BindingSource?.IsFromRequest == false)
        {
            return Task.CompletedTask;
        }
        var keenConveyanceModelBinder = bindingContext.HttpContext.RequestServices.GetRequiredService<IKeenConveyanceModelBinder>();
        return keenConveyanceModelBinder.BindModelAsync(bindingContext);
    }

    #endregion Public 方法
}
