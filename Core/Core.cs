using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

//TODO: what to do if params are null?
namespace FFSharp
{
    public static class Core
    {
        /* Select */
        public static TR? Select<TA, TR>(this TA? a, Func<TA, TR?> f)
            where TA : struct
            where TR : struct
            => a == null ? null : f is null ? null : f(a.Value);

        public static TR? Select<TA, TR>(this TA? a, Func<TA, TR?> f)
            where TA : class
            where TR : class
            => a == null ? null : f is null ? null : f(a);


        public static TR? Select<TA, TR>(this TA? a, Func<TA, TR?> f)
            where TA : struct
            where TR : class
            => a == null ? null : f is null ? null : f(a.Value);

        public static TR? Select<TA, TR>(this TA? a, Func<TA, TR?> f)
            where TA : class
            where TR : struct
            => a == null ? null : f is null ? null : f(a);

        /* Bind */

        public static TR? Bind<T, TR>(this T? optT, Func<T, TR?> f)
            where T : struct
            where TR : struct
            => optT.HasValue ? (f is null ? null : f(optT.Value)) : null;

        public static TR? Bind<T, TR>(this T? optT, Func<T, TR?> f)
            where T : class
            where TR : class
            => optT != null ? (f is null ? null : f(optT)) : null;

        public static TR? Bind<T, TR>(this T? optT, Func<T, TR?> f)
            where T : struct
            where TR : class
            => optT.HasValue ? (f is null ? null : f(optT.Value)) : null;

        public static TR? Bind<T, TR>(this T? optT, Func<T, TR?> f)
            where T : class
            where TR : struct
            => optT != null ? (f is null ? null : f(optT)) : null;

        public static IEnumerable<TR> Bind<T, TR>(IEnumerable<T> ts, Func<T, IEnumerable<TR>> f)
            => ts.SelectMany(f);

        /* Foreach */

        public static void Foreach<T>(this T? t, Action<T> action)
            where T : struct
        {
            if (t is null || action is null) return;
            action(t.Value);
        }
        public static void Foreach<T>(this T? t, Action<T> action)
            where T : class
        {
            if (t is null || action is null) return;
            action(t);
        }

        public static void ForEach<T>(this IEnumerable<T> ts, Action<T> action)
        {
            if (ts is null) return;
            foreach (var x in ts) action(x);
        }

        public static void ForEach<T>(this Span<T> sp, Action<T> action)
        {
            foreach (var x in sp) action(x);
        }


    }
}
