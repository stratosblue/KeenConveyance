#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

internal class KeenConveyanceHttpClientBuilder<TClient>(IHttpClientBuilder httpClientBuilder)
    : IKeenConveyanceHttpClientBuilder<TClient>
    where TClient : class
{
    #region Private 字段

    private readonly IHttpClientBuilder _httpClientBuilder = httpClientBuilder ?? throw new ArgumentNullException(nameof(httpClientBuilder));

    #endregion Private 字段

    #region Public 属性

    public string Name => _httpClientBuilder.Name;

    public IServiceCollection Services => _httpClientBuilder.Services;

    #endregion Public 属性
}
