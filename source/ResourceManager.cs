using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WebServer
{
    class ResourceManager
    {
        private string path = "D:\\webserver\\public_html\\";
        private char localSlashCharacter = '\\';
        private char URISlashCharacter = '/';
        private string[] supportedExtensions = new[]
        {
            ".html",
            ".htm",
            ".png",
            ".css"
        };

        private Dictionary<string, FileInfo> loadedFiles = new Dictionary<string, FileInfo>();

        /// <summary>
        /// Load all files with supportedExtensions into the dictionary
        /// </summary>
        public void loadResources()
        {
            Logging.logMessage(Logging.MessageType.Debug, "Loading resources from " + path);

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] files = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)
                                        .Where(file => supportedExtensions.Contains(file.Extension.ToLower()))
                                        .ToArray();

            foreach (FileInfo fileInfo in files) {
                string relativePath = fileInfo.FullName.Substring(path.Length - 1);

                Logging.logMessage(Logging.MessageType.Debug, "Loaded " + relativePath);
                loadedFiles.Add(relativePath, fileInfo);
            }
        }

        /// <summary>
        /// Tries to open and return the file given. 
        /// Returns null if files doesn't exist
        /// </summary>
        /// <param name="relativePath">The path relative to the sever root</param>
        /// <returns>Byte array of the files data</returns>
        public Byte[] getFile(string relativePath)
        {
            relativePath = relativePath.Replace("/", "\\"); //Swap slashes since windows uses \ for directories

            FileInfo file = null;
            if (!loadedFiles.TryGetValue(relativePath, out file)) { //If file isn't in dictionary
                try {
                    file = new FileInfo(relativePath); //Try to open file (could have been added after server start)
                    if (!file.Exists) {
                        return null;
                    }
                    Logging.logMessage(Logging.MessageType.Debug, "Loaded " + relativePath);
                    loadedFiles.Add(relativePath, file); //Found file, add to dictionary
                }

                catch {
                    return null;
                }
            }

            /*Load file into byte array and return*/
            FileStream fileStream = file.OpenRead();
            Byte[] buffer = new Byte[fileStream.Length];
            fileStream.Read(buffer, 0, (int)fileStream.Length);
            fileStream.Close();
            return buffer;
        }
    }
}
