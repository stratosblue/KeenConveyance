#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using Microsoft.AspNetCore.Http;

namespace KeenConveyance.AspNetCore;

/// <summary>
/// <see cref="Endpoint"/> 入口Key生成器
/// </summary>
public interface IEndpointEntryKeyGenerator
{
    #region Public 方法

    /// <summary>
    /// 尝试生成 <paramref name="endpoint"/> 的入口key
    /// </summary>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    string? GenerateKey(Endpoint endpoint);

    #endregion Public 方法
}
