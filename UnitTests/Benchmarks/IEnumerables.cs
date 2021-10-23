using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Benchmarks
{
#if !BENCHMARK
    [Ignore("Used only for Benchmarking")]
#endif
    class IEnumerables
    {
        [Test]
        public void NullCheck_Vs_OfType_ReferenceType()
        {
            const int COUNT = 40000000; // at this count, NullCheck is ~1.3s faster than OfType
            const double NULL_PROBABILITY = 0.5; // same count of nulls as boxes
            var r = new Random(1);
            var array = Enumerable.Range(0, COUNT).Select(i => r.NextDouble() < NULL_PROBABILITY ? null : new Box(r.Next())).ToArray();
            _ = array.Select(b => b?.Value - 1).ToArray();
            var start = DateTime.Now;
            var values_nc = array.Where(b => b != null).Cast<Box>().ToArray();
            var time_nc = (DateTime.Now - start).TotalSeconds;
            start = DateTime.Now;
            var values_ot = array.OfType<Box>().ToArray();
            var time_ot = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine($"Null check: {time_nc:0.000}s");
            Console.WriteLine($"OfType: {time_ot:0.000}s");
            values_nc.Length.Should().Be(values_ot.Length);
            var null_ratio = ((double)COUNT - values_nc.Length) / COUNT;
            null_ratio.Should().BeApproximately(NULL_PROBABILITY, 0.01);
        }

        [Test]
        public void NullCheck_Vs_OfType_ValueType()
        {
            const int COUNT = 40000000; // at this count, NullCheck is ~1.1s faster than OfType
            const double NULL_PROBABILITY = 0.5; // same count of nulls as ints
            var r = new Random(1);
            var array = Enumerable.Range(0, COUNT).Select<int, int?>(i => r.NextDouble() < NULL_PROBABILITY ? null : r.Next()).ToArray();
            _ = array.Select(i => i - 1).ToArray();
            var start = DateTime.Now;
            var values_nc = array.Where(i => i != null).Cast<int>().ToArray();
            var time_nc = (DateTime.Now - start).TotalSeconds;
            start = DateTime.Now;
            var values_ot = array.OfType<int>().ToArray();
            var time_ot = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine($"Null check: {time_nc:0.000}s");
            Console.WriteLine($"OfType: {time_ot:0.000}s");
            values_nc.Length.Should().Be(values_ot.Length);
            var null_ratio = ((double)COUNT - values_nc.Length) / COUNT;
            null_ratio.Should().BeApproximately(NULL_PROBABILITY, 0.01);
        }

        private class Box
        {
            public int Value;
            public Box(int value)
            {
                Value = value;
            }
        }
    }
}
