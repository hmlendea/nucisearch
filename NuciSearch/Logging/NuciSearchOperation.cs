using NuciLog.Core;

namespace NuciSearch.Logging
{
    public sealed class NuciSearchOperation : Operation
    {
        public static Operation GetCountryCode
            => new NuciSearchOperation(nameof(GetCountryCode));

        public static Operation Search => new NuciSearchOperation(nameof(Search));

        private NuciSearchOperation(string name) : base(name) { }
    }
}
