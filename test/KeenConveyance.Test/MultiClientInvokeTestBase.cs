using KeenConveyance.Test;
using KeenConveyance.TestWebAPI;
using KeenConveyance.TestWebAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

public abstract class MultiClientInvokeTestBase<TTestStartup> : ClientInvokeTestBase<TTestStartup> where TTestStartup : ITestStartup, new()
{
    #region Protected 方法

    protected override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<TestService>();
        services.AddScoped<HelloService>();
        services.AddKeenConveyance()
                .ConfigureClient(builder =>
                {
                    builder.BeginSetupClients(ConfigureWithTestServer, ConfigureWithTestServer)
                           .AddClient<ITestService>()
                           .AddClient<IHelloService>()
                           .CompleteClientsSetup();
                });
        return services;
    }

    #endregion Protected 方法

    #region Public 方法

    [TestMethod]
    public async Task Should_Hello_Success()
    {
        var helloService = RequiredService<IHelloService>();
        var helloServiceImpl = RequiredService<HelloService>();

        Assert.AreEqual(await helloServiceImpl.HelloAsync(default), await helloService.HelloAsync(default));
    }

    #endregion Public 方法
}
