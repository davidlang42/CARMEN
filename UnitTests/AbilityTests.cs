using FluentAssertions;
using NUnit.Framework;
using ShowModel.Applicants;
using ShowModel.Criterias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class AbilityTests
    {
        [Test]
        public void PropertyChanged_Applicant()
        {
            var ability = new Ability();
            var applicant = new Applicant();
            using var mon = ability.Monitor();
            ability.Applicant = applicant;
            mon.Should().RaisePropertyChangeFor(ac => ac.Applicant);
            mon.Clear();
            ability.Applicant = applicant;
            mon.Should().NotRaisePropertyChangeFor(ac => ac.Applicant);
        }

        [Test]
        public void PropertyChanged_Criteria()
        {
            var ability = new Ability();
            var criteria = new NumericCriteria();
            using var mon = ability.Monitor();
            ability.Criteria = criteria;
            mon.Should().RaisePropertyChangeFor(ac => ac.Criteria);
            mon.Clear();
            ability.Criteria = criteria;
            mon.Should().NotRaisePropertyChangeFor(ac => ac.Criteria);
        }

        [Test]
        public void PropertyChanged_Mark()
        {
            var ability = TestAbility();
            using var mon = ability.Monitor();
            ability.Mark = 50;
            mon.Should().RaisePropertyChangeFor(ab => ab.Mark);
            mon.Clear();
            ability.Mark = 50;
            mon.Should().NotRaisePropertyChangeFor(ab => ab.Mark);
        }

        [Test]
        public void MarkGreaterThanMaxMark()
        {
            var ability = TestAbility();
            ability.Mark = 100;
            ability.Mark.Should().Be(100);
            ability.Mark = 50;
            ability.Mark.Should().Be(50);
            ability.Mark = 101;
            ability.Mark.Should().Be(100);
        }

        public static Ability TestAbility()
        {
            var criteria = new NumericCriteria
            {
                MaxMark = 100
            };
            return new Ability
            {
                Criteria = criteria
            };
        }
    }
}
