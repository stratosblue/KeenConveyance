using System.ComponentModel;
using KeenConveyance;
using KeenConveyance.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
///
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class KeenConveyanceApplicationBuilderExtensions
{
    #region Public 方法

    /// <summary>
    /// 添加 KeenConveyance 处理中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseKeenConveyance(this IApplicationBuilder app)
    {
        var entryPath = app.ApplicationServices.GetRequiredService<IOptions<KeenConveyanceMvcOptions>>().Value.ServiceEntryPath;

        return app.Use((context, next) =>
        {
            if (context.Request.Path.StartsWithSegments(entryPath))
            {
                return ServeByKeenConveyanceAsync(context, next);
            }
            return next(context);
        });

        static async Task ServeByKeenConveyanceAsync(HttpContext context, RequestDelegate next)
        {
            var endpointProvider = context.RequestServices.GetRequiredService<IKeenConveyanceMvcEndpointMatcher>();

            if (await endpointProvider.MatchEndpointAsync(context).ConfigureAwait(false) is Endpoint endpoint)
            {
                context.Items.Add(KeenConveyanceConstants.VariantRequestMarkKey, null);

                context.SetEndpoint(endpoint);
            }

            await next(context).ConfigureAwait(false);
        }
    }

    #endregion Public 方法
}
