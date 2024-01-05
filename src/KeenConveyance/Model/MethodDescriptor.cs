using System.Collections.Immutable;

namespace KeenConveyance;

/// <summary>
/// 方法描述符
/// </summary>
/// <param name="entryKey">EntryKey</param>
/// <param name="parameters">方法参数列表</param>
public readonly struct MethodDescriptor(string entryKey, IEnumerable<ParameterDescriptor> parameters)
{
    #region Public 属性

    /// <summary>
    /// EntryKey
    /// </summary>
    public string EntryKey { get; } = entryKey;

    /// <summary>
    /// 方法参数列表
    /// </summary>
    public ImmutableArray<ParameterDescriptor> Parameters { get; } = parameters.ToImmutableArray();

    #endregion Public 属性
}
