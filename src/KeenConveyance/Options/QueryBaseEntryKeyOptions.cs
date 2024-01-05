namespace KeenConveyance;

/// <summary>
/// 基于 Query 的入口点设置选项
/// </summary>
public class QueryBaseEntryKeyOptions
{
    #region Private 字段

    private string _queryKey = DefaultQueryKey;

    #endregion Private 字段

    #region Public 字段

    /// <summary>
    /// 默认的 Query key
    /// </summary>
    public const string DefaultQueryKey = "__e";

    #endregion Public 字段

    #region Public 属性

    /// <summary>
    /// QueryKey 默认为 <see cref="DefaultQueryKey"/>
    /// </summary>
    public string QueryKey
    {
        get => _queryKey;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }
            _queryKey = value;
        }
    }

    #endregion Public 属性
}
