using System;
using System.Runtime.InteropServices;

/// <summary>
/// 内存混淆,防止内存修改工具对数据进行修改
/// </summary>
public sealed class MemoryShuffle
{
    // 随机器
    private static readonly Random SEED = new Random(Environment.TickCount);

    private static uint en_value_uint(uint orgValue, uint secretKey)
    {
        return orgValue ^ secretKey;
    }

    private static uint de_value_uint(uint encValue, uint secretKey)
    {
        return encValue ^ secretKey;
    }

    #region 32位值类型(int,uint,float)

    /// <summary>
    /// 32位值的联合体
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    private struct UnionValue32
    {
        [FieldOffset(0)]
        public int intValue;
        [FieldOffset(0)]
        public uint uintValue;
        [FieldOffset(0)]
        public float floatValue;
    }

    /// <summary>
    /// 所有32位数值类型的基类，仅提供秘钥的生成
    /// </summary>
    public class Shuffle32Base
    {
        private readonly uint SECRET_KEY = 0U;
        private UnionValue32 encUnion;

        public Shuffle32Base()
        {
            // 初始化秘钥
            SECRET_KEY |= (uint)SEED.Next(1, 255);
            SECRET_KEY |= (uint)(SEED.Next(1, 255) << 8);
            SECRET_KEY |= (uint)(SEED.Next(1, 255) << 16);
            SECRET_KEY |= (uint)(SEED.Next(1, 255) << 24);
        }

        /// <summary>
        /// 获取或设置Int型值
        /// </summary>
        protected int ValueInt
        {
            get
            {
                UnionValue32 union;
                union.intValue = 0;
                union.uintValue = de_value_uint(encUnion.uintValue, SECRET_KEY);
                return union.intValue;
            }
            set
            {
                encUnion.intValue = value;
                encUnion.uintValue = en_value_uint(encUnion.uintValue, SECRET_KEY);
            }
        }

        /// <summary>
        /// 获取或设置UInt型值
        /// </summary>
        protected uint ValueUInt
        {
            get
            {
                UnionValue32 union;
                union.uintValue = 0u;
                union.uintValue = de_value_uint(encUnion.uintValue, SECRET_KEY);
                return union.uintValue;
            }
            set
            {
                encUnion.uintValue = value;
                encUnion.uintValue = en_value_uint(encUnion.uintValue, SECRET_KEY);
            }
        }

        /// <summary>
        /// 获取或设置Float值
        /// </summary>
        protected float ValueFloat
        {
            get
            {
                UnionValue32 union;
                union.floatValue = 0f;
                union.uintValue = de_value_uint(encUnion.uintValue, SECRET_KEY);
                return union.floatValue;
            }
            set
            {
                encUnion.floatValue = value;
                encUnion.uintValue = en_value_uint(encUnion.uintValue, SECRET_KEY);
            }
        }
    }

    public class Shuffle_Int : Shuffle32Base
    {
        public Shuffle_Int() : this(0) { }

        public Shuffle_Int(int defValue) : base() { Value = defValue; }

        public int Value
        {
            get { return ValueInt; }
            set { ValueInt = value; }
        }
    }

    public class Shuffle_UInt : Shuffle32Base
    {
        public Shuffle_UInt() : this(0U) { }

        public Shuffle_UInt(uint defValue) : base() { Value = defValue; }

        public uint Value
        {
            get { return ValueUInt; }
            set { ValueUInt = value; }
        }
    }

    public class Shuffle_Float : Shuffle32Base
    {
        public Shuffle_Float() : this(0F) { }

        public Shuffle_Float(float defValue) : base() { Value = defValue; }

        public float Value
        {
            get { return ValueFloat; }
            set { ValueFloat = value; }
        }
    }

    public class Shuffle_IntArray
    {
        private Shuffle_Int[] arr;

        public Shuffle_IntArray(int length)
        {
            arr = new Shuffle_Int[length];
            for (int i = 0; i < length; i++) arr[i] = new Shuffle_Int();
        }

        public int this[int index]
        {
            get { return arr[index].Value; }
            set { arr[index].Value = value; }
        }
    }

    public class Shuffle_UIntArray
    {
        private Shuffle_UInt[] arr;

        public Shuffle_UIntArray(int length)
        {
            arr = new Shuffle_UInt[length];
            for (int i = 0; i < length; i++) arr[i] = new Shuffle_UInt();
        }

        public uint this[int index]
        {
            get { return arr[index].Value; }
            set { arr[index].Value = value; }
        }
    }

    public class Shuffle_FloatArray
    {
        private Shuffle_Float[] arr;

        public Shuffle_FloatArray(int length)
        {
            arr = new Shuffle_Float[length];
            for (int i = 0; i < length; i++) arr[i] = new Shuffle_Float();
        }

        public float this[int index]
        {
            get { return arr[index].Value; }
            set { arr[index].Value = value; }
        }
    }

    #endregion

    #region 64位值类型(long,ulong,double)

    [StructLayout(LayoutKind.Explicit)]
    private struct UnionValue64
    {
        [FieldOffset(0)]
        public long longValue;
        [FieldOffset(0)]
        public ulong ulongValue;
        [FieldOffset(0)]
        public double doubleValue;
        [FieldOffset(0)]
        public UnionUint64 uintArray;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct UnionUint64
    {
        [FieldOffset(0)]
        public ulong ulongValue;
        [FieldOffset(0)]
        public uint uintValue0;
        [FieldOffset(4)]
        public uint uintValue1;

        public uint this[int index]
        {
            get
            {
                if (index == 0) return uintValue0;
                if (index != 1) return 0U;
                return uintValue1;
            }
            set
            {
                if (index != 0)
                {
                    if (index == 1) uintValue1 = value;
                }
                else
                {
                    uintValue0 = value;
                }

            }
        }
    }

    public class Shuffle64Base
    {
        private readonly ulong SECRET_KEY = 0UL;
        private UnionValue64 encUnion;

        public Shuffle64Base()
        {
            SECRET_KEY = 0UL;
            SECRET_KEY |= (ulong)(long)SEED.Next(1, 255);
            SECRET_KEY |= (ulong)(long)(SEED.Next(1, 255) << 8);
            SECRET_KEY |= (ulong)(long)(SEED.Next(1, 255) << 16);
            SECRET_KEY |= (ulong)(long)(SEED.Next(1, 255) << 24);
            SECRET_KEY |= (ulong)(long)SEED.Next(1, 255);
            SECRET_KEY |= (ulong)(long)(SEED.Next(1, 255) << 8);
            SECRET_KEY |= (ulong)(long)(SEED.Next(1, 255) << 16);
            SECRET_KEY |= (ulong)(long)(SEED.Next(1, 255) << 24);
        }

        public long ValueLong
        {
            get
            {
                UnionValue64 union;
                union.longValue = 0L;
                union.ulongValue = 0UL;
                union.uintArray = default(UnionUint64);
                union.uintArray[0] = de_value_uint(encUnion.uintArray[0], (uint)(SECRET_KEY & 268435455UL));
                union.uintArray[1] = de_value_uint(encUnion.uintArray[1], (uint)(SECRET_KEY >> 32 & 268435455UL));
                return union.longValue;
            }
            set
            {
                encUnion.longValue = value;
                encUnion.uintArray[0] = en_value_uint(encUnion.uintArray[0], (uint)(SECRET_KEY & 268435455UL));
                encUnion.uintArray[1] = en_value_uint(encUnion.uintArray[1], (uint)(SECRET_KEY >> 32 & 268435455UL));
            }
        }

        public ulong ValueULong
        {
            get
            {
                UnionValue64 union;
                union.ulongValue = 0UL;
                union.uintArray = default(UnionUint64);
                union.uintArray[0] = de_value_uint(encUnion.uintArray[0], (uint)(SECRET_KEY & 268435455UL));
                union.uintArray[1] = de_value_uint(encUnion.uintArray[1], (uint)(SECRET_KEY >> 32 & 268435455UL));
                return union.ulongValue;
            }
            set
            {
                encUnion.ulongValue = value;
                encUnion.uintArray[0] = en_value_uint(encUnion.uintArray[0], (uint)(SECRET_KEY & 268435455UL));
                encUnion.uintArray[1] = en_value_uint(encUnion.uintArray[1], (uint)(SECRET_KEY >> 32 & 268435455UL));
            }
        }

        public double ValueDouble
        {
            get
            {
                UnionValue64 union;
                union.doubleValue = 0.0D;
                union.ulongValue = 0UL;
                union.uintArray = default(UnionUint64);
                union.uintArray[0] = de_value_uint(encUnion.uintArray[0], (uint)(SECRET_KEY & 268435455UL));
                union.uintArray[1] = de_value_uint(encUnion.uintArray[1], (uint)(SECRET_KEY >> 32 & 268435455UL));
                return union.doubleValue;
            }
            set
            {
                encUnion.doubleValue = value;
                encUnion.uintArray[0] = en_value_uint(encUnion.uintArray[0], (uint)(SECRET_KEY & 268435455UL));
                encUnion.uintArray[1] = en_value_uint(encUnion.uintArray[1], (uint)(SECRET_KEY >> 32 & 268435455UL));
            }
        }
    }

    public class Shuffle_Long : Shuffle64Base
    {
        public Shuffle_Long() : this(0L) { }
        public Shuffle_Long(long defValue) : base() { Value = defValue; }

        public long Value
        {
            get { return ValueLong; }
            set { ValueLong = value; }
        }
    }

    public class Shuffle_ULong : Shuffle64Base
    {
        public Shuffle_ULong() : this(0UL) { }
        public Shuffle_ULong(ulong defValue) : base() { Value = defValue; }

        public ulong Value
        {
            get { return ValueULong; }
            set { ValueULong = value; }
        }
    }

    public class Shuffle_Double : Shuffle64Base
    {
        public Shuffle_Double() : this(0D) { }
        public Shuffle_Double(double defValue) : base() { Value = defValue; }

        public double Value
        {
            get { return ValueDouble; }
            set { ValueDouble = value; }
        }
    }

    public class Shuffle_LongArray
    {
        private Shuffle_Long[] arr;

        public Shuffle_LongArray(int length)
        {
            arr = new Shuffle_Long[length];
            for (int i = 0; i < length; i++) arr[i] = new Shuffle_Long();
        }

        public long this[int index]
        {
            get { return arr[index].Value; }
            set { arr[index].Value = value; }
        }
    }

    public class Shuffle_ULongArray
    {
        private Shuffle_ULong[] arr;

        public Shuffle_ULongArray(int length)
        {
            arr = new Shuffle_ULong[length];
            for (int i = 0; i < length; i++) arr[i] = new Shuffle_ULong();
        }

        public ulong this[int index]
        {
            get { return arr[index].Value; }
            set { arr[index].Value = value; }
        }
    }

    public class Shuffle_DoubleArray
    {
        private Shuffle_Double[] arr;

        public Shuffle_DoubleArray(int length)
        {
            arr = new Shuffle_Double[length];
            for (int i = 0; i < length; i++) arr[i] = new Shuffle_Double();
        }

        public double this[int index]
        {
            get { return arr[index].Value; }
            set { arr[index].Value = value; }
        }
    }

    #endregion
}
