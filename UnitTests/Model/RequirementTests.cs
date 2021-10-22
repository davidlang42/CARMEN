using FluentAssertions;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Carmen.ShowModel.Requirements;

namespace UnitTests.Model
{
    public class RequirementTests
    {
        [Test]
        public void CircularReference_None()
        {
            // r4 > r2 > r1
            var r1 = new AgeRequirement { Name = "Age" };
            var r2 = new NotRequirement { Name = "Not Age", SubRequirement = r1 };
            var r3 = new GenderRequirement { Name = "Gender" };
            var r4 = new AndRequirement { Name = "Gender And Not Age" };
            r4.SubRequirements.Add(r3);
            r4.SubRequirements.Add(r2);
            r4.Validate().Should().BeEmpty();
        }

        [Test]
        public void CircularReference_Immediate()
        {
            // r1 > r1
            var r1 = new NotRequirement { Name = "Not"};
            r1.SubRequirement = r1;
            r1.Validate().Should().BeEquivalentTo(new[] { "Requirement 'Not' has a circular reference (Not)." });
        }

        [Test]
        public void CircularReference_Delayed()
        {
            // r3 > r1 > r3
            var r1 = new NotRequirement { Name = "Not And" };
            var r2 = new GenderRequirement { Name = "Gender" };
            var r3 = new AndRequirement { Name = "Gender And Not And" };
            r1.SubRequirement = r3;
            r3.SubRequirements.Add(r2);
            r3.SubRequirements.Add(r1);
            r3.Validate().Should().BeEquivalentTo(new[] { "Requirement 'Gender And Not And' has a circular reference (Gender And Not And, Not And)." });
        }

        [Test]
        public void CircularReference_Indirect()
        {
            // r4 > r3 > r2 > r1 > r3
            var r1 = new NotRequirement { Name = "r1" };
            var r2 = new NotRequirement { Name = "r2" };
            var r3 = new NotRequirement { Name = "r3" };
            var r4 = new NotRequirement { Name = "r4" };
            r1.SubRequirement = r3;
            r2.SubRequirement = r1;
            r3.SubRequirement = r2;
            r4.SubRequirement = r3;
            r4.Validate().Should().BeEquivalentTo(new[] { "Requirement 'r4' has a circular reference (r4, r3, r2, r1)." });
        }
    }
}