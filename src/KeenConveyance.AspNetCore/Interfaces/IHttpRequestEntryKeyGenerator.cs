#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using Microsoft.AspNetCore.Http;

namespace KeenConveyance.AspNetCore;

/// <summary>
/// Http请求的 <see cref="HttpContext"/> 入口Key生成器
/// </summary>
public interface IHttpRequestEntryKeyGenerator
{
    #region Public 方法

    /// <summary>
    /// 尝试生成 <paramref name="httpContext"/> 的入口key
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    ValueTask<string?> GenerateKeyAsync(HttpContext httpContext);

    #endregion Public 方法
}
