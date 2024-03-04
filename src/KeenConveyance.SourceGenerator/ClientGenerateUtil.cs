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
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Options;

using KeenConveyance;
using KeenConveyance.Client;
using KeenConveyance.ModelBinding;
using KeenConveyance.Serialization;

namespace {{rootNamespace}};

[EditorBrowsable(EditorBrowsableState.Never)]
internal static partial class GeneratedClient
{
    /// <summary>
    /// 完成对 KeenConveyance 客户端代理类型的加载 <br/>
    /// 包含：{{string.Join("、", clientGenerateInfos.Select(m => $"<see cref=\"{m.ServiceType.ToFullyQualifiedString()}\"/>"))}}
    /// </summary>
    public static void CompleteClientsSetup(this IKeenConveyanceClientBuilderGroupBuilder builder)
    {
""");
        foreach (var generateInfo in clientGenerateInfos)
        {
            builder.AppendLine($"        builder.ApplyClientImplementation<{generateInfo.ServiceType.ToFullyQualifiedString()}, {generateInfo.Name}ProxyClient>();");
        }
        builder.AppendLine("        builder.CompleteClientGroupSetup();");
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
            : base(CachedTypeNameAccessor<{{generateInfo.FullName}}>.DisplayName, httpClient, clientOptionsSnapshot, GeneratedMethodDescriptors)
        {
        }
""");

            (MethodGenerateInfo Method, string CreateExpresssion, string ParametersExpression)[] methodDescriptorCreateExpressionInfo = new (MethodGenerateInfo Method, string CreateExpresssion, string ParametersExpression)[generateInfo.Methods.Items.Length];

            for (int i = 0; i < generateInfo.Methods.Items.Length; i++)
            {
                builder.AppendLine();

                var methodGenerateInfo = generateInfo.Methods.Items[i];
                var parameters = methodGenerateInfo.Parameters.Items;
                var parameterString = string.Join(", ", parameters.Select(m => $"{m.TypeName} {m.Name}"));

                var cancellationTokenParameter = methodGenerateInfo.CancellationToken;
                var cancellationTokenAccessExpressionString = cancellationTokenParameter?.Name ?? "default";

                var noCancellationTokenParameters = parameters.Where(m => !SymbolEqualityComparer.Default.Equals(cancellationTokenParameter?.ParameterSymbol, m.ParameterSymbol))
                                                              .ToArray();

                var entryKey = GenerateEntryKey(generateInfo, methodGenerateInfo, noCancellationTokenParameters);

                var parameterDescriptorsCreateExpression = noCancellationTokenParameters.Length == 0
                                                           ? "Array.Empty<ParameterDescriptor>()"
                                                           : $"new ParameterDescriptor[]{{ {string.Join(", ", noCancellationTokenParameters.Select((parameter, index) => $"new({index}, \"{parameter.Name}\", typeof({parameter.ParameterSymbol.Type.ToFullyQualifiedString()}))"))} }}";

                methodDescriptorCreateExpressionInfo[i] = (methodGenerateInfo,
                                                           $"new MethodDescriptor(\"{entryKey}\", {parameterDescriptorsCreateExpression})",
                                                           parameterString);

                var createHttpContextString = "null;";
                var hasParameter = cancellationTokenParameter is null ? parameters.Length > 0 : parameters.Length > 1;
                if (hasParameter)
                {
                    var parametersAccessAllExpressionString = string.Join(", ", parameters.Select(m => m.Name));
                    createHttpContextString = $"PrePreparePayloadData ? CreateBufferHttpContent_{i}({parametersAccessAllExpressionString}) : new InternalRequestHttpContent_{i}({parametersAccessAllExpressionString}, ObjectSerializer);";
                }

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

                builder.AppendLine(
$$"""
        /// <inheritdoc/>
        public virtual {{methodGenerateInfo.MethodSymbol.ReturnType.ToFullyQualifiedString()}} {{methodGenerateInfo.Name}}({{parameterString}})
        {
            HttpContent? httpContent = {{createHttpContextString}}

            var executeTask = {{executeRequestString}}("{{entryKey}}", httpContent, {{cancellationTokenAccessExpressionString}});
            {{returnExpressionString}}
        }
""");

                if (noCancellationTokenParameters.Length > 0)
                {
                    builder.AppendLine();

                    #region CreateBufferHttpContent

                    builder.AppendLine(
$$"""
        private HttpContent? CreateBufferHttpContent_{{i}}({{parameterString}})
        {
            var bufferWriter = new PooledBufferWriter(BufferInitialCapacity);
            using var streamSerializer = ObjectSerializer.CreateObjectStreamSerializer(bufferWriter);
            streamSerializer.Start();
""");

                    foreach (var parameter in noCancellationTokenParameters)
                    {
                        builder.AppendLine($"            streamSerializer.Write({parameter.Name});");
                    }

                    builder.AppendLine(
$$"""
            streamSerializer.Finish();
            return new BufferHttpContent(ObjectSerializer.SupportedMediaType.MediaType, bufferWriter, {{cancellationTokenAccessExpressionString}});
        }

""");

                    #endregion CreateBufferHttpContent

                    builder.AppendLine(
$$"""
        private partial class InternalRequestHttpContent_{{i}} : MultipleObjectHttpContent
        {
""");
                    foreach (var parameter in parameters)
                    {
                        builder.AppendLine($"            private readonly {parameter.TypeName} _{parameter.Name};");
                    }

                    builder.AppendLine();

                    builder.AppendLine(
$$"""
            public InternalRequestHttpContent_{{i}}({{parameterString}}, IObjectSerializer objectSerializer)
                : base(objectSerializer, {{cancellationTokenParameter?.Name ?? "default"}})
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
            protected override async Task WriteContentAsync(IMultipleObjectAsyncStreamSerializer serializer)
            {
""");
                    foreach (var parameter in noCancellationTokenParameters)
                    {
                        builder.AppendLine($"                await serializer.WriteAsync(_{parameter.Name}, CancellationToken).ConfigureAwait(false);");
                    }

                    builder.AppendLine(
"""
            }
        }
""");
                }
            }

            builder.AppendLine(
$$"""

        protected static readonly MethodDescriptorCollection GeneratedMethodDescriptors = new MethodDescriptorCollection(new []
        {
{{string.Join(",\r\n", methodDescriptorCreateExpressionInfo.Select((m, index) => $"            // index[{index}] -> {m.Method.MethodSymbol.ToFullyQualifiedString()}({m.ParametersExpression})\r\n            {m.CreateExpresssion}"))}}
        });

""");

            builder.AppendLine("    }");
        }

        builder.AppendLine("}");

        return builder.ToString();
    }

    #endregion Public 方法

    private static string GenerateEntryKey(ServiceGenerateInfo generateInfo, MethodGenerateInfo methodGenerateInfo, ParameterGenerateInfo[] noCancellationTokenParameters)
    {
        if (noCancellationTokenParameters.Length > 0)
        {
            var parameterRawString = string.Join(", ", noCancellationTokenParameters.Select(m => GetTypeName(m.ParameterSymbol.Type)));
            var parameterSignature = URIEncoder.EncodeAsString(Crc32.Hash(Encoding.UTF8.GetBytes(parameterRawString)));
            return $"{generateInfo.Alias}:{methodGenerateInfo.Alias}@{parameterSignature}";

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
            return $"{generateInfo.Alias}:{methodGenerateInfo.Alias}";
        }
    }
}
