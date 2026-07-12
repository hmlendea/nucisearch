using NuciLog.Core;

namespace NuciSearch.Logging
{
    public sealed class NuciSearchOperation : Operation
    {
        NuciSearchOperation(string name) : base(name) { }

        public static Operation Search => new NuciSearchOperation(nameof(Search));
    }
}
