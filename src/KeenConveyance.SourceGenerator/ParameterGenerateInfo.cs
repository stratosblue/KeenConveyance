using Microsoft.CodeAnalysis;

namespace KeenConveyance.SourceGenerator;

record struct ParameterGenerateInfo(IParameterSymbol ParameterSymbol, string TypeName, string Name, TypeJsonKind TypeJsonKind, TypeGenerateInfo GenerateInfo);
