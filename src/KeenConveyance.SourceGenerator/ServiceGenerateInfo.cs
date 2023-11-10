using Microsoft.CodeAnalysis;

namespace KeenConveyance.SourceGenerator;

record struct ServiceGenerateInfo(ITypeSymbol ServiceType, string Name, string Alias, string FullName, ComparableArray<MethodGenerateInfo> Methods);

record struct TypeGenerateInfo(ITypeSymbol Type, ValueTypeKind TypeKind, ITypeSymbol ValueType);
