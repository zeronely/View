using System;

namespace Spate
{
    /// <summary>
    /// 安全类型short
    /// </summary>
    public struct _Short
    {
        private static readonly short xorKey;
        static _Short()
        {
            xorKey = (short)(new Random(Environment.TickCount).Next(0, ushort.MaxValue));
        }

        private short encValue;
        private byte initFlag;

        public _Short(short defValue)
        {
            encValue = 0;
            initFlag = 0;
            Value = defValue;
        }

        private short Value
        {
            get
            {
                if (initFlag == 0) return 0;
                return (short)(xorKey ^ encValue);
            }
            set
            {
                encValue = (short)(xorKey ^ value);
                initFlag = 1;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is _Short) && ((_Short)obj).Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator short(_Short s)
        {
            return s.Value;
        }

        public static implicit operator _Short(sbyte v)
        {
            return new _Short(v);
        }

        public static implicit operator _Short(byte v)
        {
            return new _Short(v);
        }

        public static implicit operator _Short(short v)
        {
            return new _Short(v);
        }

        public static bool operator ==(_Short a, _Short b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(_Short a, _Short b)
        {
            return a.Value != b.Value;
        }

        public static _Short operator ++(_Short s)
        {
            return new _Short((short)(s.Value + 1));
        }

        public static _Short operator --(_Short s)
        {
            return new _Short((short)(s.Value - 1));
        }
    }
}