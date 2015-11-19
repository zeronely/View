using System;

namespace Spate
{
    /// <summary>
    /// 安全类型uint
    /// </summary>
    public struct _UInt
    {
        private static readonly uint xorKey;
        static _UInt()
        {
            xorKey = (uint)(new Random(Environment.TickCount).Next(0, ushort.MaxValue));
        }

        private uint encValue;
        private byte initFlag;

        public _UInt(uint defValue)
        {
            encValue = 0;
            initFlag = 0;
            Value = defValue;
        }

        private uint Value
        {
            get
            {
                if (initFlag == 0) return 0;
                return (uint)(xorKey ^ encValue);
            }
            set
            {
                encValue = (uint)(xorKey ^ value);
                initFlag = 1;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is _UInt) && ((_UInt)obj).Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator uint(_UInt s)
        {
            return s.Value;
        }

        public static implicit operator _UInt(byte v)
        {
            return new _UInt(v);
        }

        public static implicit operator _UInt(ushort v)
        {
            return new _UInt(v);
        }

        public static implicit operator _UInt(uint v)
        {
            return new _UInt(v);
        }

        public static bool operator ==(_UInt a, _UInt b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(_UInt a, _UInt b)
        {
            return a.Value != b.Value;
        }

        public static _UInt operator ++(_UInt s)
        {
            return new _UInt(s.Value + 1);
        }

        public static _UInt operator --(_UInt s)
        {
            return new _UInt(s.Value - 1);
        }
    }
}
