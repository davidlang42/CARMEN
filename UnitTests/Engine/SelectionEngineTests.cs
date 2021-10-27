using Carmen.CastingEngine;
using Carmen.CastingEngine.Selection;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Engine
{
    public class SelectionEngineTests
    {
        [Test]
        public void ImplementationsList_ContainsCorrectTypes()
        {
            foreach (var type in SelectionEngine.Implementations)
                type.IsAssignableTo(typeof(SelectionEngine)).Should().BeTrue();
        }
    }
}
