using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class AssemblyTests
    {
        static readonly Type[] testedTypes = new[] {
            typeof(Carmen.ShowModel.ShowContext),
            typeof(Carmen.CastingEngine.Allocation.IAllocationEngine)
        };

        [Test]
        public void AssemblyTitle_MatchesName()
        {
            foreach (var type in testedTypes)
            {
                var assembly = GetAssembly(type);
                var title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
                var name = assembly.GetName().Name;
                title.Should().Be(name);
            }
        }

        [Test]
        public void AssemblyVersion_MatchesInformationalVersion()
        {
            foreach (var type in testedTypes)
            {
                var assembly = GetAssembly(type);
                if (assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion is not string info_version)
                    throw new ApplicationException("Informational version not found.");
                if (assembly.GetName().Version is not Version version)
                    throw new ApplicationException("Assembly version not found.");
                var major_minor = $"{version.Major}.{version.Minor}";
                if (info_version.Length == major_minor.Length)
                    info_version.Should().Be(major_minor);
                else
                    info_version.Should().StartWith($"{major_minor}-");
            }
        }

        [Test]
        public void AssemblyCompany_MatchesCopyright()
        {
            foreach (var type in testedTypes)
            {
                var assembly = GetAssembly(type);
                var company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
                var copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;
                company.Should().Be(copyright);
            }
        }

        [Test]
        public void AssemblyVersions_Match()
        {
            Version? first_version = null;
            foreach (var type in testedTypes)
            {
                var assembly = GetAssembly(type);
                if (assembly.GetName().Version is not Version version)
                    throw new ApplicationException("Assembly version not found.");
                if (first_version == null)
                    first_version = version;
                else
                    version.Should().BeEquivalentTo(first_version);
            }
        }

        [Test]
        public void AllLoadedCarmenAssemblies_AreTested()
        {
            var tested_assemblies = testedTypes
                .Select(t => GetAssembly(t))
                .Select(a => a.GetName().Name)
                .ToHashSet();
            var loaded_carmen_assemblies = Assembly.GetExecutingAssembly()
                .GetReferencedAssemblies()
                .Select(an => an.Name)
                .OfType<string>()
                .Where(n => n.StartsWith("Carmen."));
            foreach (var loaded_assembly in loaded_carmen_assemblies)
                if (!tested_assemblies.Contains(loaded_assembly))
                    throw new ArgumentException($"{loaded_assembly} not tested.");
        }

        private Assembly GetAssembly(Type type)
            => Assembly.GetAssembly(type) ?? throw new ArgumentException($"Error loading assembly of {type.Name}");
    }
}
