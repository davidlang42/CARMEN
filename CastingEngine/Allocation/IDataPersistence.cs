using System.IO;

namespace Carmen.CastingEngine.Allocation
{
    public interface IDataPersistence
    {
        public TextWriter Save();

        public TextReader Load();
    }
}
