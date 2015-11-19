using System;

namespace Spate
{
    /// <summary>
    /// 安全类型long
    /// </summary>
    public struct _Long
    {
        private static readonly long xorKey;
        static _Long()
        {
            xorKey = new Random(Environment.TickCount).Next(0, ushort.MaxValue);
        }

        private long encValue;
        private byte initFlag;

        public _Long(long defValue)
        {
            encValue = 0;
            initFlag = 0;
            Value = defValue;
        }

        private long Value
        {
            get
            {
                if (initFlag == 0) return 0L;
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
            return (obj is _Long) && ((_Long)obj).Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator long(_Long s)
        {
            return s.Value;
        }

        public static implicit operator _Long(sbyte v)
        {
            return new _Long(v);
        }

        public static implicit operator _Long(byte v)
        {
            return new _Long(v);
        }

        public static implicit operator _Long(short v)
        {
            return new _Long(v);
        }

        public static implicit operator _Long(ushort v)
        {
            return new _Long(v);
        }

        public static implicit operator _Long(int v)
        {
            return new _Long(v);
        }

        public static implicit operator _Long(uint v)
        {
            return new _Long(v);
        }

        public static implicit operator _Long(long v)
        {
            return new _Long(v);
        }

        public static bool operator ==(_Long a, _Long b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(_Long a, _Long b)
        {
            return a.Value != b.Value;
        }

        public static _Long operator ++(_Long s)
        {
            return new _Long(s.Value + 1);
        }

        public static _Long operator --(_Long s)
        {
            return new _Long(s.Value - 1);
        }
    }
}