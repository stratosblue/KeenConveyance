#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using System.ComponentModel;
using KeenConveyance.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

/// <summary>
/// <see cref="KeenConveyanceMvcOptions"/> 拓展方法类
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class KeenConveyanceMvcOptionsExtensions
{
    #region Public 方法

    /// <summary>
    /// 设置 <see cref="IHttpRequestEntryKeyGenerator"/>
    /// </summary>
    /// <typeparam name="TGenerator"></typeparam>
    /// <param name="options"></param>
    /// <param name="serviceLifetime"></param>
    /// <returns></returns>
    public static KeenConveyanceMvcOptions UseHttpRequestEntryKeyGenerator<TGenerator>(this KeenConveyanceMvcOptions options, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where TGenerator : IHttpRequestEntryKeyGenerator
    {
        options.HttpRequestEntryKeyGeneratorServiceDescriptor = ServiceDescriptor.Describe(typeof(IHttpRequestEntryKeyGenerator), typeof(TGenerator), serviceLifetime);
        return options;
    }

    /// <summary>
    /// 使用基于路径的 <see cref="IHttpRequestEntryKeyGenerator"/>
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static KeenConveyanceMvcOptions UsePathHttpRequestEntryKeyGenerator(this KeenConveyanceMvcOptions options)
    {
        return options.UseHttpRequestEntryKeyGenerator<PathBaseHttpRequestEntryKeyGenerator>(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// 使用基于Query的 <see cref="IHttpRequestEntryKeyGenerator"/>
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static KeenConveyanceMvcOptions UseQueryHttpRequestEntryKeyGenerator(this KeenConveyanceMvcOptions options)
    {
        return options.UseHttpRequestEntryKeyGenerator<QueryBaseHttpRequestEntryKeyGenerator>(ServiceLifetime.Singleton);
    }

    #endregion Public 方法
}
