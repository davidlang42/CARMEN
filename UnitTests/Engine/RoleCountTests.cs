using Carmen.CastingEngine.Allocation;
using Carmen.CastingEngine.Audition;
using Carmen.ShowModel.Applicants;
using Carmen.ShowModel.Criterias;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.Engine
{
    class RoleCountTests
    {
        Criteria a, b, c;
        AndRequirement AandBandC, AandB, BandC;
        OrRequirement AorBorC, AorB, BorC;
        XorRequirement AxorBxorC, AxorB, BxorC;
        AbilityRangeRequirement req_a, req_b, req_c;
        NotRequirement notA;

        WeightedAverageEngine engine;

        public RoleCountTests()
        {
            a = new NumericCriteria { Name = "A" };
            b = new NumericCriteria { Name = "B" };
            c = new NumericCriteria { Name = "C" };
            req_a = new() { Criteria = a };
            req_b = new() { Criteria = b };
            req_c = new() { Criteria = c };
            AandBandC = new() { SubRequirements = { req_a, req_b, req_c } };
            AandB = new() { SubRequirements = { req_a, req_b } };
            BandC = new() { SubRequirements = { req_b, req_c } };
            AorBorC = new() { SubRequirements = { req_a, req_b, req_c } };
            AorB = new() { SubRequirements = { req_a, req_b } };
            BorC = new() { SubRequirements = { req_b, req_c } };
            AxorBxorC = new() { SubRequirements = { req_a, req_b, req_c } };
            AxorB = new() { SubRequirements = { req_a, req_b } };
            BxorC = new() { SubRequirements = { req_b, req_c } };
            notA = new() { SubRequirement = req_a };
            engine = new WeightedAverageEngine(new WeightedSumEngine(Array.Empty<Criteria>()), Array.Empty<AlternativeCast>(), new ShowRoot());
        }

        private double TestWeight(Criteria criteria, params Requirement[] requirements)
        {
            var role = new Role();
            foreach (var requirement in requirements)
                role.Requirements.Add(requirement);
            var applicant = new Applicant()
            {
                Roles = { role }
            };
            return engine.CountRoles(applicant, criteria, null);
        }

        [Test]
        public void RoleCounts_IgnoreNOTRequirement()
        {
            TestWeight(a, notA, req_b).Should().Be(0);
            TestWeight(b, notA, req_b).Should().Be(1);
        }

        [Test]
        public void RoleCounts_AverageORRequirements()
        {
            TestWeight(a, AorB).Should().Be(0.5);
            TestWeight(a, AorBorC).Should().Be(1.0 / 3);
        }

        [Test]
        public void RoleCounts_AverageXORRequirements()
        {
            TestWeight(a, AxorB).Should().Be(0.5);
            TestWeight(a, AxorBxorC).Should().Be(1.0 / 3);
        }

        [Test]
        public void RoleCounts_GeometricMeanANDRequirements()
        {
            TestWeight(a, AandB).Should().Be(1 / Math.Sqrt(2));
            TestWeight(a, AandBandC).Should().Be(1 / Math.Sqrt(3));
        }

        [Test]
        public void RoleCounts_XORs()
        {
            TestWeight(a, AxorB, BxorC).Should().Be(0.5 / Math.Sqrt(1.5));
            TestWeight(b, AxorB, BxorC).Should().Be(0.5 / Math.Sqrt(1.5));
        }

        [Test]
        public void RoleCounts_ANDofORs()
        {
            TestWeight(a, new AndRequirement { SubRequirements = { AorB, BorC } }).Should().Be(0.5 / Math.Sqrt(1.5));
            TestWeight(b, new AndRequirement { SubRequirements = { AorB, BorC } }).Should().Be(0.5 / Math.Sqrt(1.5));
        }

        [Test]
        public void RoleCounts_ORofANDs()
        {
            TestWeight(a, new OrRequirement { SubRequirements = { AandB, BandC } }).Should().Be(1.0 / 3);
            TestWeight(b, new OrRequirement { SubRequirements = { AandB, BandC } }).Should().Be(1.0 / 3);
        }

        [Test]
        public void RoleCounts_ANDOnlyCountsAs1()
        {
            TestWeight(b, AandB, BandC).Should().Be(1 / (Math.Sqrt(3)));
            TestWeight(b, new AndRequirement { SubRequirements = { AandB, BandC } }).Should().Be(1 / Math.Sqrt(3));
        }

        [Test]
        public void RoleCounts_KeepsThe1()
        {
            TestWeight(a, AandB, req_b).Should().Be(1 / Math.Sqrt(2));
            TestWeight(b, AandB, req_b).Should().Be(1 / Math.Sqrt(2));
            TestWeight(a, AorB, req_b).Should().Be(0.5 / Math.Sqrt(1.5));
            TestWeight(b, AorB, req_b).Should().Be(1 / Math.Sqrt(1.5));
        }

        [Test]
        public void RoleCounts_ANDKeepsTheMax()
        {
            TestWeight(a, new AndRequirement { SubRequirements = { AandB, BorC } }).Should().Be(1 / Math.Sqrt(2.5));
            TestWeight(b, new AndRequirement { SubRequirements = { AandB, BorC } }).Should().Be(1 / Math.Sqrt(2.5));
            TestWeight(c, new AndRequirement { SubRequirements = { AandB, BorC } }).Should().Be(0.5 / Math.Sqrt(2.5));
        }

        [Test]
        public void RoleCounts_ORKeepsTheMax()
        {
            TestWeight(a, new OrRequirement { SubRequirements = { req_b, BorC } }).Should().Be(0);
            TestWeight(b, new OrRequirement { SubRequirements = { req_b, BorC } }).Should().Be(1 / 1.5);
            TestWeight(c, new OrRequirement { SubRequirements = { req_b, BorC } }).Should().Be(0.5 / 1.5);
        }
    }
}
