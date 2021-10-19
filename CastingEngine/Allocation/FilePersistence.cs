using System.IO;

namespace Carmen.CastingEngine.Allocation
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
