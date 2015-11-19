using System;

namespace Spate
{
    /// <summary>
    /// 安全类型int
    /// </summary>
    public struct _Int
    {
        private static readonly int xorKey;
        static _Int()
        {
            xorKey = new Random(Environment.TickCount).Next(0, ushort.MaxValue);
        }

        private int encValue;
        private byte initFlag;//记录是否为初始状态,初始状态返回默认值

        public _Int(int defValue)
        {
            encValue = 0;
            initFlag = 0;
            Value = defValue;
        }

        private int Value
        {
            get
            {
                // 如果从未赋值过则保持默认值
                if (initFlag == 0) return 0;
                return xorKey ^ encValue;
            }
            set
            {
                encValue = xorKey ^ value;
                initFlag = 1;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is _Int) && ((_Int)obj).Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator int(_Int s)
        {
            return s.Value;
        }

        public static implicit operator _Int(sbyte v)
        {
            return new _Int(v);
        }

        public static implicit operator _Int(byte v)
        {
            return new _Int(v);
        }

        public static implicit operator _Int(short v)
        {
            return new _Int(v);
        }

        public static implicit operator _Int(ushort v)
        {
            return new _Int(v);
        }

        public static implicit operator _Int(int v)
        {
            return new _Int(v);
        }

        public static bool operator ==(_Int a, _Int b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(_Int a, _Int b)
        {
            return a.Value != b.Value;
        }

        public static _Int operator ++(_Int s)
        {
            return new _Int(s.Value + 1);
        }

        public static _Int operator --(_Int s)
        {
            return new _Int(s.Value - 1);
        }
    }
}
