namespace KeenConveyance.TestWebAPI.Services;

public interface IHelloService : IApplicationService
{
    #region Public 方法

    public Task<string> HelloAsync(CancellationToken cancellationToken);

    #endregion Public 方法
}
