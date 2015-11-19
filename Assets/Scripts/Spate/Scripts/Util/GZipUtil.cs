using System;
using System.IO;
using System.Collections;

using ICSharpCode.SharpZipLib.GZip;

public static class GZipUtil
{
    /// <summary>
    /// 判定格式
    /// </summary>
    public static bool IsGZip(byte[] src)
    {
        if (src == null || src.Length < 2) return false;
        return src[0] == 0x1F && src[1] == 0x8B;
    }

    /// <summary>
    /// 压缩
    /// </summary>
    public static byte[] Compression(byte[] src)
    {
        if (IsGZip(src))
            return src;
        MemoryStream ms = new MemoryStream();
        GZipOutputStream gos = new GZipOutputStream(ms);
        gos.Write(src, 0, src.Length);
        gos.Close();
        // 由于从第五个字节开始的4个长度都是表示修改时间,因此可以强行设定
        byte[] result = ms.ToArray();
        result[4] = result[5] = result[6] = result[7] = 0x00;
        return result;
    }

    /// <summary>
    /// 解压缩
    /// </summary>
    public static byte[] Uncompression(byte[] src)
    {
        if (!IsGZip(src)) return src;
        GZipInputStream gis = new GZipInputStream(new MemoryStream(src));
        MemoryStream ms = new MemoryStream();
        int count = 0;
        byte[] tmp = new byte[4096];
        while ((count = gis.Read(tmp, 0, tmp.Length)) > 0)
        {
            ms.Write(tmp, 0, count);
        }
        gis.Close();
        return ms.ToArray();
    }
}
