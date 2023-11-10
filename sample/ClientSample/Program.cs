using KeenConveyance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebAPISample.Services;
using ClientSample;

var services = new ServiceCollection();
services.AddLogging(builder => builder.AddConsole());
services.AddKeenConveyance()
        .ConfigureClient(builder =>
        {
            builder.AddClient<ISampleService>(httpClientBuilder =>
                   {
                       httpClientBuilder.ConfigureServiceAddress("http://127.0.0.1:5225");
                       //httpClientBuilder.ConfigurePathBaseHttpRequestMessageConstructor();
                   })
                   .CompleteClientSetup();
        });

await using var serviceProvider = services.BuildServiceProvider();

var sampleService = serviceProvider.GetRequiredService<ISampleService>();
var echo = await sampleService.EchoAsync("Hello", default);

Console.WriteLine($"Echo: {echo}");
