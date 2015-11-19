using System;

namespace Spate
{
    /// <summary>
    /// 安全类型ushort
    /// </summary>
    public struct _UShort
    {
        private static readonly ushort xorKey;
        static _UShort()
        {
            xorKey = (ushort)(new Random(Environment.TickCount).Next(0, ushort.MaxValue));
        }

        private ushort encValue;
        private byte initFlag;

        public _UShort(ushort defValue)
        {
            encValue = 0;
            initFlag = 0;
            Value = defValue;
        }

        private ushort Value
        {
            get
            {
                if (initFlag == 0) return 0;
                return (ushort)(xorKey ^ encValue);
            }
            set
            {
                encValue = (ushort)(xorKey ^ value);
                initFlag = 1;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is _UShort) && ((_UShort)obj).Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator ushort(_UShort s)
        {
            return s.Value;
        }

        public static implicit operator _UShort(byte v)
        {
            return new _UShort(v);
        }

        public static implicit operator _UShort(ushort v)
        {
            return new _UShort(v);
        }

        public static bool operator ==(_UShort a, _UShort b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(_UShort a, _UShort b)
        {
            return a.Value != b.Value;
        }

        public static _UShort operator ++(_UShort s)
        {
            return new _UShort((ushort)(s.Value + 1));
        }

        public static _UShort operator --(_UShort s)
        {
            return new _UShort((ushort)(s.Value - 1));
        }
    }
}
