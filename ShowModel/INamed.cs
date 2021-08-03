using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowModel
{
    /// <summary>
    /// An object which has a name
    /// </summary>
    public interface INamed
    {
        string Name { get; }
    }

    public interface INameOrdered : INamed, IComparable
    {
        int IComparable.CompareTo(object? obj) => Name.CompareTo(((INamed?)obj)?.Name);
    }
}
