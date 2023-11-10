namespace KeenConveyance;

internal sealed class KeenConveyanceFixedServiceAddressProvider : IServiceAddressProvider
{
    #region Private 字段

    private readonly Uri _uri;

    #endregion Private 字段

    #region Public 构造函数

    public KeenConveyanceFixedServiceAddressProvider(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (!uri.IsAbsoluteUri)
        {
            throw new ArgumentException($"\"{uri}\" is not a absolute uri");
        }
        _uri = uri;
    }

    #endregion Public 构造函数

    #region Public 方法

    public ValueTask<Uri> RequireUriAsync(CancellationToken cancellationToken) => ValueTask.FromResult(_uri);

    #endregion Public 方法
}
