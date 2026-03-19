using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bartendro.Common.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IEnumerableExtensions
    {
        public static string? JoinWithAnd<T>(this IEnumerable<T> source, string separator, string valueFormat = "{0}")
        {
            var items = source.Select(x => string.Format(valueFormat, x)).ToList();

            return items.Count switch
            {
                0 => null,
                1 => items.Single(),
                2 => $"{items.First()} and {items.Last()}",
                _ => items.JoinWithAnd(separator)
            };
        }

        private static string JoinWithAnd<T>(this IReadOnlyCollection<T> source, string separator)
        {
            var allButLast = source.Take(source.Count - 1);
            var allButLastString = string.Join(separator, allButLast);

            var sb = new StringBuilder();
            sb.Append(allButLastString);
            sb.Append(separator);

            if (!separator.EndsWith(' '))
            {
                sb.Append(' ');
            }

            sb.Append("and ");
            sb.Append(source.Last());

            return sb.ToString();
        }
    }
}