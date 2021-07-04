using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Model;
using NUnit.Framework;
using System;
using System.Linq;

namespace Tests
{
    public class ApplicantTests
    {
        readonly DbContextOptions<ShowContext> contextOptions = new DbContextOptionsBuilder<ShowContext>()
            .UseSqlite("Filename=UnitTests.db").Options;

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
                DateOfBirth = new DateTime(1990, 1, 1),
                ExternalID = "John123"
            };

            var jane = new Applicant
            {
                FirstName = "Jane",
                LastName = "Doe",
                Gender = Gender.Female,
                DateOfBirth = new DateTime(2000, 1, 1)
            };

            var show = new Show
            {
                Name = "Test"
            };

            show.Applicants.Add(john);
            show.Applicants.Add(jane);

            context.Shows.Add(show);

            context.SaveChanges();
        }

        [Test]
        public void Applicants_Exist()
        {
            using var context = new ShowContext(contextOptions);
            context.Shows.First().Applicants.Count().Should().Be(2);
        }

        [Test]
        public void Positive_Ages()
        {
            using var context = new ShowContext(contextOptions);
            context.Shows.First().Applicants.ToList().All(a => a.AgeToday() > 0).Should().BeTrue();
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