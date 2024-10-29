using Microsoft.CodeAnalysis;

namespace KeenConveyance.SourceGenerator;

internal record struct ServiceGenerateInfo(ITypeSymbol ServiceType, string Name, string Alias, string FullName, ComparableArray<MethodGenerateInfo> Methods, ITypeSymbol? ClientBaseType);

internal record struct TypeGenerateInfo(ITypeSymbol Type, ValueTypeKind TypeKind, ITypeSymbol ValueType);
