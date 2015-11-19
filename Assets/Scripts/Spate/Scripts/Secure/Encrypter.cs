using System;
using System.Text;

public static class Encrypter
{
    public static void EncodeXor(byte[] buffer)
    {
        byte[] KEYS = Encoding.UTF8.GetBytes("supreme@ffly");
        int keyIndex = 0;
        int keyIndexMax = KEYS.Length;
        for (int i = 0, len = buffer.Length; i < len; i++)
        {
            buffer[i] ^= KEYS[keyIndex == keyIndexMax ? keyIndex = 0 : keyIndex++];
        }
    }
}
