using System;

namespace Spate
{
    /// <summary>
    /// 安全类型float
    /// </summary>
    public struct _Float
    {
        private static readonly byte xorKey;
        static _Float()
        {
            xorKey = (byte)(new Random(Environment.TickCount).Next(0, byte.MaxValue));
        }

        private uint encValue;
        private byte initFlag;

        public _Float(float defValue)
        {
            encValue = 0;
            initFlag = 0;
            Value = defValue;
        }

        private float Value
        {
            get
            {
                if (initFlag == 0) return 0f;
                byte[] raw = BitConverter.GetBytes(encValue);
                for (int i = 0; i < raw.Length; i++)
                {
                    raw[i] ^= xorKey;
                }
                return BitConverter.ToSingle(raw, 0);
            }
            set
            {
                byte[] raw = BitConverter.GetBytes(value);
                for (int i = 0; i < raw.Length; i++)
                {
                    raw[i] ^= xorKey;
                }
                encValue = BitConverter.ToUInt32(raw, 0);
                initFlag = 1;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is _Float) && ((_Float)obj).Value == Value;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static implicit operator float(_Float s)
        {
            return s.Value;
        }

        public static implicit operator _Float(sbyte v)
        {
            return new _Float(v);
        }

        public static implicit operator _Float(byte v)
        {
            return new _Float(v);
        }

        public static implicit operator _Float(short v)
        {
            return new _Float(v);
        }

        public static implicit operator _Float(ushort v)
        {
            return new _Float(v);
        }

        public static implicit operator _Float(int v)
        {
            return new _Float(v);
        }

        public static implicit operator _Float(uint v)
        {
            return new _Float(v);
        }

        public static implicit operator _Float(float v)
        {
            return new _Float(v);
        }

        public static bool operator ==(_Float a, _Float b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(_Float a, _Float b)
        {
            return a.Value != b.Value;
        }

        public static _Float operator ++(_Float s)
        {
            return new _Float(s.Value + 1);
        }

        public static _Float operator --(_Float s)
        {
            return new _Float(s.Value - 1);
        }
    }
}
