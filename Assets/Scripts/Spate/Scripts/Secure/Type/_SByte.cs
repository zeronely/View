using System;

namespace Spate
{
    /// <summary>
    /// 安全类型byte
    /// </summary>
    public struct _SByte
    {
        private static readonly sbyte xorKey;
        static _SByte()
        {
            xorKey = (sbyte)(new Random(Environment.TickCount).Next(0, sbyte.MaxValue));
        }

        private sbyte encValue;
        private byte initFlag;

        public _SByte(sbyte defValue)
        {
            encValue = 0;
            initFlag = 0;
            Value = defValue;
        }

        private sbyte Value
        {
            get
            {
                if (initFlag == 0) return 0;
                return (sbyte)(xorKey ^ encValue);
            }
            set
            {
                encValue = (sbyte)(xorKey ^ value);
                initFlag = 1;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is _SByte) && ((_SByte)obj).Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator int(_SByte s)
        {
            return s.Value;
        }

        public static implicit operator _SByte(sbyte v)
        {
            return new _SByte(v);
        }

        public static bool operator ==(_SByte a, _SByte b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(_SByte a, _SByte b)
        {
            return a != b;
        }

        public static _SByte operator ++(_SByte s)
        {
            return new _SByte((sbyte)(s.Value + 1));
        }

        public static _SByte operator --(_SByte s)
        {
            return new _SByte((sbyte)(s.Value - 1));
        }
    }
}