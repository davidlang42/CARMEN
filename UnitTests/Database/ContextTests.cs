using FluentAssertions;
using Carmen.ShowModel;
using Carmen.ShowModel.Requirements;
using Carmen.ShowModel.Structure;
using NUnit.Framework;
using System.Linq;
using System;
using EF = Microsoft.EntityFrameworkCore;

namespace UnitTests.Database
{
    public class ContextTests
    {
        readonly ShowConnection connection = ShowConnection.FromLocalFile($"{nameof(ContextTests)}.db");

        [OneTimeSetUp]
        public void CreateDatabase()
        {
            using var context = new ShowContext(connection);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        [Test]
        public void ShowRootAccessorCreatesOnlyOneNode()
        {
            using var context = new ShowContext(connection);
            context.Nodes.Count().Should().Be(0);
            var show_root = context.ShowRoot;
            context.ShowRoot.Should().Be(show_root);
            context.SaveChanges();
            context.Nodes.Count().Should().Be(1);
        }

        [Test]
        public void DataObjectsEnum_MatchesDbSets()
        {
            var enum_names = Enum.GetValues(typeof(DataObjects)).Cast<DataObjects>()
                .Skip(1) // "None"
                .Reverse().Skip(1).Reverse() // "All"
                .Select(e => e.ToString()).ToArray();
            var db_sets = typeof(ShowContext).GetProperties()
                .Where(p => p.PropertyType.Name.StartsWith(nameof(EF.DbSet<object>)))
                .Select(p => p.Name).ToArray();
            enum_names.Should().BeEquivalentTo(db_sets, "because the DataObject enum types should match the ShowContext DbSet properties");
        }

        [Test]
        public void DataObjectsEnum_MatchesPowersOfTwo()
        {
            var enum_values = Enum.GetValues(typeof(DataObjects)).Cast<DataObjects>()
                .Skip(1) // "None"
                .Reverse().Skip(1).Reverse() // "All"
                .Select(e => (int)e).ToArray();
            var expected_values = Enumerable.Range(0, enum_values.Length).Select(i => Math.Pow(2, i)).ToArray();
            enum_values.Should().BeEquivalentTo(expected_values, "because flag enums should match powers of two");
        }

        [Test]
        public void DataObjectsEnum_HasNoneOption()
        {
            var first_value = Enum.GetValues(typeof(DataObjects)).Cast<DataObjects>().First();
            first_value.ToString().Should().Be("None", "because flag enums must have a none option");
            ((int)first_value).Should().Be(0, "because the value of none must be 0");
        }

        [Test]
        public void DataObjectsEnum_HasAllOption()
        {
            var values = Enum.GetValues(typeof(DataObjects)).Cast<DataObjects>().ToList();
            var last_value = values.Last();
            values.Remove(last_value);
            var other_values_sum = values.Cast<int>().Sum();
            last_value.ToString().Should().Be("All", "because its helpful for flag enums to have an all option");
            ((int)last_value).Should().Be(other_values_sum, "because the value of all must be the sum of all others");
        }

        [Test]
        public void CheckDefaultSettings_TrueAfterSetDefault_FalseAfterChange()
        {
            var default_show_name = "DefaultShow";
            using var context = new ShowContext(connection);
            context.SetDefaultShowSettings(default_show_name);
            context.CheckDefaultShowSettings(default_show_name).Should().BeTrue();
            context.ShowRoot.Name = "";
            context.CheckDefaultShowSettings(default_show_name).Should().BeFalse();
        }
    }
}
