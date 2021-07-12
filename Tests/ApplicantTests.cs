using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Applicants;
using NUnit.Framework;
using System;
using System.Linq;

namespace Tests
{
    public class ApplicantTests
    {
        readonly DbContextOptions<ShowContext> contextOptions = new DbContextOptionsBuilder<ShowContext>()
            .UseSqlite($"Filename={nameof(ApplicantTests)}.db").Options;

        [SetUp]
        public void Setup()
        {
            using var context = new ShowContext(contextOptions);

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

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

            context.Applicants.Add(john);
            context.Applicants.Add(jane);

            context.SaveChanges();
        }

        [Test]
        public void Applicants_Exist()
        {
            using var context = new ShowContext(contextOptions);
            context.Applicants.Count().Should().Be(2);
        }

        [Test]
        public void Positive_Ages()
        {
            using var context = new ShowContext(contextOptions);
            context.Applicants.ToList().All(a => a.AgeToday() > 0).Should().BeTrue();
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