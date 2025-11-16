#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

namespace KeenConveyance;

/// <summary>
/// 非泛型的媒体类型
/// </summary>
public readonly struct SpecificMediaType : IEquatable<SpecificMediaType>
{
    #region Public 属性

    /// <summary>
    /// 媒体类型
    /// </summary>
    public string MediaType { get; }

    #endregion Public 属性

    #region Public 构造函数

    private SpecificMediaType(ReadOnlySpan<char> mediaType)
    {
        MediaType = mediaType.ToString();
    }

    /// <inheritdoc cref="SpecificMediaType"/>
    public SpecificMediaType(string mediaType)
    {
        ArgumentNullException.ThrowIfNull(mediaType);

        if (!TryParse(mediaType, out this))
        {
            throw new ArgumentException($"Incorrect media type format \"{mediaType}\"");
        }
    }

    #endregion Public 构造函数

    #region 静态方法

    /// <summary>
    /// 尝试从 <paramref name="mediaTypeString"/> 中获取媒体类型字符串
    /// </summary>
    /// <param name="mediaTypeString"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool TryGetTypeValue(ReadOnlySpan<char> mediaTypeString, out ReadOnlySpan<char> value)
    {
        var semicolonIndex = mediaTypeString.IndexOf(';');
        if (semicolonIndex > -1)
        {
            mediaTypeString = mediaTypeString.Slice(0, semicolonIndex);
        }

        if (mediaTypeString.Length == 0
            || mediaTypeString.IndexOfAny(" +*") > -1)
        {
            value = default;
            return false;
        }

        value = mediaTypeString;
        return true;
    }

    /// <summary>
    /// 尝试解析 <paramref name="mediaTypeString"/> 为 <see cref="SpecificMediaType"/>
    /// </summary>
    /// <param name="mediaTypeString"></param>
    /// <param name="specificMediaType"></param>
    /// <returns></returns>
    public static bool TryParse(ReadOnlySpan<char> mediaTypeString, out SpecificMediaType specificMediaType)
    {
        if (TryGetTypeValue(mediaTypeString, out var value))
        {
            var result = new char[value.Length];
            value.ToLowerInvariant(result);
            specificMediaType = new SpecificMediaType(result);
            return true;
        }
        specificMediaType = default;
        return false;
    }

    #endregion 静态方法

    #region Public 方法

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator SpecificMediaType(string value) => new(value);

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator string(SpecificMediaType value) => value.MediaType;

    /// <inheritdoc/>
    public static bool operator !=(SpecificMediaType left, SpecificMediaType right) => !(left == right);

    /// <inheritdoc/>
    public static bool operator ==(SpecificMediaType left, SpecificMediaType right) => left.Equals(right);

    /// <inheritdoc/>
    public bool Equals(SpecificMediaType other) => string.Equals(MediaType, other.MediaType, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is SpecificMediaType typedObject && Equals(typedObject);

    /// <inheritdoc/>
    public override int GetHashCode() => MediaType.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => MediaType;

    #endregion Public 方法
}
