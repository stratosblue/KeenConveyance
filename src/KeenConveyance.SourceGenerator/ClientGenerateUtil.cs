﻿using System.Collections.Immutable;
using System.Text;
using KeenConveyance.Util;
using Microsoft.CodeAnalysis;

namespace KeenConveyance.SourceGenerator;

internal static class ClientGenerateUtil
{
    #region Public 属性

    public static SymbolDisplayFormat FullyQualifiedFormatWithOutGlobalAndSpecialTypes { get; } = new SymbolDisplayFormat(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
                                                                                                                          typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                                                                                                                          genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                                                                                                                          memberOptions: SymbolDisplayMemberOptions.None,
                                                                                                                          delegateStyle: SymbolDisplayDelegateStyle.NameOnly,
                                                                                                                          extensionMethodStyle: SymbolDisplayExtensionMethodStyle.Default,
                                                                                                                          parameterOptions: SymbolDisplayParameterOptions.None,
                                                                                                                          propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
                                                                                                                          localOptions: SymbolDisplayLocalOptions.None,
                                                                                                                          kindOptions: SymbolDisplayKindOptions.None,
                                                                                                                          miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    #endregion Public 属性

    #region Public 方法

    public static string GenerateClientCode(ImmutableArray<ServiceGenerateInfo> clientGenerateInfos, string rootNamespace)
    {
        var builder = new StringBuilder(1024_0);

        builder.AppendLine(
$$"""
// <Auto-Generated/>

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Options;

using KeenConveyance;

namespace {{rootNamespace}};

[EditorBrowsable(EditorBrowsableState.Never)]
internal static partial class GeneratedClient
{
    /// <summary>
    /// 完成对 KeenConveyance 客户端代理类型的加载 <br/>
    /// 包含：{{string.Join("、", clientGenerateInfos.Select(m => $"<see cref=\"{m.ServiceType.ToFullyQualifiedString()}\"/>"))}}
    /// </summary>
    public static void CompleteClientSetup<TClient>(this IKeenConveyanceClientBuilder<TClient> builder)
    {
""");
        foreach (var generateInfo in clientGenerateInfos)
        {
            builder.AppendLine($"        builder.ApplyClientImplementation<{generateInfo.ServiceType.ToFullyQualifiedString()}, {generateInfo.Name}ProxyClient>();");
        }
        builder.AppendLine("    }");
        builder.AppendLine();

        foreach (var generateInfo in clientGenerateInfos)
        {
            builder.AppendLine(
$$"""
    private sealed partial class {{generateInfo.Name}}ProxyClient : {{generateInfo.Name}}ProxyClientBase
    {
        public {{generateInfo.Name}}ProxyClient(HttpClient httpClient, IOptionsSnapshot<KeenConveyanceClientOptions> clientOptionsSnapshot)
            : base(httpClient, clientOptionsSnapshot)
        {
        }
    }
""");
            builder.AppendLine();
        }

        foreach (var generateInfo in clientGenerateInfos)
        {
            builder.AppendLine(
$$"""
    private abstract partial class {{generateInfo.Name}}ProxyClientBase : ProxyClientBase, {{generateInfo.FullName}}
    {
        public {{generateInfo.Name}}ProxyClientBase(HttpClient httpClient, IOptionsSnapshot<KeenConveyanceClientOptions> clientOptionsSnapshot)
            : base(CachedTypeNameAccessor<{{generateInfo.FullName}}>.DisplayName, httpClient, clientOptionsSnapshot)
        {
        }
""");

            for (int i = 0; i < generateInfo.Methods.Items.Length; i++)
            {
                builder.AppendLine();

                var methodGenerateInfo = generateInfo.Methods.Items[i];
                var parameters = methodGenerateInfo.Parameters.Items;
                var parameterString = string.Join(", ", parameters.Select(m => $"{m.TypeName} {m.Name}"));

                var cancellationTokenParameter = methodGenerateInfo.CancellationToken;

                var createHttpContextString = (cancellationTokenParameter is null ? parameters.Length > 0 : parameters.Length > 1)
                                              ? $"new InternalStreamJsonHttpContent_{i}({string.Join(", ", parameters.Select(m => m.Name))}, JsonSerializerOptions);"
                                              : "null;";

                var executeRequestString = "ExecuteRequestWithRawResultAsync";
                var returnExpressionString = "return executeTask;";

                if (methodGenerateInfo.ReturnType.TypeKind.HasFlag(ValueTypeKind.Void))
                {
                    executeRequestString = "ExecuteRequestAsync";
                }
                else if (methodGenerateInfo.ReturnType.TypeKind.HasFlag(ValueTypeKind.String))
                {
                    executeRequestString = "ExecuteRequestWithRawResultAsync";
                }
                else if (methodGenerateInfo.ReturnType.TypeKind.HasFlag(ValueTypeKind.Nullable))
                {
                    executeRequestString = $"ExecuteRequestAsync<{methodGenerateInfo.ReturnType.Type.ToFullyQualifiedString()}>";
                }
                else
                {
                    executeRequestString = $"ExecuteRequestAsync<{methodGenerateInfo.ReturnType.ValueType.ToFullyQualifiedString()}>";
                }

                if (!methodGenerateInfo.ReturnType.TypeKind.HasFlag(ValueTypeKind.Task))
                {
                    if (methodGenerateInfo.ReturnType.TypeKind.HasFlag(ValueTypeKind.Void))
                    {
                        returnExpressionString = "executeTask.ConfigureAwait(false).GetAwaiter().GetResult();";
                    }
                    else
                    {
                        returnExpressionString = "return executeTask.ConfigureAwait(false).GetAwaiter().GetResult();";
                    }
                }

                string entryKey;
                if (parameters.Length > 0)
                {
                    var parameterRawString = string.Join(", ", parameters.Select(m => GetTypeName(m.ParameterSymbol.Type)));
                    var parameterSignature = URIEncoder.EncodeAsString(Crc32.Hash(Encoding.UTF8.GetBytes(parameterRawString)));
                    entryKey = $"{generateInfo.Alias}:{methodGenerateInfo.Alias}@{parameterSignature}";

                    static string GetTypeName(ITypeSymbol typeSymbol)
                    {
                        if (typeSymbol is INamedTypeSymbol namedTypeSymbol
                            && namedTypeSymbol.IsGenericType)
                        {
                            var fullName = namedTypeSymbol.ConstructedFrom.ToDisplayString(FullyQualifiedFormatWithOutGlobalAndSpecialTypes);
                            fullName = fullName.Substring(0, fullName.IndexOf('<'));
                            return $"{fullName}<{string.Join(", ", namedTypeSymbol.TypeArguments.Select(GetTypeName))}>";
                        }
                        return typeSymbol.ToDisplayString(FullyQualifiedFormatWithOutGlobalAndSpecialTypes);
                    }
                }
                else
                {
                    entryKey = $"{generateInfo.Alias}:{methodGenerateInfo.Alias}";
                }

                builder.AppendLine(
$$"""
        /// <inheritdoc/>
        public virtual {{methodGenerateInfo.MethodSymbol.ReturnType.ToFullyQualifiedString()}} {{methodGenerateInfo.Name}}({{parameterString}})
        {
            HttpContent? httpContent = {{createHttpContextString}}
            var executeTask = {{executeRequestString}}("{{entryKey}}", httpContent, {{cancellationTokenParameter?.Name ?? "default"}});
            {{returnExpressionString}}
        }
""");
                parameters = parameters.Where(m => !SymbolEqualityComparer.Default.Equals(cancellationTokenParameter?.ParameterSymbol, m.ParameterSymbol))
                                       .ToArray();

                if (parameters.Length > 0)
                {
                    builder.AppendLine();

                    builder.AppendLine(
$$"""
        private partial class InternalStreamJsonHttpContent_{{i}} : StreamJsonHttpContent
        {
""");
                    foreach (var parameter in parameters)
                    {
                        builder.AppendLine($"            private readonly {parameter.TypeName} _{parameter.Name};");
                    }

                    builder.AppendLine();

                    builder.AppendLine(
$$"""
            public InternalStreamJsonHttpContent_{{i}}({{parameterString}}, JsonSerializerOptions jsonSerializerOptions)
                : base(jsonSerializerOptions, {{cancellationTokenParameter?.Name ?? "default"}})
            {
""");

                    foreach (var parameter in parameters)
                    {
                        builder.AppendLine($"                _{parameter.Name} = {parameter.Name};");
                    }

                    builder.AppendLine(
"""
            }

            /// <inheritdoc/>
            protected override ValueTask WriteContentAsync(Utf8JsonWriter jsonWriter)
            {
""");
                    foreach (var parameter in parameters)
                    {
                        switch (parameter.TypeJsonKind)
                        {
                            case TypeJsonKind.Boolean:
                                builder.AppendLine($"                jsonWriter.WriteBoolean(\"{parameter.Name}\", _{parameter.Name});");
                                break;

                            case TypeJsonKind.Number:
                                builder.AppendLine($"                jsonWriter.WriteNumber(\"{parameter.Name}\", _{parameter.Name});");
                                break;

                            case TypeJsonKind.String:
                                builder.AppendLine($"                jsonWriter.WriteString(\"{parameter.Name}\", _{parameter.Name});");
                                break;

                            case TypeJsonKind.Object:
                                builder.AppendLine($"""
                                                                         jsonWriter.WritePropertyName("{parameter.Name}");
                                                                         JsonSerializer.Serialize(jsonWriter,  _{parameter.Name}, JsonSerializerOptions);
                                                         """);

                                break;

                            case TypeJsonKind.None:
                            default:
                                break;
                        }
                    }

                    builder.AppendLine(
"""
                return default;
            }
        }
""");
                }
            }
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");

        return builder.ToString();
    }

    #endregion Public 方法
}
