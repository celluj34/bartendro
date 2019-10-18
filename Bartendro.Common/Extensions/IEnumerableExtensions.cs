using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bartendro.Common.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize)
        {
            return source.Select((x, i) => new
                         {
                             Index = i,
                             Value = x
                         })
                         .GroupBy(x => x.Index / chunkSize)
                         .Select(x => x.Select(y => y.Value));
        }

        public static string JoinWithAnd<T>(this IEnumerable<T> source, string separator, string valueFormat = "{0}")
        {
            var items = source.Select(x => string.Format(valueFormat, x)).ToList();
            switch(items.Count)
            {
                case 0:
                    return null;

                case 1:
                    return items.Single();

                case 2:
                    return $"{items.First()} and {items.Last()}";

                default:
                    return items.JoinWithAnd(separator);
            }
        }

        private static string JoinWithAnd<T>(this IReadOnlyCollection<T> source, string separator)
        {
            var allButLast = source.Take(source.Count - 1);
            var allButLastString = string.Join(separator, allButLast);

            var sb = new StringBuilder();
            sb.Append(allButLastString);
            sb.Append(separator);

            if(!separator.EndsWith(" "))
            {
                sb.Append(" ");
            }

            sb.Append("and ");
            sb.Append(source.Last());

            return sb.ToString();
        }
    }
}