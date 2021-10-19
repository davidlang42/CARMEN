using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Carmen.CastingEngine.Audition
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
            result.Mean = marks.OrderBy(m => m).Mean(out result.Min, out result.Max, out result.Median);
            result.StandardDeviation = Math.Sqrt(marks.Select(m => Math.Pow(m - result.Mean, 2)).Average());
            return result;
        }

        public static MarkDistribution Analyse(IEnumerable<Applicant> applicants, Criteria criteria)
            => Analyse(applicants.Select(a => a.MarkFor(criteria)));

        public override string ToString() => $"Range: {Min}-{Max}, Mean: {Mean:0}, Median: {Median:0}, Std-Dev: {StandardDeviation:0.0}";
    }
}
