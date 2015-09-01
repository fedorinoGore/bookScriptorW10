using System;
using System.IO;

namespace VersFx.Formats.Text.Epub.Utils
{
    internal static class ZipPathUtils
    {
        public static string GetDirectoryPath(string filePath)
        {
            int lastSlashIndex = filePath.LastIndexOf('/');
            if (lastSlashIndex == -1)
                return String.Empty;
            else
                return filePath.Substring(0, lastSlashIndex);
        }

        public static string Combine(string directory, string fileName)
        {
            if (String.IsNullOrEmpty(directory))
                return fileName;
            else
                return String.Concat(directory, "/", fileName);
        }
        public static string verifyPathName(string dirPath)
        {
            string result = dirPath;
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            foreach (char c in invalid)
            {
                result = result.Replace(c.ToString(), "");
            }
            return result;
        }
    }
}
