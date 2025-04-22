using System.Diagnostics;
using System.Text.Json;
using KeenConveyance.Serialization;

namespace KeenConveyance.Client;

/// <summary>
/// 接口以标识可以作为 <see cref="Activity"/> 的Tag
/// </summary>
public interface IActivityTagable
{
    #region Public 字段

    /// <summary>
    /// 空值
    /// </summary>
    public const string EmptyValue = "{}";

    #endregion Public 字段

    #region Private 字段

    private static JsonSerializerOptions s_tagValueJsonSerializerOptions;

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc cref="KeenConveyanceClientOptions.BufferInitialCapacity"/>
    public static int BufferInitialCapacity { get; set; } = KeenConveyanceClientOptions.DefaultBufferInitialCapacity;

    /// <summary>
    /// 用于<see cref="Activity"/> 的Tag值生成的json序列化配置
    /// </summary>
    public static JsonSerializerOptions TagValueJsonSerializerOptions
    {
        get => s_tagValueJsonSerializerOptions;
        set
        {
            s_tagValueJsonSerializerOptions = value ?? throw new ArgumentNullException(nameof(value));

            TagValueJsonObjectSerializer = new(value);
        }
    }

    #endregion Public 属性

    #region Internal 属性

    /// <summary>
    /// 用于<see cref="Activity"/> 的Tag值生成的json序列化器
    /// </summary>
    internal static SystemTextJsonObjectSerializer TagValueJsonObjectSerializer { get; private set; }

    #endregion Internal 属性

    #region Public 构造函数

#pragma warning disable CS8618
    /// <inheritdoc cref="IActivityTagable"/>
    static IActivityTagable()
    {
        TagValueJsonSerializerOptions = KeenConveyanceJsonOptions.DefaultJsonSerializerOptions;
    }
#pragma warning restore CS8618

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 转换为 <see cref="Activity"/> 使用的Tag值
    /// </summary>
    /// <returns></returns>
    public object? ToTagValue();

    #endregion Public 方法

}
