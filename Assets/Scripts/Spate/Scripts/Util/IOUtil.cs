using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Spate
{
    public static class IOUtil
    {
        public static string ReadString(Stream input)
        {
            return ReadString(input, Encoding.UTF8);
        }

        public static string ReadString(Stream input, Encoding encode)
        {
            byte[] raw = new byte[4];
            if (input.Read(raw, 0, raw.Length) != raw.Length)
                throw new EndOfStreamException();
            int length = BitConverter.ToInt32(raw, 0);
            raw = new byte[length];
            if (input.Read(raw, 0, length) != length)
                throw new EndOfStreamException();
            return encode.GetString(raw);
        }

        public static void WriteString(string text, Stream output)
        {
            WriteString(text, output, Encoding.UTF8);
        }

        public static void WriteString(string text, Stream output, Encoding encode)
        {
            byte[] buff = encode.GetBytes(text);
            byte[] lenRaw = BitConverter.GetBytes(buff.Length);
            output.Write(lenRaw, 0, lenRaw.Length);
            output.Write(buff, 0, buff.Length);
            buff = null;
        }
    }
}
