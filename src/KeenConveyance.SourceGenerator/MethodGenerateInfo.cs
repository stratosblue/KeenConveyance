using Microsoft.CodeAnalysis;

namespace KeenConveyance.SourceGenerator;

record struct MethodGenerateInfo(IMethodSymbol MethodSymbol, string Name, string Alias, TypeGenerateInfo ReturnType, ComparableArray<ParameterGenerateInfo> Parameters, ParameterGenerateInfo? CancellationToken);
