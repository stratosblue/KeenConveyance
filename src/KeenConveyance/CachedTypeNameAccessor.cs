using Microsoft.Extensions.Internal;

namespace KeenConveyance;

/// <summary>
/// 缓存的类型名称访问器
/// </summary>
/// <typeparam name="T"></typeparam>
public static class CachedTypeNameAccessor<T>
{
    #region Public 属性

    /// <summary>
    /// 类型的展示名称
    /// </summary>
    public static string DisplayName => DisplayNameAccessor.Name;

    /// <summary>
    /// 类型的完整展示名称
    /// </summary>
    public static string FullDisplayName => FullDisplayNameAccessor.Name;

    #endregion Public 属性

    #region Private 类

    private static class DisplayNameAccessor
    {
        #region Public 属性

        public static string Name { get; } = TypeNameHelper.GetTypeDisplayName(typeof(T), fullName: false);

        #endregion Public 属性
    }

    private static class FullDisplayNameAccessor
    {
        #region Public 属性

        public static string Name { get; } = TypeNameHelper.GetTypeDisplayName(typeof(T), fullName: true);

        #endregion Public 属性
    }

    #endregion Private 类
}
