using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace KeenConveyance.AspNetCore.Mvc;

internal sealed class DelegatingControllerFeatureProvider : ControllerFeatureProvider
{
    #region Private 字段

    private readonly Predicate<TypeInfo> _controllerPredicate;

    #endregion Private 字段

    #region Public 构造函数

    public DelegatingControllerFeatureProvider(Predicate<TypeInfo> controllerPredicate)
    {
        _controllerPredicate = controllerPredicate ?? throw new ArgumentNullException(nameof(controllerPredicate));
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override bool IsController(TypeInfo typeInfo)
    {
        return base.IsController(typeInfo)
               || !typeInfo.IsInterface
               && _controllerPredicate(typeInfo);
    }

    #endregion Protected 方法
}

