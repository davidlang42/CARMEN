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

        public StreamReader Load() => new StreamReader(fileName);

        public StreamWriter Save() => new StreamWriter(fileName);
    }
}
