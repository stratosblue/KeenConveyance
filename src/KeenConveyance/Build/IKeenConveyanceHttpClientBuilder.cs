using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

/// <summary>
/// <typeparamref name="TClient"/> 的 <see cref="IHttpClientBuilder"/>
/// </summary>
/// <typeparam name="TClient"></typeparam>
public interface IKeenConveyanceHttpClientBuilder<TClient> : IHttpClientBuilder where TClient : class
{
}
