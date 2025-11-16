#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

/// <summary>
/// <typeparamref name="TClient"/> 的 <see cref="IHttpClientBuilder"/>
/// </summary>
/// <typeparam name="TClient"></typeparam>
public interface IKeenConveyanceHttpClientBuilder<TClient> : IHttpClientBuilder where TClient : class
{
}
