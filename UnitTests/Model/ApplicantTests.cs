using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Carmen.ShowModel;
using Carmen.ShowModel.Applicants;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Carmen.ShowModel.Structure;

namespace UnitTests.Model
{
    public class ApplicantTests
    {
        readonly List<Applicant> Applicants = new List<Applicant>();
        readonly ShowRoot showRoot = new ShowRoot
        {
            ShowDate = new DateTime(2021, 1, 1)
        };

        [OneTimeSetUp]
        public void CreateTestData()
        {
            Applicants.Clear();

            var john = new Applicant
            {
                FirstName = "John",
                LastName = "Smith",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(1990, 1, 1),
                ShowRoot = showRoot
            };

            var jane = new Applicant
            {
                FirstName = "Jane",
                LastName = "Doe",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(2000, 1, 1),
                ShowRoot = showRoot
            };

            Applicants.Add(john);
            Applicants.Add(jane);
        }

        [Test]
        public void Positive_Ages()
        {
            Applicants.All(a => a.Age > 0).Should().BeTrue();
        }

        [Test]
        public void Negative_Ages()
        {
            var invalid = new Applicant
            {
                FirstName = "Not-Bjorn",
                LastName = "Yet",
                Gender = Gender.Male,
                DateOfBirth = new DateTime(3000, 1, 1),
                ShowRoot = showRoot
            };
            try
            {
                _ = invalid.Age;
                Assert.Fail();
            }
            catch { }
        }

        [Test]
        public void PropertyChanged_DateOfBirth()
        {
            var applicant = new Applicant();
            using var mon = applicant.Monitor();
            applicant.DateOfBirth = new DateTime(1990, 1, 1);
            mon.Should().RaisePropertyChangeFor(a => a.DateOfBirth);
            mon.Clear();
            applicant.DateOfBirth = new DateTime(1990, 1, 1);
            mon.Should().NotRaisePropertyChangeFor(a => a.DateOfBirth);
        }

        [Test]
        public void PropertyChanged_DateOfBirthToAgeAndDescription()
        {
            var applicant = new Applicant();
            using var mon = applicant.Monitor();
            applicant.DateOfBirth = new DateTime(1990, 1, 1);
            mon.Should().RaisePropertyChangeFor(a => a.DateOfBirth);
            mon.Should().RaisePropertyChangeFor(a => a.Age);
            mon.Should().RaisePropertyChangeFor(a => a.Description);
            mon.Clear();
            applicant.DateOfBirth = new DateTime(1990, 1, 1);
            mon.Should().NotRaisePropertyChangeFor(a => a.DateOfBirth);
            mon.Should().NotRaisePropertyChangeFor(a => a.Age);
            mon.Should().NotRaisePropertyChangeFor(a => a.Description);
        }

        [Test]
        public void PropertyChanged_Gender()
        {
            var applicant = new Applicant();
            using var mon = applicant.Monitor();
            applicant.Gender = Gender.Male;
            mon.Should().RaisePropertyChangeFor(a => a.Gender);
            mon.Clear();
            applicant.Gender = Gender.Male;
            mon.Should().NotRaisePropertyChangeFor(a => a.Gender);
        }

        [Test]
        public void PropertyChanged_GenderToDescription()
        {
            var applicant = new Applicant();
            using var mon = applicant.Monitor();
            applicant.Gender = Gender.Male;
            mon.Should().RaisePropertyChangeFor(a => a.Gender);
            mon.Should().RaisePropertyChangeFor(a => a.Description);
            mon.Clear();
            applicant.Gender = Gender.Male;
            mon.Should().NotRaisePropertyChangeFor(a => a.Gender);
            mon.Should().NotRaisePropertyChangeFor(a => a.Description);
        }

        [Test]
        public void PropertyChanged_Abilities()
        {
            var applicant = new Applicant();
            var ability = new Ability();
            using var mon = applicant.Monitor();
            applicant.Abilities.Add(ability);
            mon.Should().RaisePropertyChangeFor(a => a.Abilities);
            mon.Clear();
            applicant.Abilities.Remove(ability);
            mon.Should().RaisePropertyChangeFor(a => a.Abilities);
        }

        [Test]
        public void PropertyChanged_AbilityToAbilities()
        {
            var applicant = new Applicant();
            var ability = AbilityTests.TestAbility();
            applicant.Abilities.Add(ability);
            using var mon = applicant.Monitor();
            ability.Mark = 100;
            mon.Should().RaisePropertyChangeFor(a => a.Abilities);
            mon.Clear();
            ability.Mark = 100;
            mon.Should().NotRaisePropertyChangeFor(a => a.Abilities);
            applicant.Abilities.Remove(ability);
            mon.Should().RaisePropertyChangeFor(a => a.Abilities);
            mon.Clear();
            ability.Mark = 50;
            mon.Should().NotRaisePropertyChangeFor(a => a.Abilities);
        }

        [Test]
        public void Age_AtShow_DefaultsTo_Today()
        {
            var show_root = new ShowRoot();
            var applicant = new Applicant
            {
                DateOfBirth = new DateTime(2010, 1, 1),
                ShowRoot = show_root
            };
            var age_today = applicant.AgeAt(DateTime.Now);
            age_today.Should().BeGreaterThan(10);
            applicant.Age.Should().Be(age_today);
            show_root.ShowDate = new DateTime(2014, 1, 1);
            applicant.Age.Should().Be(4);
        }
    }
}