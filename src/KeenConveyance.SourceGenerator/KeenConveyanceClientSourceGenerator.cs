using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KeenConveyance.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class KeenConveyanceClientSourceGenerator : IIncrementalGenerator
{
    #region Public 属性

    public static SymbolDisplayFormat FullyQualifiedFormatWithOutGlobal { get; } = new SymbolDisplayFormat(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining,
                                                                                                           typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                                                                                                           genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                                                                                                           memberOptions: SymbolDisplayMemberOptions.None,
                                                                                                           delegateStyle: SymbolDisplayDelegateStyle.NameOnly,
                                                                                                           extensionMethodStyle: SymbolDisplayExtensionMethodStyle.Default,
                                                                                                           parameterOptions: SymbolDisplayParameterOptions.None,
                                                                                                           propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
                                                                                                           localOptions: SymbolDisplayLocalOptions.None,
                                                                                                           kindOptions: SymbolDisplayKindOptions.None,
                                                                                                           miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);

    #endregion Public 属性

    #region Public 方法

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var clientTypeProvider = context.SyntaxProvider.CreateSyntaxProvider(FilterAddClientInvocationSyntaxNode, TransformValidClientSyntaxNode)
                                                       .Where(m => m is not null);

        context.RegisterSourceOutput(clientTypeProvider.Collect().Combine(context.AnalyzerConfigOptionsProvider),
                                     (context, input) =>
                                     {
                                         var analyzerConfigOptionsProvider = input.Right;
                                         var rootNamespace = analyzerConfigOptionsProvider.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespaceValue) && !string.IsNullOrWhiteSpace(rootNamespaceValue)
                                                             ? rootNamespaceValue
                                                             : "KeenConveyance";

                                         var allAddedClientGenerateInfo = input.Left;
                                         var clientGenerateInfos = allAddedClientGenerateInfo.Distinct()
                                                                                             .OfType<ServiceGenerateInfo>()
                                                                                             .ToImmutableArray();
                                         if (clientGenerateInfos.Length == 0)
                                         {
                                             return;
                                         }
                                         var code = ClientGenerateUtil.GenerateClientCode(clientGenerateInfos, rootNamespace);

                                         context.AddSource("GeneratedClient.g.cs", code);
                                     });

        var invalidClientAddExpressionProvider = context.SyntaxProvider.CreateSyntaxProvider(FilterAddClientInvocationSyntaxNode, TransformInvalidClientSyntaxNode)
                                                                       .Where(m => m.ExpressionSyntax is not null);

        context.RegisterSourceOutput(invalidClientAddExpressionProvider.Collect(),
                             (context, invalidClientAddExpressions) =>
                             {
                                 foreach (var (invalidClientAddExpression, typeSymbol) in invalidClientAddExpressions)
                                 {
                                     var diagnosticDescriptor = new DiagnosticDescriptor(id: "KC001", title: "Generator", messageFormat: "不能为类型 {0} 生成 KeenConveyance 客户端，请使用正确的接口进行客户端声明。", category: "KeenConveyanceClientGenerator", defaultSeverity: DiagnosticSeverity.Error, isEnabledByDefault: true);
                                     context.ReportDiagnostic(Diagnostic.Create(descriptor: diagnosticDescriptor, location: invalidClientAddExpression.GetLocation(), messageArgs: new[] { typeSymbol }));
                                 }
                             });
    }

    #endregion Public 方法

    #region Private 方法

    #region Filter

    private static bool FilterAddClientInvocationSyntaxNode(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        if (syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax
            && invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax
            && string.Compare("AddClient", memberAccessExpressionSyntax.Name.Identifier.Text) == 0)
        {
            return true;
        }
        return false;
    }

    private (InvocationExpressionSyntax ExpressionSyntax, ITypeSymbol TypeSymbol) TransformInvalidClientSyntaxNode(GeneratorSyntaxContext generatorSyntaxContext, CancellationToken cancellationToken)
    {
        var invocationExpressionSyntax = (InvocationExpressionSyntax)generatorSyntaxContext.Node;

        if (generatorSyntaxContext.SemanticModel.GetSymbolInfo(invocationExpressionSyntax) is SymbolInfo methodSymbolInfo
            && methodSymbolInfo.Symbol is IMethodSymbol methodSymbol
            && methodSymbol.IsExtensionMethod
            && methodSymbol.TypeArguments.Length == 1
            && string.Compare("KeenConveyance", methodSymbol.ContainingNamespace.Name) == 0
            && methodSymbol.TypeArguments[0] is ITypeSymbol typeSymbol
            && typeSymbol.TypeKind != TypeKind.Interface)
        {
            return (invocationExpressionSyntax, typeSymbol);
        }
        return default;
    }

    private ServiceGenerateInfo? TransformValidClientSyntaxNode(GeneratorSyntaxContext generatorSyntaxContext, CancellationToken cancellationToken)
    {
        var invocationExpressionSyntax = (InvocationExpressionSyntax)generatorSyntaxContext.Node;

        if (generatorSyntaxContext.SemanticModel.GetSymbolInfo(invocationExpressionSyntax) is SymbolInfo methodSymbolInfo
            && methodSymbolInfo.Symbol is IMethodSymbol methodSymbol
            && methodSymbol.IsExtensionMethod
            && methodSymbol.TypeArguments.Length == 1
            && string.Compare("KeenConveyance", methodSymbol.ContainingNamespace.Name) == 0
            && methodSymbol.TypeArguments[0] is ITypeSymbol typeSymbol
            && typeSymbol.TypeKind == TypeKind.Interface)
        {
            var semanticModel = generatorSyntaxContext.SemanticModel;
            var typeJsonKindAnalyzer = new TypeAnalyzer(semanticModel.Compilation);

            var aliasAttributeTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName("KeenConveyance.AliasAttribute");
            var cancellationTokenTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");

            var typeAliasAttributeData = typeSymbol.GetAttributes().FirstOrDefault(m => SymbolEqualityComparer.Default.Equals(aliasAttributeTypeSymbol, m.AttributeClass));

            var methodInfos = typeSymbol.GetMembers()
                                        .Where(m => m.Kind == SymbolKind.Method)
                                        .OfType<IMethodSymbol>()
                                        .Select(method =>
                                        {
                                            var parameters = method.Parameters.Select(m => new ParameterGenerateInfo(m, m.Type.ToFullyQualifiedString(), m.Name, typeJsonKindAnalyzer.GetJsonKind(m.Type), typeJsonKindAnalyzer.CreateTypeGenerateInfo(m.Type)))
                                                                              .ToArray();

                                            //TODO 为出现多个 CancellationToken 的情况报告错误
                                            var cancellationParameter = method.Parameters.Reverse()
                                                                              .FirstOrDefault(m => SymbolEqualityComparer.Default.Equals(cancellationTokenTypeSymbol, m.Type));

                                            ParameterGenerateInfo? cancellationParameterInfo = cancellationParameter is null
                                                                                               ? null
                                                                                               : new ParameterGenerateInfo(cancellationParameter, cancellationParameter.Type.ToFullyQualifiedString(), cancellationParameter.Name, TypeJsonKind.None, typeJsonKindAnalyzer.CreateTypeGenerateInfo(cancellationParameter.Type));

                                            var methodAliasAttributeData = method.GetAttributes().FirstOrDefault(m => SymbolEqualityComparer.Default.Equals(aliasAttributeTypeSymbol, m.AttributeClass));

                                            return new MethodGenerateInfo(method, method.Name, methodAliasAttributeData?.ConstructorArguments[0].Value?.ToString() ?? method.Name, typeJsonKindAnalyzer.CreateTypeGenerateInfo(method.ReturnType), parameters, cancellationParameterInfo);
                                        })
                                        .ToArray();

            return new ServiceGenerateInfo(typeSymbol, typeSymbol.Name, typeAliasAttributeData?.ConstructorArguments[0].Value?.ToString() ?? typeSymbol.ToDisplayString(FullyQualifiedFormatWithOutGlobal), typeSymbol.ToFullyQualifiedString(), methodInfos);
        }
        return null;
    }

    #endregion Filter

    #endregion Private 方法
}
