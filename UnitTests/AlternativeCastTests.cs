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
    public class AlternativeCastTests
    {
        [Test]
        public void PropertyChanged_Name()
        {
            var alternative_cast = new AlternativeCast();
            using var mon = alternative_cast.Monitor();
            alternative_cast.Name = "New Name";
            mon.Should().RaisePropertyChangeFor(ac => ac.Name);
            mon.Clear();
            alternative_cast.Name = "New Name";
            mon.Should().NotRaisePropertyChangeFor(ac => ac.Name);
        }

        [Test]
        public void PropertyChanged_NameToInitial()
        {
            var alternative_cast = new AlternativeCast();
            using var mon = alternative_cast.Monitor();
            alternative_cast.Name = "New Name";
            mon.Should().RaisePropertyChangeFor(ac => ac.Name);
            mon.Should().RaisePropertyChangeFor(ac => ac.Initial);
            mon.Clear();
            alternative_cast.Name = "Not New"; // but same initials
            mon.Should().RaisePropertyChangeFor(ac => ac.Name);
            mon.Should().NotRaisePropertyChangeFor(ac => ac.Initial);
        }

        [Test]
        public void PropertyChanged_Initial()
        {
            var alternative_cast = new AlternativeCast();
            using var mon = alternative_cast.Monitor();
            alternative_cast.Initial = 'B';
            mon.Should().RaisePropertyChangeFor(ac => ac.Initial);
            mon.Clear();
            alternative_cast.Initial = 'B';
            mon.Should().NotRaisePropertyChangeFor(ac => ac.Initial);
        }

        [Test]
        public void PropertyChanged_Members()
        {
            var alternative_cast = new AlternativeCast();
            var applicant = new Applicant();
            using var mon = alternative_cast.Monitor();
            alternative_cast.Members.Add(applicant);
            mon.Should().RaisePropertyChangeFor(ac => ac.Members);
            mon.Clear();
            alternative_cast.Members.Remove(applicant);
            mon.Should().RaisePropertyChangeFor(ac => ac.Members);
        }
    }
}
