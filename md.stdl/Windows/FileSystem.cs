using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace md.stdl.Windows
{
    /// <summary>
    /// Collection of file system related methods
    /// </summary>
    public static class FileSystem
    {
        [DllImport("shlwapi.dll", SetLastError = true)]
        private static extern int PathRelativePathTo(StringBuilder pszPath, string pszFrom, int dwAttrFrom, string pszTo, int dwAttrTo);

        /// <summary>
        /// Returns the relative path of an absolute path relative to the basepath
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="absPath"></param>
        /// <param name="defaultBaseAttr"></param>
        /// <param name="defaultAbsAttr"></param>
        /// <returns></returns>
        public static string GetRelativePath(
            string basePath,
            string absPath,
            int defaultBaseAttr = FILE_ATTRIBUTE_NORMAL,
            int defaultAbsAttr = FILE_ATTRIBUTE_NORMAL)
        {
            int fromAttr = GetPathAttribute(basePath, defaultBaseAttr);
            int toAttr = GetPathAttribute(absPath, defaultAbsAttr);

            StringBuilder path = new StringBuilder(260); // MAX_PATH
            if (PathRelativePathTo(
                    path,
                    basePath,
                    fromAttr,
                    absPath,
                    toAttr) == 0)
            {
                throw new ArgumentException("Paths must have a common prefix");
            }
            return path.ToString();
        }

        private static int GetPathAttribute(string path, int defaultAttr)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists)
            {
                return FILE_ATTRIBUTE_DIRECTORY;
            }

            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                return FILE_ATTRIBUTE_NORMAL;
            }

            return defaultAttr;
        }

        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;
    }
}
