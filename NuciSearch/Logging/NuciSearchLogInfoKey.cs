using NuciLog.Core;

namespace NuciSearch.Logging
{
    public sealed class NuciSearchLogInfoKey : LogInfoKey
    {
        NuciSearchLogInfoKey(string name) : base(name) { }

        public static LogInfoKey Query => new NuciSearchLogInfoKey(nameof(Query));
        public static LogInfoKey SearchType => new NuciSearchLogInfoKey(nameof(SearchType));
        public static LogInfoKey Url => new NuciSearchLogInfoKey(nameof(Url));
    }
}
