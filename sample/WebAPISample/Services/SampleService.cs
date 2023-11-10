using Microsoft.AspNetCore.Mvc;

namespace WebAPISample.Services;

[Route("[controller]")]
public class SampleService : ISampleService
{
    [HttpGet]
    public Task<string> EchoAsync(string value, CancellationToken cancellationToken)
    {
        var echo = new string(value.AsEnumerable().Reverse().ToArray());
        return Task.FromResult(echo);
    }
}
