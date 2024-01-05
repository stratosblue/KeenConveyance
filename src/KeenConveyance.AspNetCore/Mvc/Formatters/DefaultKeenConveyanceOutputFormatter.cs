using KeenConveyance.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;

namespace KeenConveyance.AspNetCore.Mvc.Formatters;

/// <summary>
/// KeenConveyance 默认使用的 <see cref="IOutputFormatter"/>
/// </summary>
public sealed class DefaultKeenConveyanceOutputFormatter : IOutputFormatter
{
    #region Public 方法

    /// <inheritdoc/>
    public bool CanWriteResult(OutputFormatterCanWriteContext context) => context.HttpContext.IsKeenConveyanceRequest();

    /// <inheritdoc/>
    public Task WriteAsync(OutputFormatterWriteContext context)
    {
        var httpContext = context.HttpContext;
        if (httpContext.Request.Headers.Accept is { } accept
            && accept.Count > 0
            && SpecificMediaType.TryParse(accept[0], out var acceptValue))
        {
            var objectSerializerSelector = httpContext.RequestServices.GetRequiredService<IObjectSerializerSelector>();
            var objectSerializer = objectSerializerSelector.Select(new(acceptValue))
                                   ?? throw new KeenConveyanceException($"No available object serializer for \"{acceptValue}\"");

            httpContext.Response.ContentType = objectSerializer.SupportedMediaType.MediaType;

            return objectSerializer.SerializeAsync(context.Object, context.ObjectType ?? typeof(object), httpContext.Response.Body, httpContext.RequestAborted);
        }
        throw new KeenConveyanceException($"Can not process with accept \"{accept}\"");
    }

    #endregion Public 方法
}
