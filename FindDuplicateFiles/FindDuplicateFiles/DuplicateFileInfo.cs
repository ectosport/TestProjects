using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindDuplicateFiles
{
    public class DuplicateFileInfo
    {
        private readonly string rootDirectory;
        private readonly Dictionary<string, List<string>> filenameDictionary = new Dictionary<string, List<string>>();

        public DuplicateFileInfo(string dirPath)
        {
            this.rootDirectory = dirPath;
        }

        public void Start()
        {
            DirectoryInfo di = new DirectoryInfo(this.rootDirectory);
            ParseDirectory(di);

            var duplicates = filenameDictionary.Where(pair => pair.Value.Count > 1);
            foreach (var dup in duplicates)
            {
                Console.WriteLine("File: " + dup.Key);
                foreach (var path in dup.Value)
                {
                    Console.WriteLine("   Located: " + path);
                }
            }
        }

        public void ParseDirectory(DirectoryInfo dir)
        {
            long size = 0;
            foreach (FileInfo file in dir.GetFiles())
            {
                if (filenameDictionary.ContainsKey(file.Name))
                {
                    filenameDictionary[file.Name].Add(file.FullName);
                }
                else
                {
                    filenameDictionary.Add(file.Name, new List<string>() { file.FullName });
                }
            }
            foreach (DirectoryInfo directory in dir.GetDirectories())
            {
                ParseDirectory(directory);
            }
        }
    }
}
