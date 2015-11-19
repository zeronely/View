using System;

namespace Spate
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public sealed class FieldOffsetAttribute : Attribute
    {
        private int mOffset = -1;
        public FieldOffsetAttribute(int offset)
        {
            if (offset <= 0)
                throw new Exception("offset value is error!");
            mOffset = offset;
        }

        public int Value
        {
            get
            {
                return mOffset;
            }
        }
    }
}
