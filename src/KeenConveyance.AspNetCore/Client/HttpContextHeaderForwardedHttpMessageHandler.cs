using Microsoft.AspNetCore.Http;

namespace KeenConveyance.AspNetCore.Client;

internal sealed class HttpContextHeaderForwardedHttpMessageHandler : DelegatingHandler
{
    #region Private 字段

    private readonly IHttpContextAccessor _httpContextAccessor;

    #endregion Private 字段

    #region Public 构造函数

    public HttpContextHeaderForwardedHttpMessageHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    #endregion Public 构造函数

    #region Protected 方法

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            foreach (var (key, value) in httpContext.Request.Headers)
            {
                if (!request.Headers.Contains(key))
                {
                    request.Headers.TryAddWithoutValidation(key, value.ToString());
                }
            }
        }
        return base.SendAsync(request, cancellationToken);
    }

    #endregion Protected 方法
}
