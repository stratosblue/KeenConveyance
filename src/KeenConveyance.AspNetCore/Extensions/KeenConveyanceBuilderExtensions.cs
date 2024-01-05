using System.ComponentModel;
using KeenConveyance;
using KeenConveyance.AspNetCore;
using KeenConveyance.AspNetCore.Mvc;
using KeenConveyance.AspNetCore.Mvc.Formatters;
using KeenConveyance.AspNetCore.Mvc.ModelBinding;
using KeenConveyance.Serialization;
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

        builder.Services.AddOptions<KeenConveyanceMvcOptions>();

        if (setupAction is not null)
        {
            setupAction.Invoke(options);
            builder.Services.Configure(setupAction);
        };

        var mvcBuilder = builder.Services.AddControllers();

        if (options.ControllerSelectPredicate is { } controllerPredicate)
        {
            mvcBuilder.ConfigureApplicationPartManager(manager =>
            {
                manager.FeatureProviders.Add(new DelegatingControllerFeatureProvider(controllerPredicate));
            });
        }

        builder.Services.TryAddSingleton<IObjectSerializerSelector, DefaultObjectSerializerSelector>();

        builder.Services.TryAddSingleton<IEndpointEntryKeyGenerator, DefaultEndpointEntryKeyGenerator>();
        builder.Services.Add(options.HttpRequestEntryKeyGeneratorServiceDescriptor);

        builder.Services.TryAddSingleton<IMvcEndpointMatcher, DefaultMvcEndpointMatcher>();

        builder.Services.Configure<MvcOptions>(options =>
        {
            if (!options.ModelBinderProviders.Any(m => m is KeenConveyanceModelBinderProvider))
            {
                options.ModelBinderProviders.Insert(0, new KeenConveyanceModelBinderProvider(options.ModelBinderProviders));
            }

            if (!options.OutputFormatters.Any(m => m is DefaultKeenConveyanceOutputFormatter))
            {
                options.OutputFormatters.Insert(0, new DefaultKeenConveyanceOutputFormatter());
            }
        });

        return builder;
    }

    #endregion Public 方法
}
