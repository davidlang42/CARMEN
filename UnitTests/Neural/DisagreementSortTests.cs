using Carmen.CastingEngine.Allocation;
using Carmen.CastingEngine.Neural;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Neural
{
    class DisagreementSortTests
    {
        [Test]
        [TestCase(10, 1)]

        public void SimpleIntegers(int n_max, int seed)
        {
            var (better_than_order, better_than_lies) = TestIntegers(n_max, seed);
            better_than_order.Should().BeTrue();
            better_than_lies.Should().BeTrue();
        }

        [TestCase(10, 1, 1000)]
        [Test]
        public void Many_SimpleIntegers(int n_max, int seed_start, int seed_end)
        {
            int success_order = 0;
            int success_lies = 0;
            int total = 0;
            for (var seed = seed_start; seed <= seed_end; seed++)
            {
                var (better_than_order, better_than_lies) = TestIntegers(n_max, seed);
                total++;
                if (better_than_order)
                    success_order++;
                if (better_than_lies)
                    success_lies++;
            }
            var percent_order = 100.0 * success_order / total;
            var percent_lies = 100.0 * success_lies / total;
            Console.WriteLine($"---------- Better Than Order: {percent_order:0.0}%, Better Than Lies: {percent_lies:0.0}%,  ----------");
            percent_order.Should().BeGreaterOrEqualTo(90);
            percent_lies.Should().BeGreaterOrEqualTo(99);
        }

        private (bool BetterThanOrder, bool BetterThanLies) TestIntegers(int n_max, int seed)
        {
            Console.WriteLine($"-------------------- N_MAX: {n_max}, SEED: {seed} --------------------");

            // Generate values in a random order
            var real_order = Enumerable.Range(1, n_max).Select(n => new Value(n)).ToArray();
            var random_order = RandomOrder(real_order, seed);
            Console.WriteLine($"Random: {string.Join(", ", random_order.Select(i => i.ToString()))}");

            // Try sorting with a lying comparer
            var comparer = new LyingComparer<Value>(Value.Compare, seed)
            {
                LyingFrequency = n_max,
                MaxLies = n_max
            };
            var sorter = new DisagreementSort<Value>(comparer);
            var sorted_values = sorter.Sort(random_order).ToArray();
            var sorted_errors = CountErrors(sorted_values);
            var sorted_rank = RankErrors(sorted_values, real_order);
            Console.WriteLine($"Disagreement Sorted: {string.Join(", ", sorted_values.Select(i => i.ToString()))} ({sorted_errors} errors)");

            // Also try sorting them OrderBy(), using the same lies as the DisagreementSort received
            var ordered_values = random_order.OrderBy(v => v, sorter).ToArray();
            var ordered_errors = CountErrors(ordered_values);
            var ordered_rank = RankErrors(ordered_values, real_order);
            Console.WriteLine($"OrderBy Sorted: {string.Join(", ", ordered_values.Select(i => i.ToString()))} ({ordered_errors} errors)");

            // Return result
            return (sorted_rank <= ordered_rank || sorted_errors <= ordered_errors, sorted_errors <= comparer.Lies);
        }

        private int CountErrors(Value[] values)
        {
            var errors = 0;
            for (var v = 1; v < values.Length; v++)
                if (values[v].Number < values[v - 1].Number)
                    errors++;
            return errors;
        }

        private int RankErrors(Value[] values, Value[] real_order)
        {
            var real_lookup = new Dictionary<Value, int>();
            for (var i = 0; i < real_order.Length; i++)
                real_lookup.Add(real_order[i], i);
            var errors = 0;
            for (var v = 0; v < values.Length; v++)
                errors += Math.Abs(real_lookup[values[v]] - v);
            return errors;
        }

        private Value[] RandomOrder(IEnumerable<Value> values, int random_seed)
        {
            var random = new Random(random_seed);
            return values.OrderBy(v => random.Next()).ToArray();
        }
    }

    class Value
    {
        public int Number;

        public Value(int number)
        {
            this.Number = number;
        }

        public override string ToString() => Number.ToString();

        public static int Compare(Value? x, Value? y)
        {
            if (x == null || y == null)
                throw new ArgumentNullException();
            return x.Number.CompareTo(y.Number);
        }
    }

    class LyingComparer<T> : IComparer<T>
    {
        Func<T?, T?, int> realCompare;
        Random random;

        /// <summary>If greater than 0, the comparer will return the wrong result 1 in every
        /// <see cref="LyingFrequency"/> times it is called. If set to 0 or less, it will never lie.</summary>
        public int LyingFrequency { get; set; } = 10;

        /// <summary>The maximum number of times this comparer will lie.</summary>
        public int MaxLies { get; set; } = 10;

        /// <summary>The count of lies, incremented every time <see cref="Compare(T?, T?)"/> is called.</summary>
        public int Lies { get; set; } = 0;

        public LyingComparer(IComparer<T> real_comparer, int random_seed)
            : this(real_comparer.Compare, random_seed)
        { }

        public LyingComparer(Func<T?, T?, int> real_compare, int random_seed)
        {
            realCompare = real_compare;
            random = new Random(random_seed);
        }

        public int Compare(T? x, T? y)
        {
            var result = realCompare(x, y);
            if (LyingFrequency > 0 && random.Next(LyingFrequency) == 0 && Lies < MaxLies)
            {
                Lies++;
                result *= -1;
                Console.WriteLine($"Lying Compare: ({x}, {y}) => {result}");
            }
            return result;
        }
    }
}
