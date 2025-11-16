using KeenConveyance.Client;
using KeenConveyance.TestWebAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KeenConveyance.Test;

public abstract class TestServerBaseTestBase<TTestStartup> where TTestStartup : ITestStartup, new()
{
    #region Protected 字段

    protected ServiceProvider RootServiceProvider = null!;

    protected AsyncServiceScope ServiceScope;

    protected TestServer TestServer = null!;

    protected WebApplication WebApplication = null!;

    #endregion Protected 字段

    #region Protected 属性

    protected IServiceProvider ServiceProvider => ServiceScope.ServiceProvider;

    #endregion Protected 属性

    #region Public 方法

    [TestCleanup]
    public async Task TestCleanupAsync()
    {
        await WebApplication.StopAsync();
        await ServiceScope.DisposeAsync();
        await RootServiceProvider.DisposeAsync();
    }

    [TestInitialize]
    public async Task TestInitializeAsync()
    {
        WebApplication = new TTestStartup().Build();

        await WebApplication.StartAsync();

        TestServer = WebApplication.GetTestServer();

        IServiceCollection services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());

        services = ConfigureServices(services);

        RootServiceProvider = services.BuildServiceProvider();
        ServiceScope = RootServiceProvider.CreateAsyncScope();
    }

    #endregion Public 方法

    #region Protected 方法

    protected abstract IServiceCollection ConfigureServices(IServiceCollection services);

    protected virtual void ConfigureWithTestServer(IHttpClientBuilder httpClientBuilder)
    {
        httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() => TestServer.CreateHandler());
    }

    protected virtual void ConfigureWithTestServer(KeenConveyanceClientOptions options)
    {
        options.ServiceAddressProvider = new FixedServiceAddressProvider(TestServer.BaseAddress);
    }

    #endregion Protected 方法
}
