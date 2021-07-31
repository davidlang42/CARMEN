using FluentAssertions;
using NUnit.Framework;
using ShowModel.Applicants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class CastGroupTests
    {
        //LATER add property changed tests for completeness: AlternateCasts, Requirements, RequiredCount, Order, Icon

        [Test]
        public void PropertyChanged_Name()
        {
            var cast_group = new CastGroup();
            using var mon = cast_group.Monitor();
            cast_group.Name = "New Name";
            mon.Should().RaisePropertyChangeFor(ac => ac.Name);
            mon.Clear();
            cast_group.Name = "New Name";
            mon.Should().NotRaisePropertyChangeFor(ac => ac.Name);
        }

        [Test]
        public void PropertyChanged_NameToAbbreviation()
        {
            var cast_group = new CastGroup();
            using var mon = cast_group.Monitor();
            cast_group.Name = "New Name";
            mon.Should().RaisePropertyChangeFor(ac => ac.Name);
            mon.Should().RaisePropertyChangeFor(ac => ac.Abbreviation);
            mon.Clear();
            cast_group.Name = "Not New"; // but same abbreviation
            mon.Should().RaisePropertyChangeFor(ac => ac.Name);
            mon.Should().NotRaisePropertyChangeFor(ac => ac.Abbreviation);
        }

        [Test]
        public void PropertyChanged_Initial()
        {
            var cast_group = new CastGroup();
            using var mon = cast_group.Monitor();
            cast_group.Abbreviation = "CG";
            mon.Should().RaisePropertyChangeFor(ac => ac.Abbreviation);
            mon.Clear();
            cast_group.Abbreviation = "CG";
            mon.Should().NotRaisePropertyChangeFor(ac => ac.Abbreviation);
        }

        [Test]
        public void PropertyChanged_Members()
        {
            var cast_group = new CastGroup();
            var applicant = new Applicant();
            using var mon = cast_group.Monitor();
            cast_group.Members.Add(applicant);
            mon.Should().RaisePropertyChangeFor(ac => ac.Members);
            mon.Clear();
            cast_group.Members.Remove(applicant);
            mon.Should().RaisePropertyChangeFor(ac => ac.Members);
        }
    }
}
