using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public class FilePersistence : IDataPersistence
    {
        string fileName;

        public FilePersistence(string file_name)
        {
            fileName = file_name;
        }

        public TextReader Load() => new StreamReader(fileName);

        public TextWriter Save() => new StreamWriter(fileName);
    }
}
