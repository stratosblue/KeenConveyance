namespace KeenConveyance.SourceGenerator;

internal readonly struct ComparableArray<T> : IEquatable<ComparableArray<T>>
{
    #region Public 属性

    public T[] Items { get; }

    #endregion Public 属性

    #region Public 构造函数

    public ComparableArray(T[] items)
    {
        Items = items;
    }

    #endregion Public 构造函数

    #region Public 方法

    public static implicit operator ComparableArray<T>(T[] value)
    {
        return new ComparableArray<T>(value);
    }

    public static bool operator !=(ComparableArray<T> left, ComparableArray<T> right)
    {
        return !(left == right);
    }

    public static bool operator ==(ComparableArray<T> left, ComparableArray<T> right)
    {
        return left.Equals(right);
    }

    public bool Equals(ComparableArray<T> other) => Items.SequenceEqual(other.Items);

    public override bool Equals(object obj)
    {
        return obj is ComparableArray<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = -604923257;
        foreach (var item in Items)
        {
            hashCode += item!.GetHashCode();
        }
        return hashCode;
    }

    #endregion Public 方法
}
