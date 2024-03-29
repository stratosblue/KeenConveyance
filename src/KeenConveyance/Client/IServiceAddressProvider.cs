﻿namespace KeenConveyance.Client;

/// <summary>
/// 服务地址提供器
/// </summary>
public interface IServiceAddressProvider
{
    #region Public 方法

    /// <summary>
    /// 获取服务地址
    /// </summary>
    /// <param name="clientName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<Uri> RequireUriAsync(string clientName, CancellationToken cancellationToken);

    #endregion Public 方法
}
