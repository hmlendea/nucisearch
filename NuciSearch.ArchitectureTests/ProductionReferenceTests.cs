using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using NUnit.Framework;

namespace NuciSearch.ArchitectureTests
{
    [TestFixture]
    public sealed class ProductionReferenceTests
    {
        private static string ProductionProjectPath => Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "ProductionProjects",
            "NuciSearch.csproj");

        [Test]
        public void GivenProductionProjects_WhenCheckingDeclaredReferences_ThenTestDependenciesAreAbsent()
        {
            XDocument project = XDocument.Load(ProductionProjectPath);
            IEnumerable<XElement> references = project.Descendants().Where(element =>
                string.Equals(element.Name.LocalName, "ProjectReference") ||
                string.Equals(element.Name.LocalName, "PackageReference") ||
                string.Equals(element.Name.LocalName, "Reference"));

            Assert.That(references, Is.Not.Empty, "Production dependency selection must not be empty.");
            Assert.That(
                references.Select(element => element.Attribute("Include")?.Value)
                    .Where(IsTestDependency),
                Is.Empty,
                "Production project references must not point to test projects or test tooling.");
        }

        [Test]
        public void GivenProductionAssembly_WhenCheckingReferences_ThenTestAssembliesAreAbsent()
        {
            AssemblyName[] references = ProductionArchitecture.Assembly.GetReferencedAssemblies();

            Assert.That(references, Is.Not.Empty, "Production assembly references must be inspected.");
            Assert.That(
                references.Select(reference => reference.Name).Where(IsTestDependency),
                Is.Empty,
                "The production assembly must remain independent of test assemblies and frameworks.");
        }

        [Test]
        public void GivenProductionProject_WhenCheckingFriendAssemblies_ThenInternalsAreNotExposed()
        {
            XDocument project = XDocument.Load(ProductionProjectPath);

            Assert.That(
                project.Descendants().Where(element =>
                    string.Equals(element.Name.LocalName, "InternalsVisibleTo") ||
                    string.Equals(element.Attribute("Include")?.Value,
                        typeof(InternalsVisibleToAttribute).FullName)),
                Is.Empty,
                "Production projects must not declare InternalsVisibleTo.");
        }

        [Test]
        public void GivenProductionAssembly_WhenCheckingFriendAssemblies_ThenInternalsAreNotExposed()
            => Assert.That(
                ProductionArchitecture.Assembly.GetCustomAttributesData()
                    .Where(attribute => attribute.AttributeType == typeof(InternalsVisibleToAttribute)),
                Is.Empty,
                "The compiled production assembly must not expose internals to friend assemblies.");

        private static bool IsTestDependency(string? reference)
            => reference is not null &&
                (reference.Contains("Tests", StringComparison.OrdinalIgnoreCase) ||
                 reference.Contains("NUnit", StringComparison.OrdinalIgnoreCase) ||
                 reference.Contains("Moq", StringComparison.OrdinalIgnoreCase) ||
                 reference.Contains("ArchUnit", StringComparison.OrdinalIgnoreCase) ||
                 reference.Contains("Test.Sdk", StringComparison.OrdinalIgnoreCase) ||
                 reference.Contains("testhost", StringComparison.OrdinalIgnoreCase) ||
                 reference.Contains("coverlet", StringComparison.OrdinalIgnoreCase));
    }
}