using System;
using System.Collections.Generic;
using System.Linq;

namespace RendleLabs.Unpkg
{
    public static class ValueTupleLinq
    {
        public static IEnumerable<ValueTuple<T1, T2>> Where<T1, T2>(this IEnumerable<ValueTuple<T1, T2>> source, Func<T1, T2, bool> predicate)
        {
            return source.Where(t => predicate(t.Item1, t.Item2));
        }

        public static IEnumerable<TResult> Select<T1, T2, TResult>(this IEnumerable<ValueTuple<T1, T2>> source, Func<T1, T2, TResult> selector)
        {
            return source.Select(t => selector(t.Item1, t.Item2));
        }
    }
}