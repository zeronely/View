using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spate
{
    public static class FileUtil
    {
        public static void EnsureFile(string file)
        {
            if (!File.Exists(file))
            {
                string folder = Path.GetDirectoryName(file);
                EnsureFolder(folder);
                File.Create(file).Close();
            }
        }

        public static void EnsureFileParent(string file)
        {
            string folder = Path.GetDirectoryName(file);
            EnsureFolder(folder);
        }

        public static void EnsureFolder(string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }

        public static void DeleteFolder(string folder, bool deleteSelf)
        {
            string[] files = Directory.GetFiles(folder);
            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i]);
            }
            string[] dirs = Directory.GetDirectories(folder);
            for (int i = 0; i < dirs.Length; i++)
            {
                DeleteFolder(dirs[i], true);
            }
            if (deleteSelf)
                Directory.Delete(folder);
        }
    }
}
