using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

using NuciSearch.Services;

using Assembly = System.Reflection.Assembly;

namespace NuciSearch.ArchitectureTests
{
    internal static class ProductionArchitecture
    {
        internal static Assembly Assembly { get; } = typeof(ISearchService).Assembly;

        internal static Architecture Model { get; } = new ArchLoader()
            .LoadAssemblies(Assembly)
            .Build();
    }
}