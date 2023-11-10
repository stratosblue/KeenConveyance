namespace WebAPISample.Services;

internal interface ISampleService
{
    public Task<string> EchoAsync(string value, CancellationToken cancellationToken);
}
