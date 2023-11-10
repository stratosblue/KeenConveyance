using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;

namespace KeenConveyance.AspNetCore;

internal sealed class DefaultKeenConveyanceModelBinder : IKeenConveyanceModelBinder
{
    private record struct ParameterNameCacheInfo(string? ParameterName, bool HasFound);

    #region Private 字段

    private readonly JsonDocumentOptions _jsonDocumentOptions;

    private readonly JsonSerializerOptions _jsonSerializerOptions;

    private readonly ConcurrentDictionary<ModelMetadata, ParameterNameCacheInfo> _parameterNameCache = new();

    #endregion Private 字段

    #region Public 字段

    public const string RequestParameterJsonDocumentKey = "___KeenConveyance_Request_Parameter_Json_Document";

    #endregion Public 字段

    #region Public 构造函数

    public DefaultKeenConveyanceModelBinder(IOptions<JsonOptions> jsonOptions)
    {
        _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;

        _jsonDocumentOptions = new JsonDocumentOptions()
        {
            AllowTrailingCommas = _jsonSerializerOptions.AllowTrailingCommas,
            CommentHandling = JsonCommentHandling.Skip,
            MaxDepth = _jsonSerializerOptions.MaxDepth
        };
    }

    #endregion Public 构造函数

    #region Private 方法

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BindModel(ModelBindingContext bindingContext, JsonDocument jsonDocument)
    {
        if (TryGetInterfaceParameterName(bindingContext, out var parameterName)
            && jsonDocument.RootElement.TryGetProperty(parameterName, out var jsonElement))
        {
            bindingContext.Result = ModelBindingResult.Success(jsonElement.Deserialize(bindingContext.ModelType, _jsonSerializerOptions));
        }
    }

    private async Task ReadBodyAndBindModelAsync(ModelBindingContext bindingContext)
    {
        var jsonDocument = await JsonDocument.ParseAsync(utf8Json: bindingContext.HttpContext.Request.Body,
                                                         options: _jsonDocumentOptions,
                                                         cancellationToken: bindingContext.HttpContext.RequestAborted).ConfigureAwait(false);

        bindingContext.HttpContext.Items.Add(RequestParameterJsonDocumentKey, jsonDocument);

        BindModel(bindingContext, jsonDocument);
    }

    private bool TryGetInterfaceParameterName(ModelBindingContext bindingContext, [NotNullWhen(true)] out string? parameterName)
    {
        // 通过 ModelMetadata 进行缓存，理论上效率最高，但不明确是否会有 ModelMetadata 变更的情况
        if (_parameterNameCache.TryGetValue(bindingContext.ModelMetadata, out var cachedValue))
        {
            parameterName = cachedValue.ParameterName;
            return cachedValue.HasFound;
        }

        parameterName = GetInterfaceParameterName(bindingContext);
        _parameterNameCache.TryAdd(bindingContext.ModelMetadata, new(parameterName, parameterName is not null));

        return parameterName is not null;

        static string? GetInterfaceParameterName(ModelBindingContext bindingContext)
        {
            //从接口方法定义中获取参数名称，避免服务端实现接口时修改了参数名，导致无法进行绑定
            if (bindingContext.ActionContext.ActionDescriptor is ControllerActionDescriptor actionDescriptor
                && actionDescriptor.MethodInfo is MethodInfo actionMethodInfo
                && actionMethodInfo.DeclaringType is Type declaringType)
            {
                // 判断当前的方法是来自接口
                foreach (var declaringTypeInterface in declaringType.GetInterfaces())
                {
                    var interfaceMapping = declaringType.GetInterfaceMap(declaringTypeInterface);
                    var interfaceMethodIndex = Array.IndexOf(interfaceMapping.TargetMethods, actionMethodInfo);
                    if (interfaceMethodIndex != -1)
                    {
                        //查找参数位置
                        var parameterIndex = Array.FindIndex(actionMethodInfo.GetParameters(), 0, m => m.Name == bindingContext.ModelMetadata.Name);
                        if (parameterIndex != -1)
                        {
                            return interfaceMapping.InterfaceMethods[interfaceMethodIndex].GetParameters()[parameterIndex].Name;
                        }
                    }
                }
            }
            return null;
        }
    }

    #endregion Private 方法

    #region Public 方法

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext.HttpContext.Items.TryGetValue(RequestParameterJsonDocumentKey, out var savedRequestParameterJsonDocumentObject)
            && savedRequestParameterJsonDocumentObject is JsonDocument savedRequestParameterJsonDocument)
        {
            BindModel(bindingContext, savedRequestParameterJsonDocument);
            return Task.CompletedTask;
        }
        return ReadBodyAndBindModelAsync(bindingContext);
    }

    #endregion Public 方法
}
