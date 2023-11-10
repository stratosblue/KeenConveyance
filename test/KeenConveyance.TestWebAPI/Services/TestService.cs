using Microsoft.AspNetCore.Mvc;

namespace KeenConveyance.TestWebAPI.Services;

[Route("[controller]")]
public class TestService : ITestService
{
    #region Private 字段

    private readonly ILogger<TestService> _logger;

    #endregion Private 字段

    #region Public 构造函数

    public TestService(ILogger<TestService> logger)
    {
        _logger = logger;
    }

    #endregion Public 构造函数

    #region Public 方法

    [HttpGet]
    [Route("Add_int")]
    public int Add(int a, int b)
    {
        return a + b;
    }

    [HttpGet]
    [Route("Add_int_nullable")]
    public int? Add(int? a, int? b)
    {
        return a + b;
    }

    [HttpGet]
    [Route("Add_double")]
    public double Add(double a, double b)
    {
        return a + b;
    }

    [HttpGet]
    [Route("Add_double_nullable")]
    public double? Add(double? a, double? b)
    {
        return a + b;
    }

    [HttpGet]
    [Route("AddAsync_int")]
    public Task<int> AddAsync(int a, int b, CancellationToken cancellationToken)
    {
        return Task.FromResult(a + b);
    }

    [HttpGet]
    [Route("AddAsync_int_nullable")]
    public Task<int?> AddAsync(int? a, int? b, CancellationToken cancellationToken)
    {
        return Task.FromResult(a + b);
    }

    [HttpGet]
    [Route("AddAsync_double")]
    public Task<double> AddAsync(double a, double b, CancellationToken cancellationToken)
    {
        return Task.FromResult(a + b);
    }

    [HttpGet]
    [Route("AddAsync_double_nullable")]
    public Task<double?> AddAsync(double? a, double? b, CancellationToken cancellationToken)
    {
        return Task.FromResult(a + b);
    }

    [HttpGet]
    [Route("Delay")]
    public void Delay()
    {
        Thread.Sleep(1000);
    }

    [HttpGet]
    [Route("DelayAsync")]
    public Task DelayAsync()
    {
        return Task.Delay(1000);
    }

    [HttpGet]
    [Route("DtoGenericRequest")]
    public TestGenericResponseDto<TestResponseDto> DtoGenericRequest(TestGenericRequestDto<TestRequestDto> input)
    {
        return new(new(input.Data.Value1, input.Data.Value2), input.Value1, input.Value2);
    }

    [HttpGet]
    [Route("DtoGenericRequestAsync")]
    public Task<TestGenericResponseDto<TestResponseDto>> DtoGenericRequestAsync(TestGenericRequestDto<TestRequestDto> input, CancellationToken cancellationToken)
    {
        var result = new TestGenericResponseDto<TestResponseDto>(new(input.Data.Value1, input.Data.Value2), input.Value1, input.Value2);

        return Task.FromResult(result);
    }

    [HttpGet]
    [Route("DtoRequest")]
    public TestResponseDto DtoRequest(TestRequestDto input)
    {
        return new TestResponseDto(input.Value1, input.Value2);
    }

    [HttpGet]
    [Route("DtoRequestAsync")]
    public Task<TestResponseDto> DtoRequestAsync(TestRequestDto input, CancellationToken cancellationToken)
    {
        var result = new TestResponseDto(input.Value1, input.Value2);
        return Task.FromResult(result);
    }

    [HttpGet]
    [Route("Echo")]
    public string Echo(string value)
    {
        return new string(value.AsEnumerable().Reverse().ToArray());
    }

    [HttpGet]
    [Route("EchoAsync")]
    public Task<string> EchoAsync(string value, CancellationToken cancellationToken)
    {
        var echo = new string(value.AsEnumerable().Reverse().ToArray());
        return Task.FromResult(echo);
    }

    [HttpGet]
    [Route("Hello")]
    public string Hello()
    {
        return "Hello";
    }

    [HttpGet]
    [Route("HelloAsync")]
    public Task<string> HelloAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult("Hello");
    }

    [HttpGet]
    [Route("Wait")]
    public void Wait(TimeSpan wait)
    {
        Thread.Sleep(wait);
    }

    [HttpGet]
    [Route("WaitAsync")]
    public Task WaitAsync(TimeSpan wait, CancellationToken cancellationToken)
    {
        return Task.Delay(wait, cancellationToken);
    }

    #endregion Public 方法
}
