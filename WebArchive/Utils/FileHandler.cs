using System;
using System.IO;
using System.Linq;
using System.Text;

namespace WebArchive.Utils
{
    public static class FileHandler
    {

        public static string[] GetTextFilesFromDirectory(string path)
        {
            string projectDirectory = Directory.GetCurrentDirectory();
            return Directory.GetFiles(projectDirectory + "/wwwroot/" + path, "*.txt")
                            .Select(Path.GetFileNameWithoutExtension)
                            .ToArray();
        }

    }
}
