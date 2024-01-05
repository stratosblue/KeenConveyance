namespace KeenConveyance.Client;

/// <summary>
/// 固定地址的 <see cref="IServiceAddressProvider"/>
/// </summary>
public sealed class FixedServiceAddressProvider : IServiceAddressProvider
{
    #region Private 字段

    private readonly Uri _uri;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc cref="FixedServiceAddressProvider"/>
    public FixedServiceAddressProvider(Uri uri)
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator FixedServiceAddressProvider(string value)
    {
        return new FixedServiceAddressProvider(new Uri(value));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator FixedServiceAddressProvider(Uri value)
    {
        return new FixedServiceAddressProvider(value);
    }

    /// <inheritdoc/>
    public ValueTask<Uri> RequireUriAsync(string clientName, CancellationToken cancellationToken) => ValueTask.FromResult(_uri);

    #endregion Public 方法
}
