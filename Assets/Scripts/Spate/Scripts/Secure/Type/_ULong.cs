using System;

namespace Spate
{
    /// <summary>
    /// 安全类型ulong
    /// </summary>
    public struct _ULong
    {
        private static readonly ulong xorKey;
        static _ULong()
        {
            xorKey = (ulong)(new Random(Environment.TickCount).Next(0, ushort.MaxValue));
        }

        private ulong encValue;
        private byte initFlag;

        public _ULong(ulong defValue)
        {
            encValue = 0;
            initFlag = 0;
            Value = defValue;
        }

        private ulong Value
        {
            get
            {
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
            return (obj is _ULong) && ((_ULong)obj).Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator ulong(_ULong s)
        {
            return s.Value;
        }

        public static implicit operator _ULong(byte v)
        {
            return new _ULong(v);
        }

        public static implicit operator _ULong(ushort v)
        {
            return new _ULong(v);
        }

        public static implicit operator _ULong(uint v)
        {
            return new _ULong(v);
        }

        public static implicit operator _ULong(ulong v)
        {
            return new _ULong(v);
        }

        public static bool operator ==(_ULong a, _ULong b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(_ULong a, _ULong b)
        {
            return a.Value != b.Value;
        }

        public static _ULong operator ++(_ULong s)
        {
            return new _ULong(s.Value + 1);
        }

        public static _ULong operator --(_ULong s)
        {
            return new _ULong(s.Value - 1);
        }
    }
}
