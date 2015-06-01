using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DirectorySizes
{
    public class dirData
    {
        public string dirName { get; private set; }
        public double size { get; private set; }
        public bool isDir { get; private set; }

        public dirData(string name, double sz, bool dir)
        {
            dirName = name;
            size = sz;
            isDir = dir;
        }
    }
}
