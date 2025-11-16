#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

namespace Microsoft.CodeAnalysis;

internal static class ISymbolExtensions
{
    #region Private 字段

    private static readonly SymbolDisplayFormat s_symbolDisplayFormat = SymbolDisplayFormat.FullyQualifiedFormat.WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    #endregion Private 字段

    #region Public 方法

    public static string ToFullyQualifiedNullableString(this ISymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(s_symbolDisplayFormat);
    }

    public static string ToFullyQualifiedString(this ISymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    #endregion Public 方法
}
