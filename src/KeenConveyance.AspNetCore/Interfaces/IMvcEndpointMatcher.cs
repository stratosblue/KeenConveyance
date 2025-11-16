#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using Microsoft.AspNetCore.Http;

namespace KeenConveyance.AspNetCore;

/// <summary>
/// AspNetCore MvcEndpoint 匹配器，用于将 KeenConveyance 请求匹配到 MvcEndpoint
/// </summary>
public interface IMvcEndpointMatcher
{
    #region Public 方法

    /// <summary>
    /// 尝试将 <paramref name="httpContext"/> 匹配到 <see cref="Endpoint"/>
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    ValueTask<Endpoint?> MatchEndpointAsync(HttpContext httpContext);

    #endregion Public 方法
}
