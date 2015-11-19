using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;


namespace Spate
{
    public static class Hasher
    {
        /// <summary>
        /// 计算文件的MD5 Hash值
        /// </summary>
        public static string CalcFileHash(string filePath)
        {
            if (!File.Exists(filePath)) return string.Empty;
            using (Stream stream = File.OpenRead(filePath))
            {
                return CalcHash(stream);
            }
        }

        /// <summary>
        /// 计算字符串的MD5 Hash值
        /// </summary>
        public static string CalcHash(string text)
        {
            return CalcHash(text, Encoding.UTF8);
        }

        /// <summary>
        /// 计算字符串的MD5 Hash值
        /// </summary>
        public static string CalcHash(string text, Encoding encode)
        {
            return CalcHash(encode.GetBytes(text));
        }

        /// <summary>
        /// 计算Bytes的MD5 Hash
        /// </summary>
        public static string CalcHash(byte[] buffer)
        {
            MD5 md = new MD5CryptoServiceProvider();
            byte[] bytes = md.ComputeHash(buffer);
            md.Clear();
            return ToHex(bytes);
        }

        /// <summary>
        /// 计算流的MD5 Hash
        /// </summary>
        public static string CalcHash(Stream stream)
        {
            MD5 md = new MD5CryptoServiceProvider();
            byte[] bytes = md.ComputeHash(stream);
            md.Clear();
            return ToHex(bytes);
        }

        /// <summary>
        /// 将byte[]转换为X2的字符串
        /// </summary>
        public static string ToHex(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("X2"));
            }
            return builder.ToString();
        }
    }
}
