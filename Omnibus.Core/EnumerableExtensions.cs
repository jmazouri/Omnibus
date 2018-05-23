using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Omnibus.Core
{
    public static class EnumerableExtensions
    {
        public static (IEnumerable<T> trueValues, IEnumerable<T> falseValues) Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
            => (source.Where(predicate), source.Where(d => !predicate(d)));
    }
}
