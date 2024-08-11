using Microsoft.CodeAnalysis;

namespace KeenConveyance.SourceGenerator;

internal enum TypeJsonKind
{
    None,

    Boolean,

    Number,

    String,

    Object,
}

[Flags]
internal enum ValueTypeKind
{
    Undefined = 0,

    Void = 1,

    String = 1 << 1,

    Task = 1 << 2,

    TaskT = 1 << 3 | Task,

    Nullable = 1 << 4,
}

internal class TypeAnalyzer
{
    #region Private 字段

#pragma warning disable RS1024 // 正确比较符号

    private readonly HashSet<ITypeSymbol> _booleanTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

    private readonly ITypeSymbol _nullableSymbol;

    private readonly HashSet<ITypeSymbol> _numberTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

    private readonly HashSet<ITypeSymbol> _stringTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

#pragma warning restore RS1024 // 正确比较符号

    #endregion Private 字段

    #region Public 属性

    public INamedTypeSymbol CancellationTokenSymbol { get; private set; }

    public INamedTypeSymbol StringSymbol { get; private set; }

    public INamedTypeSymbol TaskSymbol { get; private set; }

    public INamedTypeSymbol TaskTSymbol { get; private set; }

    public INamedTypeSymbol ValueTaskSymbol { get; private set; }

    public INamedTypeSymbol ValueTaskTSymbol { get; private set; }

    public INamedTypeSymbol VoidSymbol { get; private set; }

    #endregion Public 属性

    #region Public 构造函数

    public TypeAnalyzer(Compilation compilation)
    {
        _numberTypes.Add(compilation.GetSpecialType(SpecialType.System_Int16));
        _numberTypes.Add(compilation.GetSpecialType(SpecialType.System_Int32));
        _numberTypes.Add(compilation.GetSpecialType(SpecialType.System_Int64));

        _numberTypes.Add(compilation.GetSpecialType(SpecialType.System_UInt16));
        _numberTypes.Add(compilation.GetSpecialType(SpecialType.System_UInt32));
        _numberTypes.Add(compilation.GetSpecialType(SpecialType.System_UInt64));

        _numberTypes.Add(compilation.GetSpecialType(SpecialType.System_Single));
        _numberTypes.Add(compilation.GetSpecialType(SpecialType.System_Decimal));
        _numberTypes.Add(compilation.GetSpecialType(SpecialType.System_Double));

        _booleanTypes.Add(compilation.GetSpecialType(SpecialType.System_Boolean));

        _stringTypes.Add(compilation.GetSpecialType(SpecialType.System_String));
        _stringTypes.Add(compilation.GetTypeByMetadataName("System.Guid")!);
        _stringTypes.Add(compilation.GetTypeByMetadataName("System.DateTime")!);
        _stringTypes.Add(compilation.GetTypeByMetadataName("System.DateTimeOffset")!);

        _nullableSymbol = compilation.GetSpecialType(SpecialType.System_Nullable_T);

        TaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task") ?? throw new InvalidOperationException();
        TaskTSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1") ?? throw new InvalidOperationException();
        ValueTaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask") ?? throw new InvalidOperationException();
        ValueTaskTSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.ValueTask`1") ?? throw new InvalidOperationException();
        VoidSymbol = compilation.GetSpecialType(SpecialType.System_Void) ?? throw new InvalidOperationException();
        CancellationTokenSymbol = compilation.GetTypeByMetadataName("System.Threading.CancellationToken") ?? throw new InvalidOperationException();
        StringSymbol = compilation.GetSpecialType(SpecialType.System_String);
    }

    #endregion Public 构造函数

    #region Public 方法

    public TypeGenerateInfo CreateTypeGenerateInfo(ITypeSymbol typeSymbol)
    {
        ValueTypeKind valueTypeKind = ValueTypeKind.Undefined;
        bool isWarped = false;

        if (IsNullable(typeSymbol))
        {
            valueTypeKind |= ValueTypeKind.Nullable;
            isWarped = true;
        }
        if (IsTask(typeSymbol))
        {
            valueTypeKind |= ValueTypeKind.Task;
        }
        if (IsTaskT(typeSymbol))
        {
            valueTypeKind |= ValueTypeKind.TaskT;
            isWarped = true;
        }

        if (isWarped)
        {
            if (TryGetValueType(typeSymbol, out var valueTypeSymbol))
            {
                if (SymbolEqualityComparer.Default.Equals(StringSymbol, valueTypeSymbol))
                {
                    valueTypeKind |= ValueTypeKind.String;
                }
                return new TypeGenerateInfo(typeSymbol, valueTypeKind, valueTypeSymbol!);
            }
            else
            {
                throw new InvalidOperationException($"Can not get value type for {typeSymbol}");
            }
        }
        else
        {
            if (SymbolEqualityComparer.Default.Equals(VoidSymbol, typeSymbol)
                || valueTypeKind.HasFlag(ValueTypeKind.Task))
            {
                valueTypeKind |= ValueTypeKind.Void;
            }
            if (SymbolEqualityComparer.Default.Equals(StringSymbol, typeSymbol))
            {
                valueTypeKind |= ValueTypeKind.String;
            }
            return new TypeGenerateInfo(typeSymbol, valueTypeKind, typeSymbol);
        }
    }

    public TypeJsonKind GetJsonKind(ITypeSymbol typeSymbol)
    {
        if (_numberTypes.Contains(typeSymbol))
        {
            return TypeJsonKind.Number;
        }
        else if (_numberTypes.Contains(typeSymbol))
        {
            return TypeJsonKind.Number;
        }
        else if (_stringTypes.Contains(typeSymbol))
        {
            return TypeJsonKind.String;
        }
        return TypeJsonKind.Object;
    }

    public bool IsNullable(ITypeSymbol typeSymbol)
    {
        return SymbolEqualityComparer.Default.Equals(_nullableSymbol, typeSymbol.OriginalDefinition);
    }

    public bool IsTask(ITypeSymbol typeSymbol)
    {
        return SymbolEqualityComparer.Default.Equals(TaskSymbol, typeSymbol)
               || SymbolEqualityComparer.Default.Equals(ValueTaskSymbol, typeSymbol);
    }

    public bool IsTaskT(ITypeSymbol typeSymbol)
    {
        return SymbolEqualityComparer.Default.Equals(TaskTSymbol, typeSymbol.OriginalDefinition)
               || SymbolEqualityComparer.Default.Equals(ValueTaskTSymbol, typeSymbol.OriginalDefinition);
    }

    public bool TryGetValueType(ITypeSymbol typeSymbol, out ITypeSymbol? valueTypeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol
            && namedTypeSymbol.TypeArguments.Length == 1)
        {
            valueTypeSymbol = namedTypeSymbol.TypeArguments[0];
            return true;
        }
        valueTypeSymbol = null;
        return false;
    }

    #endregion Public 方法
}
