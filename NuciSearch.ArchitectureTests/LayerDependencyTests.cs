using System.Text.RegularExpressions;

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.NUnit;

using NUnit.Framework;

using static ArchUnitNET.Fluent.ArchRuleDefinition;
using static ArchUnitNET.Fluent.Slices.SliceRuleDefinition;

namespace NuciSearch.ArchitectureTests
{
    [TestFixture]
    public sealed class LayerDependencyTests
    {
        private static readonly IObjectProvider<IType> presentation = Types().That()
            .ResideInNamespaceMatching(@"^NuciSearch\.Components(\.|$)").As("Blazor presentation");

        private static readonly IObjectProvider<IType> services = Types().That()
            .ResideInNamespaceMatching(@"^NuciSearch\.Services(\.|$)").As("application services");

        private static readonly IObjectProvider<IType> localisation = Types().That()
            .ResideInNamespaceMatching(@"^NuciSearch\.Localisation(\.|$)").As("culture providers");

        private static readonly IObjectProvider<IType> logging = Types().That()
            .ResideInNamespaceMatching(@"^NuciSearch\.Logging(\.|$)").As("logging definitions");

        [TestCase("NuciSearch.Services")]
        [TestCase("NuciSearch.Localisation")]
        [TestCase("NuciSearch.Logging")]
        public void GivenAnApplicationArea_WhenCheckingDependencies_ThenPresentationIsNotReferenced(
            string applicationNamespace)
        {
            IObjectProvider<IType> applicationArea = Types().That()
                .ResideInNamespaceMatching($@"^{Regex.Escape(applicationNamespace)}(\.|$)");

            AssertHasTypes(applicationArea);
            AssertHasTypes(presentation);

            Types().That().Are(applicationArea).Should().NotDependOnAny(presentation)
                .Because("application logic must not depend on Blazor presentation")
                .Check(ProductionArchitecture.Model);
        }

        [Test]
        public void GivenPresentation_WhenReferencingServices_ThenOnlyServiceInterfacesAreUsed()
        {
            IObjectProvider<IType> serviceImplementations = Classes().That()
                .ResideInNamespaceMatching(@"^NuciSearch\.Services(\.|$)");

            AssertHasTypes(presentation);
            AssertHasTypes(serviceImplementations);

            Types().That().Are(presentation).Should().NotDependOnAny(serviceImplementations)
                .Because("Blazor components must access application services through interfaces")
                .Check(ProductionArchitecture.Model);
        }

        [Test]
        public void GivenPresentation_WhenCheckingApplicationDependencies_ThenImplementationAreasAreNotUsed()
        {
            AssertHasTypes(presentation);
            AssertHasTypes(localisation);
            AssertHasTypes(logging);

            Types().That().Are(presentation).Should().NotDependOnAny(localisation)
                .AndShould().NotDependOnAny(logging)
                .Because("culture providers and logging implementations belong behind the application boundary")
                .Check(ProductionArchitecture.Model);
        }

        [Test]
        public void GivenApplicationAreas_WhenCheckingDependencies_ThenNamespaceSlicesAreAcyclic()
        {
            AssertHasTypes(presentation);
            AssertHasTypes(services);
            AssertHasTypes(localisation);
            AssertHasTypes(logging);

            Slices().Matching("NuciSearch.(*)").Should().BeFreeOfCycles()
                .Check(ProductionArchitecture.Model);
        }

        private static void AssertHasTypes(IObjectProvider<IType> selection)
            => Assert.That(
                selection.GetObjects(ProductionArchitecture.Model),
                Is.Not.Empty,
                $"The {selection.Description} boundary must select production types.");
    }
}