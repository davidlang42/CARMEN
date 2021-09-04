using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class StackTests
    {
        [Test]
        public void Stack_Pushes_LastIn_FirstOut()
        {
            var values = new[] { 1, 2, 3 };
            var stack = new Stack<int>();
            foreach (var value in values)
                stack.Push(value);
            foreach (var value in values.Reverse())
                stack.Pop().Should().Be(value);
        }

        /// <summary>To validate the behaviour of a stack initialised with some values</summary>
        [Test]
        public void Stack_Initialises_LastSequence_FirstOut()
        {
            var values = new[] { 1, 2, 3 };
            var stack = new Stack<int>(values.Reverse());
            foreach (var value in values)
                stack.Pop().Should().Be(value);
        }
    }
}
