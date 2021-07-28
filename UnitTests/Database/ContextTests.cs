﻿using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ShowModel;
using ShowModel.Requirements;
using ShowModel.Structure;
using NUnit.Framework;
using System.Linq;
using System;

namespace UnitTests.Database
{
    public class ContextTests
    {
        [Test]
        public void DataObjectsEnum_MatchesDbSets()
        {
            var enum_names = Enum.GetValues(typeof(DataObjects)).Cast<DataObjects>()
                .Skip(1) // "None"
                .Select(e => e.ToString()).ToArray();
            var db_sets = typeof(ShowContext).GetProperties()
                .Where(p => p.PropertyType.Name.StartsWith(nameof(DbSet<object>)))
                .Select(p => p.Name).ToArray();
            enum_names.Should().BeEquivalentTo(db_sets, "because the DataObject enum types should match the ShowContext DbSet properties");
        }

        [Test]
        public void DataObjectsEnum_MatchesPowersOfTwo()
        {
            var enum_values = Enum.GetValues(typeof(DataObjects)).Cast<DataObjects>()
                .Skip(1) // "None"
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
    }
}