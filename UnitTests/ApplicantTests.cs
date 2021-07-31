using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Applicants;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    public class ApplicantTests
    {
        readonly List<Applicant> Applicants = new List<Applicant>();

        [OneTimeSetUp]
        public void CreateTestData()
        {
            Applicants.Clear();

            var john = new Applicant
            {
                FirstName = "John",
                LastName = "Smith",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            var jane = new Applicant
            {
                FirstName = "Jane",
                LastName = "Doe",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(2000, 1, 1)
            };

            Applicants.Add(john);
            Applicants.Add(jane);
        }

        [Test]
        public void Positive_Ages()
        {
            Applicants.All(a => a.AgeToday > 0).Should().BeTrue();
        }

        [Test]
        public void Negative_Ages()
        {
            var invalid = new Applicant
            {
                FirstName = "Not-Bjorn",
                LastName = "Yet",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(3000, 1, 1)
            };
            try
            {
                _ = invalid.AgeToday;
                Assert.Fail();
            }
            catch { }
        }

        //LATER add other property changed tests for completeness

        [Test]
        public void PropertyChanged_DateOfBirth()
        {
            var applicant = new Applicant();
            using var mon = applicant.Monitor();
            applicant.DateOfBirth = new DateTime(1990, 1, 1);
            mon.Should().RaisePropertyChangeFor(ac => ac.DateOfBirth);
            mon.Clear();
            applicant.DateOfBirth = new DateTime(1990, 1, 1);
            mon.Should().NotRaisePropertyChangeFor(ac => ac.DateOfBirth);
        }

        [Test]
        public void PropertyChanged_DateOfBirthToAge()
        {
            var applicant = new Applicant();
            using var mon = applicant.Monitor();
            applicant.DateOfBirth = new DateTime(1990, 1, 1);
            mon.Should().RaisePropertyChangeFor(ac => ac.DateOfBirth);
            mon.Should().RaisePropertyChangeFor(ac => ac.AgeToday);
            mon.Clear();
            applicant.DateOfBirth = new DateTime(1990, 1, 1);
            mon.Should().NotRaisePropertyChangeFor(ac => ac.DateOfBirth);
            mon.Should().NotRaisePropertyChangeFor(ac => ac.AgeToday);
        }

        [Test]
        public void PropertyChanged_Gender()
        {
            var applicant = new Applicant();
            using var mon = applicant.Monitor();
            applicant.Gender = Gender.Male;
            mon.Should().RaisePropertyChangeFor(ac => ac.Gender);
            mon.Clear();
            applicant.Gender = Gender.Male;
            mon.Should().NotRaisePropertyChangeFor(ac => ac.Gender);
        }

        [Test]
        public void PropertyChanged_Abilities()
        {
            var applicant = new Applicant();
            var ability = new Ability();
            using var mon = applicant.Monitor();
            applicant.Abilities.Add(ability);
            mon.Should().RaisePropertyChangeFor(ac => ac.Abilities);
            mon.Clear();
            applicant.Abilities.Remove(ability);
            mon.Should().RaisePropertyChangeFor(ac => ac.Abilities);
        }

        [Test]
        public void PropertyChanged_AbilityToOverallAbility()
        {
            var applicant = new Applicant();
            var ability = new Ability();
            using var mon = applicant.Monitor();
            applicant.Abilities.Add(ability);
            mon.Should().NotRaisePropertyChangeFor(ac => ac.OverallAbility);
            ability.Mark = 100;
            mon.Should().RaisePropertyChangeFor(ac => ac.OverallAbility);
            mon.Clear();
            ability.Mark = 100;
            mon.Should().NotRaisePropertyChangeFor(ac => ac.OverallAbility);
            applicant.Abilities.Remove(ability);
            ability.Mark = 50;
            mon.Should().NotRaisePropertyChangeFor(ac => ac.OverallAbility);
        }
    }
}