using System;

namespace SignalMenu
{
    public static class ObjectNames
    {
        private static readonly string _session = Guid.NewGuid().ToString("N").Substring(0, 6);

        public static string Get(string tag)
        {
            return $"_gt_{_session}_{tag.GetHashCode():X8}";
        }
    }
}
