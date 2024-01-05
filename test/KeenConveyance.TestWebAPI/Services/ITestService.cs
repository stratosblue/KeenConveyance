using MemoryPack;

namespace KeenConveyance.TestWebAPI.Services;

public interface ITestService : IApplicationService
{
    #region Public 方法

    public int Add(int a, int b);

    public int? Add(int? a, int? b);

    public double Add(double a, double b);

    public double? Add(double? a, double? b);

    public Task<int> AddAsync(int a, int b, CancellationToken cancellationToken);

    public Task<int?> AddAsync(int? a, int? b, CancellationToken cancellationToken);

    public Task<double> AddAsync(double a, double b, CancellationToken cancellationToken);

    public Task<double?> AddAsync(double? a, double? b, CancellationToken cancellationToken);

    public void Delay();

    public Task DelayAsync();

    public TestGenericResponseDto<TestResponseDto> DtoGenericRequest(TestGenericRequestDto<TestRequestDto> input);

    public Task<TestGenericResponseDto<TestResponseDto>> DtoGenericRequestAsync(TestGenericRequestDto<TestRequestDto> input, CancellationToken cancellationToken);

    public TestResponseDto DtoRequest(TestRequestDto input);

    public Task<TestResponseDto> DtoRequestAsync(TestRequestDto input, CancellationToken cancellationToken);

    public string Echo(string value);

    public Task<string> EchoAsync(string value, CancellationToken cancellationToken);

    public string Hello();

    public Task<string> HelloAsync(CancellationToken cancellationToken);

    public void Wait(TimeSpan wait);

    public Task WaitAsync(TimeSpan wait, CancellationToken cancellationToken);

    #endregion Public 方法
}

[MemoryPackable]
public partial record TestRequestDto(int Value1, string Value2);

[MemoryPackable]
public partial record TestResponseDto(int Value1, string Value2);

[MemoryPackable]
public partial record TestGenericRequestDto<T>(T Data, int Value1, string Value2);

[MemoryPackable]
public partial record TestGenericResponseDto<T>(T Data, int Value1, string Value2);
