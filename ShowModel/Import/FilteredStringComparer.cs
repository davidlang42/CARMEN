using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carmen.ShowModel.Import
{
    public class FilteredStringComparer : IEqualityComparer<string>
    {
        bool[] allowChar;
        StringComparison comparisonType;

        public FilteredStringComparer(Func<char, bool> char_allow_function, StringComparison comparison_type)
        {
            allowChar = new bool[char.MaxValue + 1];
            for (int i = char.MinValue; i <= char.MaxValue; i++)
                allowChar[i] = char_allow_function((char)i);
            comparisonType = comparison_type;
        }

        private string Filter(string input) => new string(input.Where(c => allowChar[c]).ToArray());

        public bool Equals(string? x, string? y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            return Filter(x).Equals(Filter(y), comparisonType);
        }

        public int GetHashCode([DisallowNull] string obj) => Filter(obj).GetHashCode(comparisonType);
    }
}
