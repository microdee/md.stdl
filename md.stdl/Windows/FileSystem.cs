using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
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

        /// <summary>
        /// Intuitively delete a directory and its contents
        /// </summary>
        /// <param name="path"></param>
        /// <param name="recursive"></param>
        public static void DeleteDirectory(string path, bool recursive)
        {
            if (recursive)
            {
                var subfolders = Directory.GetDirectories(path);
                foreach (var s in subfolders)
                {
                    DeleteDirectory(s, recursive);
                }
            }
            
            var files = Directory.GetFiles(path);
            foreach (var f in files)
            {
                var attr = File.GetAttributes(f);
                if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(f, attr ^ FileAttributes.ReadOnly);
                }
                File.Delete(f);
            }
            Directory.Delete(path);
        }

        /// <summary>
        /// Intuitively recursively copy a directory with filters. This is a blocking function
        /// </summary>
        /// <param name="src">Source folder</param>
        /// <param name="dst">Destination folder</param>
        /// <param name="ignore">blacklist files or patterns</param>
        /// <param name="match">whitelist files or patterns</param>
        /// <param name="progress">an optional callback function on progress change</param>
        /// <param name="error">an optional callback function on error</param>
        public static void CopyDirectory(
            string src,
            string dst,
            string[] ignore = null,
            string[] match = null,
            Action<FileSystemInfo> progress = null,
            Action<Exception> error = null)
        {

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(src);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + src);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (match != null)
            {
                dirs = dirs.Where(info =>
                {
                    return match.Any(pattern => new WildcardPattern(pattern).IsMatch(info.Name));
                }).ToArray();
            }
            if (ignore != null)
            {
                dirs = dirs.Where(info =>
                {
                    return !ignore.Any(pattern => new WildcardPattern(pattern).IsMatch(info.Name));
                }).ToArray();
            }
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(dst))
            {
                Directory.CreateDirectory(dst);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();

            if (match != null)
            {
                files = files.Where(info =>
                {
                    return match.Any(pattern => new WildcardPattern(pattern).IsMatch(info.Name));
                }).ToArray();
            }
            if (ignore != null)
            {
                files = files.Where(info =>
                {
                    return !ignore.Any(pattern => new WildcardPattern(pattern).IsMatch(info.Name));
                }).ToArray();
            }

            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(dst, file.Name);
                progress?.Invoke(file);
                try
                {
                    file.CopyTo(temppath, true);
                }
                catch
                {
                    try
                    {
                        if (File.Exists(temppath))
                        {
                            var attrs = File.GetAttributes(temppath);
                            if ((attrs & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                            {
                                attrs = attrs & ~FileAttributes.ReadOnly;
                                File.SetAttributes(temppath, attrs);
                            }
                            if ((attrs & FileAttributes.Hidden) == FileAttributes.Hidden)
                            {
                                attrs = attrs & ~FileAttributes.Hidden;
                                File.SetAttributes(temppath, attrs);
                            }
                            file.CopyTo(temppath, true);
                        }
                    }
                    catch (Exception e)
                    {
                        error?.Invoke(e);
                    }
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(dst, subdir.Name);
                progress?.Invoke(subdir);
                try
                {
                    CopyDirectory(subdir.FullName, temppath, ignore, match, progress);
                }
                catch (Exception e)
                {
                    error?.Invoke(e);
                }
            }
        }
    }
}
