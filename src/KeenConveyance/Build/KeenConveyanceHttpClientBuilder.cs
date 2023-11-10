using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

internal class KeenConveyanceHttpClientBuilder<TClient> : IKeenConveyanceHttpClientBuilder<TClient>
    where TClient : class
{
    #region Private 字段

    private readonly IHttpClientBuilder _httpClientBuilder;

    #endregion Private 字段

    #region Public 属性

    public string Name => _httpClientBuilder.Name;

    public IServiceCollection Services => _httpClientBuilder.Services;

    #endregion Public 属性

    #region Public 构造函数

    public KeenConveyanceHttpClientBuilder(IHttpClientBuilder httpClientBuilder)
    {
        _httpClientBuilder = httpClientBuilder ?? throw new ArgumentNullException(nameof(httpClientBuilder));
    }

    #endregion Public 构造函数
}
