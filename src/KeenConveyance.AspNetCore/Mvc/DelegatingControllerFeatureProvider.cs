using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace KeenConveyance.AspNetCore.Mvc;

internal sealed class DelegatingControllerFeatureProvider(Predicate<TypeInfo> controllerPredicate)
    : ControllerFeatureProvider
{
    #region Private 字段

    private readonly Predicate<TypeInfo> _controllerPredicate = controllerPredicate ?? throw new ArgumentNullException(nameof(controllerPredicate));

    #endregion Private 字段

    #region Protected 方法

    protected override bool IsController(TypeInfo typeInfo)
    {
        return base.IsController(typeInfo)
               || !typeInfo.IsInterface
               && _controllerPredicate(typeInfo);
    }

    #endregion Protected 方法
}

