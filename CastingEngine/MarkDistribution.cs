using ShowModel.Applicants;
using ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CastingEngine
{
    public struct MarkDistribution
    {
        public uint Min;
        public uint Max;
        public double Mean;
        public double Median;
        public double StandardDeviation;

        public static MarkDistribution Analyse(IEnumerable<uint> marks)
        {
            var result = new MarkDistribution();
            result.Mean = marks.Mean(out result.Min, out result.Max, out result.Median);
            result.StandardDeviation = Math.Sqrt(marks.Select(m => Math.Pow(m - result.Mean, 2)).Average());
            return result;
        }

        public static MarkDistribution Analyse(IEnumerable<Applicant> applicants, Criteria criteria)
            => Analyse(applicants.Select(a => a.MarkFor(criteria)));
    }
}
