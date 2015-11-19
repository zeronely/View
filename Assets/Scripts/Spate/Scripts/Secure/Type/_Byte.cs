using System;

namespace Spate
{
    /// <summary>
    /// 安全类型byte
    /// </summary>
    public struct _Byte
    {
        private static readonly byte xorKey;
        static _Byte()
        {
            xorKey = (byte)(new Random(Environment.TickCount).Next(0, byte.MaxValue));
        }

        private byte encValue;
        private byte initFlag;

        public _Byte(byte defValue)
        {
            encValue = 0;
            initFlag = 0;
            Value = defValue;
        }

        private byte Value
        {
            get
            {
                if (initFlag == 0) return 0;
                return (byte)(xorKey ^ encValue);
            }
            set
            {
                encValue = (byte)(xorKey ^ value);
                initFlag = 1;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is _Byte) && ((_Byte)obj).Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator int(_Byte s)
        {
            return s.Value;
        }

        public static implicit operator _Byte(byte v)
        {
            return new _Byte(v);
        }

        public static bool operator ==(_Byte a, _Byte b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(_Byte a, _Byte b)
        {
            return a != b;
        }

        public static _Byte operator ++(_Byte s)
        {
            return new _Byte((byte)(s.Value + 1));
        }

        public static _Byte operator --(_Byte s)
        {
            return new _Byte((byte)(s.Value - 1));
        }
    }
}