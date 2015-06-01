using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindDuplicateFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            var dupTable = new DuplicateFileInfo(@"C:\Users\reidl\SkyDrive\Music");
            dupTable.Start();
        }
    }
}
