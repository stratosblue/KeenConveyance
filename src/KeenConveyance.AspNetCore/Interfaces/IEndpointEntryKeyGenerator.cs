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
