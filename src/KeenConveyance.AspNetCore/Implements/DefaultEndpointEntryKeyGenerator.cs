using System.Reflection;
using System.Text;
using KeenConveyance.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;

namespace KeenConveyance.AspNetCore;

internal sealed class DefaultEndpointEntryKeyGenerator : IEndpointEntryKeyGenerator
{
    #region Private 字段

    private readonly Predicate<Endpoint> _endpointSelectPredicate;

    #endregion Private 字段

    #region Public 构造函数

    public DefaultEndpointEntryKeyGenerator(IOptions<KeenConveyanceMvcOptions> optionsAccessor)
    {
        _endpointSelectPredicate = optionsAccessor.Value.EndpointSelectPredicate ?? (static _ => true);
    }

    #endregion Public 构造函数

    #region Public 方法

    public string? GenerateKey(Endpoint endpoint)
    {
        if (_endpointSelectPredicate(endpoint)
            && endpoint.Metadata.OfType<ControllerActionDescriptor>().FirstOrDefault() is ControllerActionDescriptor actionDescriptor
            && actionDescriptor.MethodInfo is MethodInfo actionMethodInfo
            && actionMethodInfo.DeclaringType is Type declaringType)
        {
            // 判断当前的方法是来自接口
            foreach (var declaringTypeInterface in declaringType.GetInterfaces())
            {
                var interfaceMapping = declaringType.GetInterfaceMap(declaringTypeInterface);
                if (Array.IndexOf(interfaceMapping.TargetMethods, actionMethodInfo) != -1)
                {
                    var typeAliasAttribute = declaringTypeInterface.GetCustomAttribute<AliasAttribute>();
                    var parameters = actionMethodInfo.GetParameters();
                    var actionMethodRawInfo = declaringTypeInterface.GetMethod(actionMethodInfo.Name, parameters.Select(static m => m.ParameterType).ToArray());
                    var methodAliasAttribute = actionMethodRawInfo?.GetCustomAttribute<AliasAttribute>();

                    var parameterString = string.Join(", ", parameters.Select(m => GetTypeName(m.ParameterType)));
                    if (string.IsNullOrWhiteSpace(parameterString))
                    {
                        return $"{typeAliasAttribute?.Name ?? declaringTypeInterface.FullName}:{methodAliasAttribute?.Name ?? actionMethodInfo.Name}";
                    }
                    else
                    {
                        var crc32 = URIEncoder.EncodeAsString(Crc32.Hash(Encoding.UTF8.GetBytes(parameterString)));
                        return $"{typeAliasAttribute?.Name ?? declaringTypeInterface.FullName}:{methodAliasAttribute?.Name ?? actionMethodInfo.Name}@{crc32}";
                    }
                }
            }
        }
        return null;
    }

    #endregion Public 方法

    #region Private 方法

    private string GetTypeName(Type type)
    {
        if (type.IsGenericType)
        {
            var fullName = type.GetGenericTypeDefinition().FullName!;
            fullName = fullName.Substring(0, fullName.IndexOf('`'));
            return $"{fullName}<{string.Join(", ", type.GenericTypeArguments.Select(GetTypeName))}>";
        }
        return type.FullName!;
    }

    #endregion Private 方法
}
