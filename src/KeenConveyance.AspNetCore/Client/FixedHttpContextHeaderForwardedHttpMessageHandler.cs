using Microsoft.AspNetCore.Http;

namespace KeenConveyance.AspNetCore.Client;

internal sealed class FixedHttpContextHeaderForwardedHttpMessageHandler(IHttpContextAccessor httpContextAccessor)
    : DelegatingHandler
{
    #region Private 字段

    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

    private string[] _headers = [];

    #endregion Private 字段

    #region Public 方法

    public FixedHttpContextHeaderForwardedHttpMessageHandler SetHeaders(string[] headers)
    {
        _headers = headers;
        return this;
    }

    #endregion Public 方法

    #region Protected 方法

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_httpContextAccessor.HttpContext is HttpContext httpContext)
        {
            foreach (var header in _headers)
            {
                if (!request.Headers.Contains(header)
                     && httpContext.Request.Headers.TryGetValue(header, out var value))
                {
                    request.Headers.TryAddWithoutValidation(header, value.ToString());
                }
            }
        }
        return base.SendAsync(request, cancellationToken);
    }

    #endregion Protected 方法
}
