using Carmen.CastingEngine.Audition;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Engine
{
    public class AuditionEngineTests
    {
        [Test]
        public void ImplementationsList_ContainsCorrectTypes()
        {
            foreach (var type in AuditionEngine.Implementations)
                type.IsAssignableTo(typeof(AuditionEngine)).Should().BeTrue();
        }
    }
}
