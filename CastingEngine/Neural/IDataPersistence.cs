using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.CastingEngine.Neural
{
    public interface IDataPersistence
    {
        public StreamWriter Save();

        public StreamReader Load();
    }
}
