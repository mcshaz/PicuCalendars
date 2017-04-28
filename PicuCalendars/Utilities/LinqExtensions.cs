using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PicuCalendars.Utilities
{
    public static class LinqExtensions
    {
        [DebuggerStepThrough()]
        public static IEnumerable<IList<T>> ConsecutiveGroup<T>(this IEnumerable<T> enumerable, Func<T,T,bool> isInGroup)
        {
            using (var e = enumerable.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    List<T> currentList = new List<T>{ e.Current };
                    T prior = e.Current;
                    while (e.MoveNext())
                    {
                        if (isInGroup(prior, e.Current))
                        {
                            currentList.Add(e.Current);
                        }
                        else
                        {
                            yield return currentList;
                            currentList = new List<T> { e.Current };
                        }
                        prior = e.Current;
                    }
                    yield return currentList;
                }
            }
        }

        [DebuggerStepThrough()]
        public static bool ConsecutiveAny<T>(this IEnumerable<T> enumerable, Func<T, T, bool> predicate)
        {
            using (var e = enumerable.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    T prior = e.Current;
                    while (e.MoveNext())
                    {
                        if (predicate(prior, e.Current))
                        {
                            return true;
                        }
                        prior = e.Current;
                    }
                }
                return false;
            }
        }

        [DebuggerStepThrough()]
        public static bool ConsecutiveAll<T>(this IEnumerable<T> enumerable, Func<T, T, bool> predicate)
        {
            using (var e = enumerable.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    T prior = e.Current;
                    while (e.MoveNext())
                    {
                        if (!predicate(prior, e.Current))
                        {
                            return false;
                        }
                        prior = e.Current;
                    }
                }
                return true;
            }
        }
    }
}
