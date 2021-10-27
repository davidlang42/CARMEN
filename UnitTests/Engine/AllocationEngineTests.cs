using Carmen.CastingEngine.Allocation;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Engine
{
    public class AllocationEngineTests
    {
        [Test]
        public void ImplementationsList_ContainsCorrectTypes()
        {
            foreach (var type in AllocationEngine.Implementations)
                type.IsAssignableTo(typeof(AllocationEngine)).Should().BeTrue();
        }
    }
}
