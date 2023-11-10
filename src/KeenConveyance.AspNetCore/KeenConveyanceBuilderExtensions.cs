using System.ComponentModel;
using KeenConveyance;
using KeenConveyance.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class KeenConveyanceBuilderExtensions
{
    #region Public 方法

    /// <summary>
    /// 配置 KeenConveyance 服务端
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="setupAction"></param>
    /// <returns></returns>
    public static IKeenConveyanceBuilder ConfigureService(this IKeenConveyanceBuilder builder, Action<KeenConveyanceMvcOptions>? setupAction = null)
    {
        var options = new KeenConveyanceMvcOptions();

        setupAction?.Invoke(options);

        var mvcBuilder = builder.Services.AddControllers();

        if (options.ControllerSelectPredicate is { } controllerPredicate)
        {
            mvcBuilder.ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new DelegatingControllerFeatureProvider(controllerPredicate));
            });
        }

        builder.Services.TryAddSingleton<IKeenConveyanceModelBinder, DefaultKeenConveyanceModelBinder>();

        builder.Services.TryAddSingleton<IEndpointEntryKeyGenerator, DefaultEndpointEntryKeyGenerator>();
        builder.Services.Add(options.HttpRequestEntryKeyGeneratorServiceDescriptor);

        builder.Services.TryAddSingleton<IKeenConveyanceMvcEndpointMatcher, DefaultKeenConveyanceMvcEndpointMatcher>();

        builder.Services.Configure<MvcOptions>(options =>
        {
            if (options.ModelBinderProviders.Any(m => m is DefaultKeenConveyanceMvcModelBinderProvider))
            {
                return;
            }
            options.ModelBinderProviders.Insert(0, new DefaultKeenConveyanceMvcModelBinderProvider(options.ModelBinderProviders));
        });

        return builder;
    }

    #endregion Public 方法
}
