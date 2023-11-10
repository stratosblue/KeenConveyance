namespace KeenConveyance;

/// <summary>
/// 客户端使用的 <see cref="HttpRequestMessage"/> 构造器
/// </summary>
public interface IHttpRequestMessageConstructor
{
    #region Public 方法

    /// <summary>
    /// 构造用于请求的 <see cref="HttpRequestMessage"/>
    /// </summary>
    /// <param name="serviceAddress"></param>
    /// <param name="entryKey"></param>
    /// <param name="httpContent"></param>
    /// <returns></returns>
    public HttpRequestMessage CreateHttpRequestMessage(Uri serviceAddress, string entryKey, HttpContent? httpContent);

    #endregion Public 方法
}
