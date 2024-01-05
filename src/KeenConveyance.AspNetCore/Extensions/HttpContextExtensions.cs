using System.ComponentModel;
using System.Runtime.CompilerServices;
using KeenConveyance;

namespace Microsoft.AspNetCore.Http;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class HttpContextExtensions
{
    #region Public 方法

    /// <summary>
    /// 判断当前请求是否来自 KeenConveyance
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKeenConveyanceRequest(this HttpContext httpContext)
    {
        return httpContext.Items.ContainsKey(KeenConveyanceConstants.VariantRequestMarkKey);
    }

    /// <summary>
    /// 判断当前请求是否来自 KeenConveyance
    /// </summary>
    /// <param name="httpRequest"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKeenConveyanceRequest(this HttpRequest httpRequest)
    {
        return httpRequest.HttpContext.IsKeenConveyanceRequest();
    }

    #endregion Public 方法
}
