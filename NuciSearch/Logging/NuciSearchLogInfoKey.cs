using NuciLog.Core;

namespace NuciSearch.Logging
{
    public sealed class NuciSearchLogInfoKey : LogInfoKey
    {
        public static LogInfoKey IpAddress => new NuciSearchLogInfoKey(nameof(IpAddress));

        public static LogInfoKey Query => new NuciSearchLogInfoKey(nameof(Query));

        public static LogInfoKey SearchType => new NuciSearchLogInfoKey(nameof(SearchType));

        public static LogInfoKey Url => new NuciSearchLogInfoKey(nameof(Url));

        private NuciSearchLogInfoKey(string name) : base(name) { }
    }
}
