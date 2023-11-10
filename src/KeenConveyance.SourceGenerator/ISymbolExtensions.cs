namespace Microsoft.CodeAnalysis;

internal static class ISymbolExtensions
{
    #region Public 方法

    public static string ToFullyQualifiedString(this ISymbol typeSymbol)
    {
        return typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    #endregion Public 方法
}
