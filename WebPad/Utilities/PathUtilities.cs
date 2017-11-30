using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebPad.Utilities
{
    public static class PathUtilities
    {

        // original from: https://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }



        public static string MakeAbsolutePath(string full, string relative)
        {
            if (String.IsNullOrEmpty(full)) throw new ArgumentNullException("full");
            if (String.IsNullOrEmpty(relative)) throw new ArgumentNullException("relative");

            string dirPath = System.IO.Path.GetDirectoryName(full) + "\\";
            string result = System.IO.Path.GetFullPath(dirPath + relative);

            return result;
        }



    }
}
