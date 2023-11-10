namespace KeenConveyance;

/// <summary>
/// 服务地址提供器
/// </summary>
public interface IServiceAddressProvider
{
    #region Public 方法

    /// <summary>
    /// 获取服务地址
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<Uri> RequireUriAsync(CancellationToken cancellationToken);

    #endregion Public 方法
}
