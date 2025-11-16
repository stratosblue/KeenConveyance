#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

namespace WebAPISample.Services;

internal interface ISampleService
{
    public Task<string> EchoAsync(string value, CancellationToken cancellationToken);
}
