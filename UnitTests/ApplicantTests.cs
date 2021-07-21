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
            Applicants.All(a => a.AgeToday() > 0).Should().BeTrue();
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
                invalid.AgeToday();
                Assert.Fail();
            }
            catch { }
        }
    }
}