namespace WebAPISample.Services;

public interface ISampleService : IApplicationService
{
    public Task<string> EchoAsync(string value, CancellationToken cancellationToken);
}
