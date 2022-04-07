using System.Collections.Generic;

namespace Bartendro.Common.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IEnumerableOfStringExtensions
    {
        public static string Join(this IEnumerable<string> list, string separator)
        {
            return string.Join(separator, list);
        }
    }
}