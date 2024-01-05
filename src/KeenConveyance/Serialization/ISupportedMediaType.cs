namespace KeenConveyance.Serialization;

/// <summary>
/// 声明支持媒体类型接口
/// </summary>
public interface ISupportedMediaType
{
    #region Public 属性

    /// <summary>
    /// 支持的媒体类型
    /// </summary>
    SpecificMediaType SupportedMediaType { get; }

    #endregion Public 属性
}
