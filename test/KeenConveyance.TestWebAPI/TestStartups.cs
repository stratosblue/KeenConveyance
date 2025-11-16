using KeenConveyance.TestWebAPI.MemoryPackSupportedTest;
using KeenConveyance.TestWebAPI.Services;
using Microsoft.AspNetCore.TestHost;

namespace KeenConveyance.TestWebAPI;

#region Base

public interface ITestStartup
{
    #region Public 方法

    WebApplication Build(params string[] args);

    #endregion Public 方法
}

public abstract class TestStartup : ITestStartup
{
    #region Public 属性

    public static bool NoTestServer { get; set; } = false;

    #endregion Public 属性

    #region Protected 方法

    protected virtual WebApplication ConfigureWebApplication(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
#if NET9_0_OR_GREATER
            app.MapOpenApi();
#endif
            app.MapSwaggerUI();
        }

        app.UseAuthorization();

        app.UseKeenConveyance();

        app.MapControllers();

        return app;
    }

    protected abstract WebApplicationBuilder ConfigureWebApplicationBuilder(WebApplicationBuilder builder);

    protected virtual WebApplicationBuilder CreateWebApplicationBuilder(params string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers()
                        .AddApplicationPart(typeof(ITestStartup).Assembly);

        builder.Services.AddEndpointsApiExplorer();

#if NET9_0_OR_GREATER
        builder.Services.AddOpenApi();
#endif

        builder.Services.AddKeenConveyance(options => options.ObjectSerializers.Add(new MemoryPackObjectSerializer()));

        if (!NoTestServer)
        {
            builder.WebHost.UseTestServer();
        }

        return builder;
    }

    #endregion Protected 方法

    #region Public 方法

    public virtual WebApplication Build(params string[] args)
    {
        var builder = CreateWebApplicationBuilder(args);
        builder = ConfigureWebApplicationBuilder(builder);
        var app = builder.Build();
        return ConfigureWebApplication(app);
    }

    #endregion Public 方法
}

#endregion Base

public class PathBaseTestStartup : TestStartup
{
    #region Protected 方法

    protected override WebApplicationBuilder ConfigureWebApplicationBuilder(WebApplicationBuilder builder)
    {
        builder.Services.AddKeenConveyance()
                        .ConfigureService(options =>
                        {
                            options.ControllerSelectPredicate = typeInfo => typeInfo.IsAssignableTo(typeof(IApplicationService));
                            options.UsePathHttpRequestEntryKeyGenerator();
                        });

        return builder;
    }

    #endregion Protected 方法
}

public class QueryBaseTestStartup : TestStartup
{
    #region Protected 方法

    protected override WebApplicationBuilder ConfigureWebApplicationBuilder(WebApplicationBuilder builder)
    {
        builder.Services.AddKeenConveyance()
                        .ConfigureService(options =>
                        {
                            options.ControllerSelectPredicate = typeInfo => typeInfo.IsAssignableTo(typeof(IApplicationService));
                            options.UseQueryHttpRequestEntryKeyGenerator();
                        });

        return builder;
    }

    #endregion Protected 方法
}
