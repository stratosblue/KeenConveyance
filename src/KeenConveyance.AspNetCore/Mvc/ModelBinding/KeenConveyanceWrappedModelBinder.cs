using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using KeenConveyance.ModelBinding;
using KeenConveyance.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using MvcActionDescriptor = Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor;

namespace KeenConveyance.AspNetCore.Mvc.ModelBinding;

internal class KeenConveyanceWrappedModelBinder(IModelBinder innerModelBinder,
                                                IObjectSerializerSelector objectSerializerSelector)
    : IModelBinder
{
    #region Public 字段

    public const string KeenConveyanceRequestParameterValueProviderKey = "___KeenConveyance_Request_Parameter_Value_Provider_Key";

    #endregion Public 字段

    #region Private 字段

    private readonly ConcurrentDictionary<MvcActionDescriptor, ImmutableArray<ParameterDescriptor>> _actionParametersCache = new();

    private readonly ConcurrentDictionary<MvcActionDescriptor, IReadOnlyList<Type>> _actionParameterTypesCache = new();

    private readonly IModelBinder _innerModelBinder = innerModelBinder ?? throw new ArgumentNullException(nameof(innerModelBinder));

    private readonly IObjectSerializerSelector _objectSerializerSelector = objectSerializerSelector ?? throw new ArgumentNullException(nameof(objectSerializerSelector));

    private readonly ConcurrentDictionary<ModelMetadata, ParameterDescriptor> _parameterDescriptorCache = new();

    #endregion Private 字段

    #region Private 方法

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueTask BindModelAsync(ModelBindingContext bindingContext, IParameterValueProvider parameterValueProvider)
    {
        var parameterDescriptor = GetParameterDescriptor(bindingContext);
        var parameterValueGetTask = parameterValueProvider.GetAsync(parameterDescriptor);
        return parameterValueGetTask.IsCompleted
               ? Bind(bindingContext, parameterValueGetTask.Result)
               : BindAsync(bindingContext, parameterValueGetTask);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ValueTask Bind(ModelBindingContext bindingContext, object? value)
        {
            bindingContext.Result = ModelBindingResult.Success(value);
            return ValueTask.CompletedTask;
        }

        static async ValueTask BindAsync(ModelBindingContext bindingContext, ValueTask<object?> parameterValueGetTask)
        {
            var value = await parameterValueGetTask;
            bindingContext.Result = ModelBindingResult.Success(value);
        }
    }

    private Task InternalBindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext.HttpContext.Items.TryGetValue(KeenConveyanceRequestParameterValueProviderKey, out var savedParameterValueProvider)
            && savedParameterValueProvider is IParameterValueProvider parameterValueProvider)
        {
            var bindModelTask = BindModelAsync(bindingContext, parameterValueProvider);
            return bindModelTask.IsCompletedSuccessfully
                   ? CheckShouldFallbackAsync(bindingContext, _innerModelBinder)
                   : AwaitBindAsync(bindModelTask, bindingContext, _innerModelBinder);
        }
        return ReadBodyAndBindModelAsync(bindingContext);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Task CheckShouldFallbackAsync(ModelBindingContext bindingContext, IModelBinder innerModelBinder)
        {
            return bindingContext.Result.IsModelSet ? Task.CompletedTask : innerModelBinder.BindModelAsync(bindingContext);
        }

        static async Task AwaitBindAsync(ValueTask bindTask, ModelBindingContext bindingContext, IModelBinder innerModelBinder)
        {
            await bindTask.ConfigureAwait(false);
            await CheckShouldFallbackAsync(bindingContext, innerModelBinder).ConfigureAwait(false);
        }
    }

    private async Task ReadBodyAndBindModelAsync(ModelBindingContext bindingContext)
    {
        var httpContext = bindingContext.HttpContext;
        var cancellationToken = bindingContext.HttpContext.RequestAborted;

        var parameterTypes = GetParameterTypes(bindingContext.ActionContext.ActionDescriptor);

        SpecificMediaType contentType = httpContext.Request.ContentType!;

        var objectSerializer = _objectSerializerSelector.Select(new ObjectSerializerSelectContext(contentType))
                               ?? throw new InvalidOperationException($"Unsupported input content type \"{httpContext.Request.ContentType}\"");

        var multipleObjectCollection = await objectSerializer.DeserializeMultipleAsync(parameterTypes, httpContext.Request.Body, cancellationToken).ConfigureAwait(false);

        var parameterValueProvider = DefaultParameterValueProvider.Create(multipleObjectCollection);

        bindingContext.HttpContext.Items.Add(KeenConveyanceRequestParameterValueProviderKey, parameterValueProvider);

        await BindModelAsync(bindingContext, parameterValueProvider).ConfigureAwait(false);
    }

    #endregion Private 方法

    #region Public 方法

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (!bindingContext.HttpContext.IsKeenConveyanceRequest())
        {
            return _innerModelBinder.BindModelAsync(bindingContext);
        }
        return InternalBindModelAsync(bindingContext);
    }

    #endregion Public 方法

    #region parameters

    private ParameterDescriptor GetParameterDescriptor(ModelBindingContext bindingContext)
    {
        // 通过 ModelMetadata 进行缓存，理论上效率最高，但不明确是否会有 ModelMetadata 变更的情况
        if (_parameterDescriptorCache.TryGetValue(bindingContext.ModelMetadata, out var cachedValue))
        {
            return cachedValue;
        }

        var parameterDescriptors = GetParameterDescriptors(bindingContext.ActionContext.ActionDescriptor);

        var parameterDescriptor = parameterDescriptors.First(m => string.Equals(m.ParameterName, bindingContext.ModelMetadata.Name));

        _parameterDescriptorCache.TryAdd(bindingContext.ModelMetadata, parameterDescriptor);

        return parameterDescriptor;
    }

    private ImmutableArray<ParameterDescriptor> GetParameterDescriptors(MvcActionDescriptor actionDescriptor)
    {
        // 通过 ActionDescriptor 进行缓存，理论上效率最高，但不明确是否会有 ActionDescriptor 变更的情况
        if (_actionParametersCache.TryGetValue(actionDescriptor, out var cachedValue))
        {
            return cachedValue;
        }

        // 排除 CancellationToken 重新编排新的索引，索引对应传输内容中的索引
        List<ParameterDescriptor> parameters = [];
        var index = 0;
        foreach (var descriptor in actionDescriptor.Parameters)
        {
            if (descriptor.ParameterType == typeof(CancellationToken))
            {
                continue;
            }
            parameters.Add(new ParameterDescriptor(index++, descriptor.Name, descriptor.ParameterType));
        }
        var result = parameters.ToImmutableArray();

        _actionParametersCache.TryAdd(actionDescriptor, result);

        return result;
    }

    private IReadOnlyList<Type> GetParameterTypes(MvcActionDescriptor actionDescriptor)
    {
        // 通过 ActionDescriptor 进行缓存，理论上效率最高，但不明确是否会有 ActionDescriptor 变更的情况
        if (_actionParameterTypesCache.TryGetValue(actionDescriptor, out var cachedValue))
        {
            return cachedValue;
        }

        var parameterDescriptors = GetParameterDescriptors(actionDescriptor);
        var result = parameterDescriptors.Select(m => m.ParameterType).ToImmutableList();

        _actionParameterTypesCache.TryAdd(actionDescriptor, result);

        return result;
    }

    #endregion parameters
}
