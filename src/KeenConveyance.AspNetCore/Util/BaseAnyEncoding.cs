#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配

using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

#if NET8_0_OR_GREATER

using System.Collections.Frozen;

#endif

namespace System.Buffers;

/// <summary>
/// Base <typeparamref name="T"/> 序列编码器
/// </summary>
/// <typeparam name="T"></typeparam>
internal interface IBaseAnyEncoder<T> : IDisposable where T : notnull, IEquatable<T>
{
    #region Public 方法

    /// <summary>
    /// 解码码 <typeparamref name="T"/> 序列 <paramref name="source"/> 到 <see cref="byte"/> 序列
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    PooledData<byte> Decode(ReadOnlySpan<T> source);

    /// <summary>
    /// 编码 <see cref="byte"/> 序列 <paramref name="source"/> 到 <typeparamref name="T"/> 序列
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    PooledData<T> Encode(ReadOnlySpan<byte> source);

    #endregion Public 方法
}

/// <summary>
/// 池化的 <typeparamref name="T"/> 数据，需要 Dispose（建议立即 Dispose ，当心值类型传递导致的多次 Dispose）
/// </summary>
/// <typeparam name="T"></typeparam>
internal ref struct PooledData<T>
{
    #region Private 字段

    private T[] _pooledValue;

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 有效数据
    /// </summary>
    internal ReadOnlySpan<T> Span { get; }

    #endregion Public 属性

    #region Internal 构造函数

    internal PooledData(T[] pooledValue, ReadOnlySpan<T> values)
    {
        _pooledValue = pooledValue;
        Span = values;
    }

    #endregion Internal 构造函数

    #region Public 方法

    /// <inheritdoc cref="IDisposable.Dispose"/>
    internal void Dispose()
    {
        if (_pooledValue is { } pooledValue)
        {
            _pooledValue = null!;
            ArrayPool<T>.Shared.Return(pooledValue);
        }
    }

    #endregion Public 方法
}

/// <summary>
/// 基于任意序列的编码器
/// </summary>
internal static class BaseAnyEncoding
{
    #region Public 方法

    /// <summary>
    /// 使用 <paramref name="baseValues"/> 作为基础表创建编码器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="baseValues"></param>
    /// <returns></returns>
    internal static IBaseAnyEncoder<T> CreateEncoder<T>(params T[] baseValues) where T : notnull, IEquatable<T>
    {
        return CreateEncoder<T>(baseValues.AsSpan());
    }

    /// <summary>
    /// 使用 <paramref name="baseValues"/> 作为基础表创建编码器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="baseValues"></param>
    /// <returns></returns>
    internal static IBaseAnyEncoder<T> CreateEncoder<T>(ReadOnlySpan<T> baseValues) where T : notnull, IEquatable<T>
    {
        ThrowIfBaseValuesLengthOutOfRange(baseValues.Length);

        return CanProcessWithBit(baseValues.Length)
               ? new DefaultBitBaseAnyEncoder<T>(baseValues)
               : new DefaultDivRemBaseAnyEncoder<T>(baseValues);
    }

    /// <summary>
    /// 使用 <paramref name="baseValues"/> 作为基础表解码 <paramref name="source"/> 到 <see cref="byte"/> 序列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="baseValues"></param>
    /// <returns></returns>
    internal static PooledData<byte> Decode<T>(ReadOnlySpan<T> source, ReadOnlySpan<T> baseValues) where T : notnull
    {
        ThrowIfBaseValuesLengthOutOfRange(baseValues.Length);

        if (source.IsEmpty)
        {
            return default;
        }

        var dictionary = CreateBaseValueDictionary(baseValues);

        var sourceIndexesBuffer = ArrayPool<int>.Shared.Rent(source.Length);

        try
        {
            var sourceIndexes = sourceIndexesBuffer.AsSpan(0, source.Length);
            CreateSourceIndexes(source, sourceIndexes, dictionary);

            return InternalGeneralBaseDecode(sourceIndexes, baseValues.Length);
        }
        finally
        {
            ArrayPool<int>.Shared.Return(sourceIndexesBuffer, false);
        }
    }

    /// <summary>
    /// 使用 <paramref name="baseValues"/> 作为基础表编码 <paramref name="source"/> 到 <typeparamref name="T"/> 序列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="baseValues"></param>
    /// <returns></returns>
    internal static PooledData<T> Encode<T>(ReadOnlySpan<byte> source, ReadOnlySpan<T> baseValues) where T : notnull
    {
        ThrowIfBaseValuesLengthOutOfRange(baseValues.Length);

        return InternalGeneralBaseEncode(source, baseValues);
    }

    #region string

    /// <summary>
    /// 使用 <paramref name="baseValues"/> 作为基础表解码 <paramref name="source"/> 到字符串
    /// </summary>
    /// <param name="source"></param>
    /// <param name="baseValues"></param>
    /// <param name="encoding">编码，默认为 <see cref="Encoding.UTF8"/></param>
    /// <returns></returns>
    internal static string DecodeToString(ReadOnlySpan<char> source, ReadOnlySpan<char> baseValues, Encoding? encoding = null)
    {
        using var pooledData = Decode(source, baseValues);
        return pooledData.ToDisplayString(encoding);
    }

    /// <summary>
    /// 使用 <paramref name="baseValues"/> 作为基础表编码 <paramref name="source"/> 到字符串
    /// </summary>
    /// <param name="source"></param>
    /// <param name="baseValues"></param>
    /// <returns></returns>
    internal static string EncodeToString(ReadOnlySpan<byte> source, ReadOnlySpan<char> baseValues)
    {
        using var pooledData = Encode(source, baseValues);
        return pooledData.ToDisplayString();
    }

    /// <summary>
    /// 使用 <paramref name="baseValues"/> 作为基础表编码 <paramref name="source"/> 到字符串
    /// </summary>
    /// <param name="source"></param>
    /// <param name="baseValues"></param>
    /// <param name="encoding">编码，默认为 <see cref="Encoding.UTF8"/></param>
    /// <returns></returns>
    internal static string EncodeToString(ReadOnlySpan<char> source, ReadOnlySpan<char> baseValues, Encoding? encoding = null)
    {
#if NETSTANDARD2_0

        var sourceData = (encoding ?? Encoding.UTF8).GetBytes(source.ToArray());
        using var pooledData = Encode(sourceData, baseValues);
        return pooledData.ToDisplayString();

#else
        using var buffer = MemoryPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(source));

        var length = (encoding ?? Encoding.UTF8).GetBytes(source, buffer.Memory.Span);
        using var pooledData = Encode(buffer.Memory.Span.Slice(0, length), baseValues);
        return pooledData.ToDisplayString();

#endif
    }

    #endregion string

    #endregion Public 方法

    #region Impl

    private const int ByteMaxValue = byte.MaxValue + 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PooledData<byte> InternalGeneralBaseDecode(ReadOnlySpan<int> sourceIndexes, int targetBase)
    {
        if (sourceIndexes.IsEmpty)
        {
            return default;
        }

        return CanProcessWithBit(targetBase)
               ? InternalGeneralBitBaseDecode(sourceIndexes, targetBase)
               : InternalGeneralDivRemBaseDecode(sourceIndexes, targetBase);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PooledData<T> InternalGeneralBaseEncode<T>(ReadOnlySpan<byte> source, ReadOnlySpan<T> baseValues)
    {
        if (source.IsEmpty)
        {
            return default;
        }

        return CanProcessWithBit(baseValues.Length)
               ? InternalGeneralBitBaseEncode(source, baseValues)
               : InternalGeneralDivRemBaseEncode(source, baseValues);
    }

    #region Bit

    private static PooledData<byte> InternalGeneralBitBaseDecode(ReadOnlySpan<int> sourceIndexes, int targetBase)
    {
        var bitsPerData = InternalGetBitLength(targetBase);

        var resultBufferArray = ArrayPool<byte>.Shared.Rent(sourceIndexes.Length * sizeof(int));
        try
        {
            var bufferSpan = resultBufferArray.AsSpan();
            bufferSpan.Clear();
            var writer = new GeneralBitIntegerWriter(bufferSpan, bitsPerData);
            foreach (var item in sourceIndexes)
            {
                writer.Write(item);
            }
            return new(resultBufferArray, writer.Written);
        }
        catch
        {
            ArrayPool<byte>.Shared.Return(resultBufferArray, false);
            throw;
        }
        throw new NotImplementedException();
    }

    private static unsafe PooledData<T> InternalGeneralBitBaseEncode<T>(scoped ReadOnlySpan<byte> source, ReadOnlySpan<T> baseValues)
    {
        var sourceSize = source.Length;
        var baseSize = baseValues.Length;
        var bitsPerData = InternalGetBitLength(baseSize);
        var hasPadding = sizeof(ulong) > source.Length;

        if (hasPadding)
        {
            // 填充到 ulong 方便读取
            Span<byte> tempSource = stackalloc byte[sizeof(ulong)];
            source.CopyTo(tempSource);
            source = tempSource;
        }

        var reader = new GeneralBitIntegerReader(source, bitsPerData);

        var resultBufferArray = ArrayPool<T>.Shared.Rent((source.Length * 8) / bitsPerData + 1);
        try
        {
            var resultLength = 0;
            var baseIndex = 0;
            while (reader.Next(ref baseIndex))
            {
                resultBufferArray[resultLength++] = baseValues[baseIndex];
            }

            if (hasPadding)
            {
                var quotient = Math.DivRem(sourceSize * 8, bitsPerData, out var remainder);
                resultLength = quotient + (remainder > 0 ? 1 : 0);
            }

            return new PooledData<T>(resultBufferArray, resultBufferArray.AsSpan(0, resultLength));
        }
        catch
        {
            ArrayPool<T>.Shared.Return(resultBufferArray, false);
            throw;
        }
    }

    #endregion Bit

    #region DivRem

    // 从 https://github.com/JoyMoe/Base62.Net/blob/1f078f6cbbf0f00d7cea857e17c055354fb3ab88/src/Base62/EncodingExtensions.cs#L159 修改而来
    // 进行了一些内存使用和细节的调整

    private static PooledData<byte> InternalGeneralDivRemBaseDecode(ReadOnlySpan<int> sourceIndexes, int targetBase)
    {
        var sourceLength = sourceIndexes.Length;
        var magnification = Math.Max(targetBase / ByteMaxValue, 2);
        var leadingZeroCount = Math.Min(IndexOfLeftNotZero(sourceIndexes), sourceIndexes.Length - 1);

        var resultBufferArray = ArrayPool<byte>.Shared.Rent(sourceLength * magnification);
        var sourceBufferArray = ArrayPool<int>.Shared.Rent(sourceLength * 2);
        var sourceData = sourceIndexes;

        try
        {
            Span<byte> resultBufferSpan = resultBufferArray;

            Span<int> sourceBuffer1 = sourceBufferArray.AsSpan(0, sourceLength);
            Span<int> sourceBuffer2 = sourceBufferArray.AsSpan(sourceLength);
            Span<int> sourceBuffer = sourceBuffer1;

            var resultBufferIndex = resultBufferSpan.Length;
            var sourceBufferIndex = 0;
            var bufferSwitch = true;

            int count;

            while ((count = sourceData.Length) > 0)
            {
                var remainder = 0;
                for (var i = 0; i != count; i++)
                {
                    var accumulator = sourceData[i] + remainder * targetBase;
                    var digit = Math.DivRem(accumulator, ByteMaxValue, out remainder);

                    if (sourceBufferIndex > 0 || digit > 0)
                    {
                        sourceBuffer[sourceBufferIndex++] = digit;
                    }
                }

                resultBufferSpan[--resultBufferIndex] = (byte)remainder;
                sourceData = sourceBuffer.Slice(0, sourceBufferIndex);
                sourceBuffer = SwapBuffer(ref sourceBuffer1, ref sourceBuffer2, ref sourceBufferIndex, ref bufferSwitch);
            }

            if (leadingZeroCount > 0)
            {
                resultBufferIndex -= leadingZeroCount;
                resultBufferSpan.Slice(resultBufferIndex, leadingZeroCount).Clear();
            }

            resultBufferSpan = resultBufferSpan.Slice(resultBufferIndex);

            return new(resultBufferArray, resultBufferSpan);
        }
        catch
        {
            ArrayPool<byte>.Shared.Return(resultBufferArray, false);
            throw;
        }
        finally
        {
            ArrayPool<int>.Shared.Return(sourceBufferArray, false);
        }
    }

    private static PooledData<T> InternalGeneralDivRemBaseEncode<T>(ReadOnlySpan<byte> source, ReadOnlySpan<T> baseValues)
    {
        var targetBase = baseValues.Length;
        var sourceLength = source.Length;
        var magnification = Math.Max(ByteMaxValue / targetBase, 2);
        var leadingZeroCount = Math.Min(IndexOfLeftNotZero(source), source.Length - 1);

        var resultBufferArray = ArrayPool<T>.Shared.Rent(sourceLength * magnification);
        var sourceBufferArray = ArrayPool<byte>.Shared.Rent(sourceLength * 2);
        var sourceData = source;

        try
        {
            Span<T> resultBufferSpan = resultBufferArray;

            Span<byte> sourceBuffer1 = sourceBufferArray.AsSpan(0, sourceLength);
            Span<byte> sourceBuffer2 = sourceBufferArray.AsSpan(sourceLength);
            Span<byte> sourceBuffer = sourceBuffer1;

            var resultBufferIndex = resultBufferSpan.Length;
            var sourceBufferIndex = 0;
            var bufferSwitch = true;

            int count;

            while ((count = sourceData.Length) > 0)
            {
                var remainder = 0;
                for (var i = 0; i != count; i++)
                {
                    var accumulator = sourceData[i] + remainder * ByteMaxValue;
                    var digit = Math.DivRem(accumulator, targetBase, out remainder);

                    if (sourceBufferIndex > 0 || digit > 0)
                    {
                        sourceBuffer[sourceBufferIndex++] = (byte)digit;
                    }
                }

                resultBufferSpan[--resultBufferIndex] = baseValues[remainder];
                sourceData = sourceBuffer.Slice(0, sourceBufferIndex);
                sourceBuffer = SwapBuffer(ref sourceBuffer1, ref sourceBuffer2, ref sourceBufferIndex, ref bufferSwitch);
            }

            if (leadingZeroCount > 0)
            {
                resultBufferIndex -= leadingZeroCount;
                resultBufferSpan.Slice(resultBufferIndex, leadingZeroCount).Fill(baseValues[0]);
            }

            resultBufferSpan = resultBufferSpan.Slice(resultBufferIndex);

            return new PooledData<T>(resultBufferArray, resultBufferSpan);
        }
        catch
        {
            ArrayPool<T>.Shared.Return(resultBufferArray, false);
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(sourceBufferArray, false);
        }
    }

    #endregion DivRem

    #endregion Impl

    #region Base

    private static Dictionary<T, int> CreateBaseValueDictionary<T>(ReadOnlySpan<T> baseValues) where T : notnull
    {
        var dictionary = new Dictionary<T, int>(baseValues.Length);
        var index = 0;

        foreach (var item in baseValues)
        {
            dictionary.Add(item, index++);
        }

        return dictionary;
    }

    private static void CreateSourceIndexes<T>(ReadOnlySpan<T> source, Span<int> sourceIndexes, Dictionary<T, int> dictionary) where T : notnull
    {
        Debug.Assert(source.Length == sourceIndexes.Length);

        for (int i = 0; i < source.Length; i++)
        {
            sourceIndexes[i] = dictionary[source[i]];
        }
    }

#if NET8_0_OR_GREATER

    private static void CreateSourceIndexes<T>(ReadOnlySpan<T> source, Span<int> sourceIndexes, FrozenDictionary<T, int> dictionary) where T : notnull
    {
        Debug.Assert(source.Length == sourceIndexes.Length);

        for (int i = 0; i < source.Length; i++)
        {
            sourceIndexes[i] = dictionary[source[i]];
        }
    }

#endif

    [DebuggerStepThrough]
    private static int IndexOfLeftNotZero(ReadOnlySpan<byte> source)
    {
        var index = 0;
        for (; index < source.Length; index++)
        {
            if (source[index] != 0)
            {
                break;
            }
        }
        return index;
    }

    [DebuggerStepThrough]
    private static int IndexOfLeftNotZero(ReadOnlySpan<int> source)
    {
        var index = 0;
        for (; index < source.Length; index++)
        {
            if (source[index] != 0)
            {
                break;
            }
        }
        return index;
    }

    [DebuggerStepThrough]
    private static Span<T> SwapBuffer<T>(scoped ref Span<T> buffer1, scoped ref Span<T> buffer2, scoped ref int index, scoped ref bool bufferSwitch)
    {
        var temp = buffer1;
        buffer1 = buffer2;
        buffer2 = temp;
        index = 0;
        bufferSwitch = !bufferSwitch;
        return bufferSwitch ? buffer1 : buffer2;
    }

    #endregion Base

    #region Check

    private static readonly int[] s_canProcessWithBitValues =
        [
            1 << 1,
            1 << 2,
            1 << 3,
            1 << 4,
            1 << 5,
            1 << 6,
            1 << 7,
            1 << 8,
        ];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    private static bool CanProcessWithBit(int value)
    {
        return s_canProcessWithBitValues.Contains(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    private static int InternalGetBitLength(int value)
    {
        return s_canProcessWithBitValues.AsSpan().BinarySearch(value) + 1;
    }

    [DebuggerStepThrough]
    private static void ThrowIfBaseValuesLengthOutOfRange(int length)
    {
        if (length < 2)
        {
            throw new ArgumentOutOfRangeException("Base value at least two elements are required", (Exception?)null);
        }
    }

    #endregion Check

    #region Encoder Impls

    private sealed class DefaultBitBaseAnyEncoder<T> : IBaseAnyEncoder<T> where T : notnull, IEquatable<T>
    {
        #region Private 字段

        private readonly int _baseLength = 0;

#if NET8_0_OR_GREATER

        private readonly FrozenDictionary<T, int> _baseValueDictionary;

#else

        private readonly Dictionary<T, int> _baseValueDictionary;

#endif

        private readonly T[] _baseValues;

        #endregion Private 字段

        #region Internal 构造函数

        internal DefaultBitBaseAnyEncoder(ReadOnlySpan<T> baseValues)
        {
            _baseLength = baseValues.Length;

            if (_baseLength < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(baseValues));
            }

#if NET8_0_OR_GREATER

            _baseValueDictionary = CreateBaseValueDictionary(baseValues).ToFrozenDictionary();

#else

            _baseValueDictionary = CreateBaseValueDictionary(baseValues);

#endif

            _baseValues = baseValues.ToArray();
        }

        #endregion Internal 构造函数

        #region Public 方法

        public PooledData<byte> Decode(ReadOnlySpan<T> source)
        {
            var sourceIndexesBuffer = ArrayPool<int>.Shared.Rent(source.Length);

            try
            {
                var sourceIndexes = sourceIndexesBuffer.AsSpan(0, source.Length);
                CreateSourceIndexes(source, sourceIndexes, _baseValueDictionary);

                return InternalGeneralBitBaseDecode(sourceIndexes, _baseLength);
            }
            finally
            {
                ArrayPool<int>.Shared.Return(sourceIndexesBuffer, false);
            }
        }

        public void Dispose()
        { }

        public PooledData<T> Encode(ReadOnlySpan<byte> source)
        {
            return InternalGeneralBitBaseEncode<T>(source, _baseValues);
        }

        #endregion Public 方法
    }

    private sealed class DefaultDivRemBaseAnyEncoder<T> : IBaseAnyEncoder<T> where T : notnull, IEquatable<T>
    {
        #region Private 字段

        private readonly int _baseLength = 0;

#if NET8_0_OR_GREATER

        private readonly FrozenDictionary<T, int> _baseValueDictionary;

#else

        private readonly Dictionary<T, int> _baseValueDictionary;

#endif

        private readonly T[] _baseValues;

        #endregion Private 字段

        #region Internal 构造函数

        internal DefaultDivRemBaseAnyEncoder(ReadOnlySpan<T> baseValues)
        {
            _baseLength = baseValues.Length;

            if (_baseLength < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(baseValues));
            }

#if NET8_0_OR_GREATER

            _baseValueDictionary = CreateBaseValueDictionary(baseValues).ToFrozenDictionary();

#else

            _baseValueDictionary = CreateBaseValueDictionary(baseValues);

#endif

            _baseValues = baseValues.ToArray();
        }

        #endregion Internal 构造函数

        #region Public 方法

        public PooledData<byte> Decode(ReadOnlySpan<T> source)
        {
            var sourceIndexesBuffer = ArrayPool<int>.Shared.Rent(source.Length);

            try
            {
                var sourceIndexes = sourceIndexesBuffer.AsSpan(0, source.Length);
                CreateSourceIndexes(source, sourceIndexes, _baseValueDictionary);

                return InternalGeneralDivRemBaseDecode(sourceIndexes, _baseLength);
            }
            finally
            {
                ArrayPool<int>.Shared.Return(sourceIndexesBuffer, false);
            }
        }

        public void Dispose()
        { }

        public PooledData<T> Encode(ReadOnlySpan<byte> source)
        {
            return InternalGeneralDivRemBaseEncode<T>(source, _baseValues);
        }

        #endregion Public 方法
    }

    #endregion Encoder Impls

    #region Primitives

    [DebuggerStepThrough]
    private ref struct GeneralBitIntegerReader
    {
        #region Public 字段

        internal const int BitsPerByte = 8;

        internal const int BitsPerUnsignedInteger = sizeof(int) * BitsPerByte;

        internal const int BitsPerUnsignedLong = sizeof(ulong) * BitsPerByte;

        #endregion Public 字段

        #region Private 字段

        private readonly int _bitLength;

        private readonly int _bitsPerData;

        private readonly int _dataFixedBitIndex;

        private readonly int _dataFixedByteIndex;

        private readonly int _valueRightShiftBit;

        private int _bitIndex;

        private int _currentByteBitUsed;

        private ReadOnlySpan<byte> _internalData;

        #endregion Private 字段

        #region Public 属性

        internal readonly ReadOnlySpan<byte> Data { get; }

        #endregion Public 属性

        #region Public 构造函数

        [DebuggerStepThrough]
        internal GeneralBitIntegerReader(ReadOnlySpan<byte> data, int bitsPerData)
        {
            if (data.Length < sizeof(ulong))
            {
                throw new ArgumentOutOfRangeException(nameof(data), "must padding data to a size no less than ulong");
            }

            if (bitsPerData < 1
                || bitsPerData >= sizeof(byte) * BitsPerByte)
            {
                throw new ArgumentOutOfRangeException(nameof(bitsPerData));
            }

            Data = data;
            _internalData = data;

            _bitLength = data.Length * BitsPerByte;
            _bitsPerData = bitsPerData;

            //确保最后剩余的数据足够读取为 ulong
            _dataFixedBitIndex = _bitLength - BitsPerUnsignedLong;
            _dataFixedByteIndex = data.Length - sizeof(ulong);

            _valueRightShiftBit = BitsPerUnsignedLong - bitsPerData;
            _bitIndex = 0;
            _currentByteBitUsed = 0;
        }

        #endregion Public 构造函数

        #region Public 方法

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool Next(ref int value)
        {
            if (_bitIndex < _dataFixedBitIndex)
            {
                value = ReadNextAsInt32();
                return true;
            }
            else if (_bitIndex < _bitLength)
            {
                value = ReadNextAsInt32WithOutDataMove();
                return true;
            }
            return false;
        }

        #endregion Public 方法

        #region Private 方法

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong ReadAsUInt64(in ReadOnlySpan<byte> source)
        {
            return BinaryPrimitives.ReadUInt64BigEndian(source);
        }

        private int ReadNextAsInt32()
        {
            var value = ReadAsUInt64(_internalData);

            value <<= _currentByteBitUsed;
            value >>= _valueRightShiftBit;

            _bitIndex += _bitsPerData;

            var bitMoved = _bitsPerData + _currentByteBitUsed;
            if (bitMoved >= BitsPerByte)
            {
                var quotient = Math.DivRem(bitMoved, BitsPerByte, out _currentByteBitUsed);
                _internalData = _internalData.Slice(quotient);
            }
            else
            {
                _currentByteBitUsed += _bitsPerData;
            }

            //HACK 理论上不会有符号溢出
            return (int)value;
        }

        private int ReadNextAsInt32WithOutDataMove()
        {
            var value = ReadAsUInt64(Data.Slice(_dataFixedByteIndex));

            value <<= _bitIndex - _dataFixedBitIndex;
            value >>= _valueRightShiftBit;

            _bitIndex += _bitsPerData;

            //HACK 理论上不会有符号溢出
            return (int)value;
        }

        #endregion Private 方法
    }

    [DebuggerStepThrough]
    private ref struct GeneralBitIntegerWriter
    {
        #region Public 字段

        internal const int BitsPerByte = 8;

        internal const int BitsPerUnsignedInteger = sizeof(int) * BitsPerByte;

        #endregion Public 字段

        #region Private 字段

        private readonly int _bitsPerData;

        private readonly Span<byte> _buffer;

        private readonly int _valueLeftShiftBit;

        private int _currentByteBitUsed;

        private Span<byte> _internalBuffer;

        private int _writtenByteLength;

        #endregion Private 字段

        #region Public 属性

        internal readonly Span<byte> Written => GetWritten();

        #endregion Public 属性

        #region Public 构造函数

        internal GeneralBitIntegerWriter(Span<byte> buffer, int bitsPerData)
        {
            if (bitsPerData < 1
                || bitsPerData >= sizeof(byte) * BitsPerByte)
            {
                throw new ArgumentOutOfRangeException(nameof(bitsPerData));
            }

            _internalBuffer = buffer;

            _buffer = buffer;
            _bitsPerData = bitsPerData;

            _valueLeftShiftBit = BitsPerUnsignedInteger - bitsPerData;
            _writtenByteLength = 0;
            _currentByteBitUsed = 0;
        }

        #endregion Public 构造函数

        #region Public 方法

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Write(int value)
        {
            var unsignedValue = (uint)value;

            unsignedValue <<= _valueLeftShiftBit;
            unsignedValue >>= _currentByteBitUsed;

            var existedValue = BinaryPrimitives.ReadUInt32BigEndian(_internalBuffer);

            existedValue |= unsignedValue;

            BinaryPrimitives.WriteUInt32BigEndian(_internalBuffer, existedValue);

            var bitMoved = _bitsPerData + _currentByteBitUsed;
            if (bitMoved >= BitsPerByte)
            {
                var quotient = Math.DivRem(bitMoved, BitsPerByte, out _currentByteBitUsed);
                _internalBuffer = _internalBuffer.Slice(quotient);
                _writtenByteLength += quotient;
            }
            else
            {
                _currentByteBitUsed += _bitsPerData;
            }
        }

        #endregion Public 方法

        #region Private 方法

        private readonly Span<byte> GetWritten()
        {
            var length = _writtenByteLength + (_currentByteBitUsed >= _bitsPerData ? 1 : 0);
            return _buffer.Slice(0, length);
        }

        #endregion Private 方法
    }

    #endregion Primitives
}

/// <summary>
/// <see cref="BaseAnyEncoding"/> 拓展方法
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class BaseAnyEncodingExtensions
{
    #region Public 方法

    /// <summary>
    /// 从字符串 <paramref name="source"/> 解码数据到 <see cref="byte"/> 序列
    /// </summary>
    /// <param name="encoder"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static PooledData<byte> DecodeFromString(this IBaseAnyEncoder<char> encoder, ReadOnlySpan<char> source)
    {
        return encoder.Decode(source);
    }

    /// <summary>
    /// 从 <paramref name="source"/> 解码数据到字符串
    /// </summary>
    /// <param name="encoder"></param>
    /// <param name="source"></param>
    /// <param name="encoding">编码，默认为 <see cref="Encoding.UTF8"/></param>
    /// <returns></returns>
    internal static string DecodeToString(this IBaseAnyEncoder<char> encoder, ReadOnlySpan<char> source, Encoding? encoding = null)
    {
        using var pooledData = encoder.Decode(source);
        return pooledData.ToDisplayString(encoding);
    }

    /// <summary>
    /// 编码数据 <paramref name="source"/> 到字符串
    /// </summary>
    /// <param name="encoder"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static string EncodeToString(this IBaseAnyEncoder<char> encoder, ReadOnlySpan<byte> source)
    {
        using var pooledData = encoder.Encode(source);
        return pooledData.ToDisplayString();
    }

    /// <summary>
    /// 编码字符串 <paramref name="source"/> 到字符串
    /// </summary>
    /// <param name="encoder"></param>
    /// <param name="source"></param>
    /// <param name="encoding">编码，默认为 <see cref="Encoding.UTF8"/></param>
    /// <returns></returns>
    internal static string EncodeToString(this IBaseAnyEncoder<char> encoder, string source, Encoding? encoding = null)
    {
        //TODO ArrayPool
        var sourceData = (encoding ?? Encoding.UTF8).GetBytes(source);
        using var pooledData = encoder.Encode(sourceData);
        return pooledData.ToDisplayString();
    }

    #endregion Public 方法
}

/// <summary>
/// <see cref="PooledData{T}"/> 拓展方法
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class PooledDataExtensions
{
    #region Public 方法

    /// <summary>
    /// 创建用于展示的字符串
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pooledData"></param>
    /// <param name="separator">分隔字符串，默认没有分隔</param>
    /// <param name="itemConverter">元素转换器，默认为 <see cref="object.ToString()"/></param>
    /// <param name="capacity">内部使用的 <see cref="StringBuilder"/> 的初始化容量</param>
    /// <returns></returns>
    internal static string ToDisplayString<T>(this PooledData<T> pooledData, string? separator = null, Func<T, string>? itemConverter = null, int capacity = 128)
    {
        var span = pooledData.Span;

        if (span.IsEmpty)
        {
            return string.Empty;
        }

        itemConverter ??= static value => value!.ToString()!;

        var builder = new StringBuilder(capacity);

        if (string.IsNullOrEmpty(separator))
        {
            foreach (var item in span)
            {
                builder.Append(itemConverter(item));
            }
        }
        else
        {
            foreach (var item in span)
            {
                builder.Append(itemConverter(item));
                builder.Append(separator);
            }
            builder.Remove(builder.Length - separator!.Length, separator.Length);
        }

        return builder.ToString();
    }

    /// <summary>
    /// 使用指定编码 <paramref name="encoding"/> 创建用于展示的字符串
    /// </summary>
    /// <param name="pooledData"></param>
    /// <param name="encoding">编码，默认为 <see cref="Encoding.UTF8"/></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    internal static string ToDisplayString(this PooledData<byte> pooledData, Encoding? encoding = null)
    {
        var span = pooledData.Span;

        if (span.IsEmpty)
        {
            return string.Empty;
        }

#if NETSTANDARD2_0

        return (encoding ?? Encoding.UTF8).GetString(span.ToArray());

#else

        return (encoding ?? Encoding.UTF8).GetString(span);

#endif
    }

    /// <summary>
    /// 创建用于展示的字符串
    /// </summary>
    /// <param name="pooledData"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [DebuggerStepThrough]
    internal static string ToDisplayString(this PooledData<char> pooledData)
    {
        return pooledData.Span.ToString();
    }

    #endregion Public 方法
}
