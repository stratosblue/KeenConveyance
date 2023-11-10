using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace KeenConveyance.AspNetCore;

internal sealed class QueryBaseHttpRequestEntryKeyGenerator : IHttpRequestEntryKeyGenerator
{
    #region Private 字段

    private readonly string _queryKey;

    #endregion Private 字段

    #region Public 构造函数

    public QueryBaseHttpRequestEntryKeyGenerator(IOptions<QueryBaseEntryKeyOptions> optionsAccessor)
    {
        _queryKey = optionsAccessor?.Value?.QueryKey ?? throw new ArgumentNullException(nameof(optionsAccessor));
    }

    #endregion Public 构造函数

    #region Public 方法

    public ValueTask<string?> GenerateKeyAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Query.TryGetValue(_queryKey, out var values)
            && values.Count == 1)
        {
            return ValueTask.FromResult(values[0]);
        }
        return default;
    }

    #endregion Public 方法
}
