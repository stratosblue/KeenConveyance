#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using System.Buffers;
using KeenConveyance.ModelBinding;

namespace KeenConveyance.Serialization;

/// <summary>
/// 默认的 <see cref="IMultipleObjectCollection"/>
/// </summary>
public sealed class DefaultMultipleObjectCollection : IMultipleObjectCollection, IItemDirectSetter, IParameterValueProvider
{
    #region Private 字段

    private object?[] _container;

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc/>
    public int Count { get; }

    #endregion Public 属性

    #region Public 索引器

    /// <inheritdoc/>
    public object? this[int index]
    {
        get
        {
            ThrowIfDisposed();
            if (index <= Count)
            {
                return _container[index];
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        internal set => SetItem(index, value);
    }

    /// <inheritdoc/>
    object? IItemDirectSetter.this[int index]
    {
        set => SetItem(index, value);
    }

    #endregion Public 索引器

    #region Public 构造函数

    private DefaultMultipleObjectCollection(int count)
    {
        Count = count;
        _container = ArrayPool<object?>.Shared.Rent(count);
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 创建一个容量为 <paramref name="count"/> 的 <see cref="DefaultMultipleObjectCollection"/>
    /// </summary>
    /// <param name="count"></param>
    /// <param name="itemDirectSetter">用于直接设置元素的设置器</param>
    /// <returns></returns>
    public static DefaultMultipleObjectCollection Create(int count, out IItemDirectSetter itemDirectSetter)
    {
        var result = new DefaultMultipleObjectCollection(count);
        itemDirectSetter = result;
        return result;
    }

    /// <inheritdoc/>
    public ValueTask<object?> GetAsync(ParameterDescriptor parameter)
    {
        ThrowIfDisposed();

        var value = this[parameter.ParameterIndex];
        return ValueTask.FromResult(value);
    }

    #endregion Public 方法

    #region Private 方法

    private void SetItem(int index, object? value)
    {
        ThrowIfDisposed();

        if (index < Count)
        {
            _container[index] = value;
            return;
        }
        throw new ArgumentOutOfRangeException(nameof(index));
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
    }

    #endregion Private 方法

    #region IDisposable

    private bool _isDisposed;

    /// <summary>
    ///
    /// </summary>
    ~DefaultMultipleObjectCollection()
    {
        Dispose();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_isDisposed)
        {
            _isDisposed = true;
            ArrayPool<object?>.Shared.Return(_container, false);
            _container = null!;
            GC.SuppressFinalize(this);
        }
    }

    #endregion IDisposable
}
