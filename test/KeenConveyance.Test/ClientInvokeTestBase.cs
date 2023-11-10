using System.Diagnostics;
using KeenConveyance.Test;
using KeenConveyance.TestWebAPI;
using KeenConveyance.TestWebAPI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance;

public abstract class ClientInvokeTestBase<TTestStartup> : TestServerBaseTestBase<TTestStartup> where TTestStartup : ITestStartup, new()
{
    #region Protected 方法

    private void RequiredTestServices(out ITestService testService, out TestService testServiceImpl)
    {
        testService = RequiredService<ITestService>();
        testServiceImpl = RequiredService<TestService>();
        Assert.IsNotNull(testService);
        Assert.IsNotNull(testServiceImpl);
    }

    protected override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<TestService>();
        services.AddKeenConveyance()
                .ConfigureClient(builder =>
                {
                    builder.AddClient<ITestService>(ConfigureWithTestServer)
                           .CompleteClientSetup();
                });
        return services;
    }

    protected T RequiredService<T>() where T : notnull => ServiceProvider.GetRequiredService<T>();

    #endregion Protected 方法

    #region Public 方法

    [TestMethod]
    public async Task Should_Cancel_Success_Async()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var wait = TimeSpan.FromSeconds(2);

        using var cts = new CancellationTokenSource();

        var delayTask1 = Task.Run(async () =>
        {
            var sw = Stopwatch.StartNew();
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => testServiceImpl.WaitAsync(wait, cts.Token));
            sw.Stop();
            return sw.ElapsedMilliseconds;
        });
        var delayTask2 = Task.Run(async () =>
        {
            var sw = Stopwatch.StartNew();
            await Assert.ThrowsExceptionAsync<TaskCanceledException>(() => testService.WaitAsync(wait, cts.Token));
            sw.Stop();
            return sw.ElapsedMilliseconds;
        });

        cts.CancelAfter(TimeSpan.FromSeconds(1));

        await Task.WhenAll(delayTask1, delayTask2);

        Assert.IsTrue(delayTask1.Result < 2000, $"Complete use: {delayTask1.Result}");
        Assert.IsTrue(delayTask2.Result < 2000, $"Complete use: {delayTask2.Result}");
    }

    [TestMethod]
    public async Task Should_Success_Delay()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var delayTask1 = Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            testServiceImpl.Delay();
            sw.Stop();
            return sw.ElapsedMilliseconds;
        });
        var delayTask2 = Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            testService.Delay();
            sw.Stop();
            return sw.ElapsedMilliseconds;
        });

        await Task.WhenAll(delayTask1, delayTask2);

        //等待时间正确
        Assert.IsTrue(Math.Abs(delayTask1.Result - 1000) < delayTask1.Result / 10);

        //误差时间在十分之一以内
        Assert.IsTrue(Math.Abs(delayTask1.Result - delayTask2.Result) < delayTask1.Result / 10);
    }

    [TestMethod]
    public async Task Should_Success_Delay_Async()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var delayTask1 = Task.Run(async () =>
        {
            var sw = Stopwatch.StartNew();
            await testServiceImpl.DelayAsync();
            sw.Stop();
            return sw.ElapsedMilliseconds;
        });
        var delayTask2 = Task.Run(async () =>
        {
            var sw = Stopwatch.StartNew();
            await testService.DelayAsync();
            sw.Stop();
            return sw.ElapsedMilliseconds;
        });

        await Task.WhenAll(delayTask1, delayTask2);

        //等待时间正确
        Assert.IsTrue(Math.Abs(delayTask1.Result - 1000) < delayTask1.Result / 10);

        //误差时间在十分之一以内
        Assert.IsTrue(Math.Abs(delayTask1.Result - delayTask2.Result) < delayTask1.Result / 10);
    }

    [TestMethod]
    public void Should_Success_Echo()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var input = Guid.NewGuid().ToString();

        Assert.AreEqual(testServiceImpl.Echo(input), testService.Echo(input));
    }

    [TestMethod]
    public async Task Should_Success_Echo_Async()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var input = Guid.NewGuid().ToString();

        Assert.AreEqual(await testServiceImpl.EchoAsync(input, default), await testService.EchoAsync(input, default));
    }

    [TestMethod]
    public void Should_Success_Hello()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        Assert.AreEqual(testServiceImpl.Hello(), testService.Hello());
    }

    [TestMethod]
    public async Task Should_Success_Hello_Async()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        Assert.AreEqual(await testServiceImpl.HelloAsync(default), await testService.HelloAsync(default));
    }

    [TestMethod]
    public async Task Should_Success_Wait()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var wait = TimeSpan.FromSeconds(Random.Shared.NextDouble());

        var delayTask1 = Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            testServiceImpl.Wait(wait);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        });
        var delayTask2 = Task.Run(() =>
        {
            var sw = Stopwatch.StartNew();
            testService.Wait(wait);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        });

        await Task.WhenAll(delayTask1, delayTask2);

        //误差时间在 50ms 以内
        Assert.IsTrue(Math.Abs(delayTask1.Result - delayTask2.Result) < 50);
    }

    [TestMethod]
    public async Task Should_Success_Wait_Async()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var wait = TimeSpan.FromSeconds(Random.Shared.NextDouble());

        var delayTask1 = Task.Run(async () =>
        {
            var sw = Stopwatch.StartNew();
            await testServiceImpl.WaitAsync(wait, default);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        });
        var delayTask2 = Task.Run(async () =>
        {
            var sw = Stopwatch.StartNew();
            await testService.WaitAsync(wait, default);
            sw.Stop();
            return sw.ElapsedMilliseconds;
        });

        await Task.WhenAll(delayTask1, delayTask2);

        //误差时间在 50ms 以内
        Assert.IsTrue(Math.Abs(delayTask1.Result - delayTask2.Result) < 50);
    }

    [TestMethod]
    public void Should_Success_With_Double_Param()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var a = Random.Shared.NextDouble();
        var b = Random.Shared.NextDouble();

        Assert.AreEqual(testServiceImpl.Add(a, b), testService.Add(a, b));
    }

    [TestMethod]
    public async Task Should_Success_With_Double_Param_Async()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var a = Random.Shared.NextDouble();
        var b = Random.Shared.NextDouble();

        Assert.AreEqual(await testServiceImpl.AddAsync(a, b, default), await testService.AddAsync(a, b, default));
    }

    [TestMethod]
    public void Should_Success_With_Dto()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var input = new TestRequestDto(Random.Shared.Next(), Guid.NewGuid().ToString());

        Assert.AreEqual(testServiceImpl.DtoRequest(input), testService.DtoRequest(input));
    }

    [TestMethod]
    public async Task Should_Success_With_Dto_Async()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var input = new TestRequestDto(Random.Shared.Next(), Guid.NewGuid().ToString());

        Assert.AreEqual(await testServiceImpl.DtoRequestAsync(input, default), await testService.DtoRequestAsync(input, default));
    }

    [TestMethod]
    public void Should_Success_With_Generic_Dto()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var input = new TestGenericRequestDto<TestRequestDto>(new(Random.Shared.Next(), Guid.NewGuid().ToString()), Random.Shared.Next(), Guid.NewGuid().ToString());

        Assert.AreEqual(testServiceImpl.DtoGenericRequest(input), testService.DtoGenericRequest(input));
    }

    [TestMethod]
    public async Task Should_Success_With_Generic_Dto_Async()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var input = new TestGenericRequestDto<TestRequestDto>(new(Random.Shared.Next(), Guid.NewGuid().ToString()), Random.Shared.Next(), Guid.NewGuid().ToString());

        Assert.AreEqual(await testServiceImpl.DtoGenericRequestAsync(input, default), await testService.DtoGenericRequestAsync(input, default));
    }

    [TestMethod]
    public void Should_Success_With_Int_Param()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var a = Random.Shared.Next();
        var b = Random.Shared.Next();

        Assert.AreEqual(testServiceImpl.Add(a, b), testService.Add(a, b));
    }

    [TestMethod]
    public async Task Should_Success_With_Int_Param_Async()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        var a = Random.Shared.Next();
        var b = Random.Shared.Next();

        Assert.AreEqual(await testServiceImpl.AddAsync(a, b, default), await testService.AddAsync(a, b, default));
    }

    [TestMethod]
    public void Should_Success_With_Nullable_Double_Param()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        double? a = Random.Shared.NextDouble();
        double? b = Random.Shared.NextDouble();

        Assert.AreEqual(testServiceImpl.Add(a, b), testService.Add(a, b));
        Assert.AreEqual(testServiceImpl.Add(a, null), testService.Add(a, null));
        Assert.AreEqual(testServiceImpl.Add(null, b), testService.Add(null, b));
        Assert.AreEqual(testServiceImpl.Add((double?)null, (double?)null), testService.Add((double?)null, (double?)null));
    }

    [TestMethod]
    public async Task Should_Success_With_Nullable_Double_Param_Async()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        double? a = Random.Shared.NextDouble();
        double? b = Random.Shared.NextDouble();

        Assert.AreEqual(await testServiceImpl.AddAsync(a, b, default), await testService.AddAsync(a, b, default));

        Assert.AreEqual(await testServiceImpl.AddAsync(a, b, default), await testService.AddAsync(a, b, default));
        Assert.AreEqual(await testServiceImpl.AddAsync(a, null, default), await testService.AddAsync(a, null, default));
        Assert.AreEqual(await testServiceImpl.AddAsync(null, b, default), await testService.AddAsync(null, b, default));
        Assert.AreEqual(await testServiceImpl.AddAsync((double?)null, (double?)null, default), await testService.AddAsync((double?)null, (double?)null, default));
    }

    [TestMethod]
    public void Should_Success_With_Nullable_Int_Param()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        int? a = Random.Shared.Next();
        int? b = Random.Shared.Next();

        Assert.AreEqual(testServiceImpl.Add(a, b), testService.Add(a, b));
        Assert.AreEqual(testServiceImpl.Add(a, null), testService.Add(a, null));
        Assert.AreEqual(testServiceImpl.Add(null, b), testService.Add(null, b));
        Assert.AreEqual(testServiceImpl.Add((int?)null, (int?)null), testService.Add((int?)null, (int?)null));
    }

    [TestMethod]
    public async Task Should_Success_With_Nullable_Int_Param_Async()
    {
        RequiredTestServices(out var testService, out var testServiceImpl);

        int? a = Random.Shared.Next();
        int? b = Random.Shared.Next();

        Assert.AreEqual(await testServiceImpl.AddAsync(a, b, default), await testService.AddAsync(a, b, default));

        Assert.AreEqual(await testServiceImpl.AddAsync(a, b, default), await testService.AddAsync(a, b, default));
        Assert.AreEqual(await testServiceImpl.AddAsync(a, null, default), await testService.AddAsync(a, null, default));
        Assert.AreEqual(await testServiceImpl.AddAsync(null, b, default), await testService.AddAsync(null, b, default));
        Assert.AreEqual(await testServiceImpl.AddAsync((int?)null, (int?)null, default), await testService.AddAsync((int?)null, (int?)null, default));
    }

    #endregion Public 方法
}
