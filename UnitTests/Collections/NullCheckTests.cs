using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Collections
{
    class NullCheckTests
    {
        [Test]
        [TestCase(1000000)]
        //[TestCase(40000000)] // at this count, NullCheck is ~1.3s faster than OfType
        public void NullCheck_Vs_OfType_ReferenceType(int count)
        {
            const double NULL_PROBABILITY = 0.5; // same count of nulls as boxes
            var r = new Random(1);
            var array = Enumerable.Range(0, count).Select(i => r.NextDouble() < NULL_PROBABILITY ? null : new Box(r.Next())).ToArray();
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
            var null_ratio = ((double)count - values_nc.Length) / count;
            null_ratio.Should().BeApproximately(NULL_PROBABILITY, 0.01);
            // assert result has not changed since this was written
            time_nc.Should().BeLessThan(time_ot);
        }

        [Test]
        [TestCase(1000000)]
        //[TestCase(40000000)] // at this count, NullCheck is ~1.1s faster than OfType
        public void NullCheck_Vs_OfType_ValueType(int count)
        {
            const double NULL_PROBABILITY = 0.5; // same count of nulls as ints
            var r = new Random(1);
            var array = Enumerable.Range(0, count).Select<int, int?>(i => r.NextDouble() < NULL_PROBABILITY ? null : r.Next()).ToArray();
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
            var null_ratio = ((double)count - values_nc.Length) / count;
            null_ratio.Should().BeApproximately(NULL_PROBABILITY, 0.01);
            // assert result has not changed since this was written
            time_nc.Should().BeLessThan(time_ot);
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
