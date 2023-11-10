using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KeenConveyance.AspNetCore;

/// <summary>
/// KeenConveyance 使用的 ModelBinder
/// </summary>
public interface IKeenConveyanceModelBinder
{
    #region Public 方法

    /// <summary>
    /// 绑定模型
    /// </summary>
    /// <param name="bindingContext"></param>
    /// <returns></returns>
    public Task BindModelAsync(ModelBindingContext bindingContext);

    #endregion Public 方法
}
