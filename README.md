# KeenConveyance

## 1. Intro

A `Asp.Net Core Controller` service call framework based on `interface`. 基于 `interface` 的 `Asp.Net Core Controller` 服务调用框架。

 - 动态 `Controller` 选择. 支持选择指定类型注册为 `Controller` 并暴露 `HttpAPI`;
 - 服务端基于 `Asp.Net Core`	的 `Controller`. 支持所有标准的 `Controller` 功能, 如 `Filter`、`Authentication`、`Authorization`等(*);
 - 客户端基于 `Microsoft.Extensions.Http` 和 `SourceGenerator`. 无运行时动态代码生成, 支持 `Microsoft.Extensions.Http` 丰富的拓展 (`Polly`等);
 - 基于约定的客户端代码生成. 不依赖类库共享;
 - 目标框架 `net6.0`+;

### NOTE!!!
 - 虽然支持所有标准的 `Controller` 功能且执行其流程, 但框架并非按照 `Controller` 定义进行请求, 框架的请求中会忽略 `[FromQuery]`、`[FromRoute]` 等定义直接使用 `ModelBinder` 从 `Body` 进行绑定;
	- 即: 如果请求由框架发起, 则依赖原始 `HttpContext`	信息的功能可能不能正常工作, 例如请求的 `Query`、`Path` 中不包含 `Action` 的信息;
    
## 2. 快速开始

### 2.1 服务端
 - 新建 `Asp.Net Core Web API` 项目, 并引用包:
```xml
<ItemGroup>
  <PackageReference Include="KeenConveyance.AspNetCore" Version="1.0.0-*" />
</ItemGroup>
```

 - 新建 `IApplicationService` 接口用于标记服务, 以便后续筛选 (非强制要求)
```C#
public interface IApplicationService
{ }
```

 - 配置 `KeenConveyance` 服务
```C#
services.AddKeenConveyance()
        .ConfigureService(options =>
        {
            //筛选所有派生自 IApplicationService 的类型作为 Controller
            options.ControllerSelectPredicate = typeInfo => typeInfo.IsAssignableTo(typeof(IApplicationService));
        });
```

 - 在请求管道中启用 `KeenConveyance`
```C#
// 在 MapControllers 之前 UseAuthorization 之后
app.UseKeenConveyance();
```

 - 定义服务接口并实现服务 (或使现有 `Controller` 派生自服务接口), 如:
```C#
public interface ISampleService : IApplicationService
{
    public Task<string> EchoAsync(string value, CancellationToken cancellationToken);
}

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
```

-------

### 2.2 客户端
 - 新建任意类型项目 (需要 `DI` 支持, 可能需要手动引入), 并引用包:
```xml
<ItemGroup>
  <PackageReference Include="KeenConveyance" Version="1.0.0-*" />
</ItemGroup>
```
 Note: 项目直接引用而不要间接引用!

 - 配置 `KeenConveyance` 服务
```C#
services.AddKeenConveyance()
        .ConfigureClient(builder =>
        {
            //必须在此链式调用 AddClient 使用服务接口定义添加客户端, 并在最终调用 CompleteClientSetup 方法
            builder.AddClient<ISampleService>(httpClientBuilder =>
                   {
                       //配置服务地址
                       httpClientBuilder.ConfigureServiceAddress("http://127.0.0.1:5225");  //必须配置客户端的服务地址
                       // 在此进行 ISampleService 的 HttpClient 配置
                   })
                   .CompleteClientSetup();  //必须在最后调用 CompleteClientSetup 方法，提示未找到此方法可尝试手动using当前项目的根命名空间
        });
```

-------

至此已完成所有配置，从 `DI` 容器中直接获取 `ISampleService` 实例并调用方法即可

-------

## 其他

 - 客户端代码生成在当前项目的根命名空间下，提示未找到 `CompleteClientSetup` 方法可以手动 `using` 当前项目的根命名空间进行尝试;
 - 生成的客户端代码都已标记为 `partial` 类型, 可声明对应的 `partial` 类，并重写方法来进行自定义操作, 服务的代理类型为格式为 `{ServiceName}ProxyClient` (如 `ISampleServiceProxy` 的代理类型为 `ISampleServiceProxyClient`) :
   ```C#
    internal static partial class GeneratedClient
    {
        private partial class ISampleServiceProxyClient
        {
            public override Task<string> EchoAsync(string value, CancellationToken cancellationToken)
            {
                //进行请求前后的自定义操作
                return base.EchoAsync(value, cancellationToken);
            }
        }
    }
   ```