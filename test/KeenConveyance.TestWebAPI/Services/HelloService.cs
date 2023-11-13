using Microsoft.AspNetCore.Mvc;

namespace KeenConveyance.TestWebAPI.Services;

[Route("[controller]")]
public class HelloService : IHelloService
{
    #region Public 方法

    [HttpGet]
    public Task<string> HelloAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult("Hello");
    }

    #endregion Public 方法
}
