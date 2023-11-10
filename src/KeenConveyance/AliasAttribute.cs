namespace KeenConveyance;

/// <summary>
/// 为接口或方法标记别名
/// </summary>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public class AliasAttribute : Attribute
{
    #region Public 属性

    /// <summary>
    /// 别名
    /// </summary>
    public string Name { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="AliasAttribute"/>
    public AliasAttribute(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException($"“{nameof(name)}”不能为 null 或空白。", nameof(name));
        }

        Name = name;
    }

    #endregion Public 构造函数
}
