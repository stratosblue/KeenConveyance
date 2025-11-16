#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace KeenConveyance.AspNetCore;

internal sealed class PathBaseHttpRequestEntryKeyGenerator : IHttpRequestEntryKeyGenerator
{
    #region Private 字段

    private readonly int _minimumPathLength;

    private readonly KeenConveyanceMvcOptions _options;

    private readonly PathString _serviceEntryPath;

    #endregion Private 字段

    #region Public 构造函数

    public PathBaseHttpRequestEntryKeyGenerator(IOptions<KeenConveyanceMvcOptions> optionsAccessor)
    {
        _options = optionsAccessor.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        _serviceEntryPath = _options.ServiceEntryPath;
        if (!_serviceEntryPath.HasValue)
        {
            throw new ArgumentException($"{nameof(optionsAccessor.Value.ServiceEntryPath)} is empty");
        }

        // 去除 /
        _minimumPathLength = _serviceEntryPath.Value.Length + 1;
    }

    #endregion Public 构造函数

    #region Public 方法

    public ValueTask<string?> GenerateKeyAsync(HttpContext httpContext)
    {
        if (httpContext.Request.Path.Value is string path
            && path.Length > _minimumPathLength)
        {
            var key = httpContext.Request.Path.Value.Substring(_minimumPathLength);
            return ValueTask.FromResult<string?>(key);
        }
        return default;
    }

    #endregion Public 方法
}
