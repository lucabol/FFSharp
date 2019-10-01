using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using static System.Linq.Enumerable;
using static System.Console;
using System.Runtime.ExceptionServices;

//TODO: what to do if params are null?
namespace FFSharp
{
    public static class Core
    {
        /* Map */
        public static TR? Map<T, TR>(this T? a, Func<T, TR?> f)
            where T : struct
            where TR : struct
            => a == null ? null : f is null ? null : f(a.Value);

        public static TR? Map<T, TR>(this T? a, Func<T, TR?> f)
            where T : class
            where TR : class
            => a == null ? null : f is null ? null : f(a);

        public static TR? Map<T, TR>(this T? a, Func<T, TR?> f)
            where T : struct
            where TR : class
            => a == null ? null : f is null ? null : f(a.Value);

        public static TR? Map<T, TR>(this T? a, Func<T, TR?> f)
            where T : class
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

        public static IEnumerable<TR> Bind<T, TR>(this IEnumerable<T> ts, Func<T, TR?> f)
            where TR : struct
            => ts.SelectMany(t => f(t).AsEnumerable());

        public static IEnumerable<TR> Bind<T, TR>(this IEnumerable<T> ts, Func<T, TR?> f)
            where TR : class
            => ts.SelectMany(t => f(t).AsEnumerable());

        public static IEnumerable<TR> Bind<T, TR>(this T? t, Func<T, IEnumerable<TR>> f)
            where T : struct
            => t.AsEnumerable().SelectMany(f);

        public static IEnumerable<TR> Bind<T, TR>(this T? t, Func<T, IEnumerable<TR>> f)
            where T : class
            => t.AsEnumerable().SelectMany(f);

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

        /* Where */
        public static T? Where<T>(this T? a, Func<T, bool> pred)
            where T : struct
            => a is null || pred is null ? null : pred(a.Value) ? a : null;

        public static T? Where<T>(this T? a, Func<T, bool> pred)
            where T : class
            => a is null || pred is null ? null : pred(a) ? a : null;

        /* Option helpers */
        public static IEnumerable<T> AsEnumerable<T>(this T? a)
            where T : struct
            => a.HasValue ? Repeat(a.Value, 1) : Empty<T>();

        public static IEnumerable<T> AsEnumerable<T>(this T? a)
            where T : class
            => a  != null ? Repeat(a, 1) : Empty<T>();

        /* Error management */
        private static readonly FieldInfo STACK_TRACE_STRING_FI = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly Type TRACE_FORMAT_TI = Type.GetType("System.Diagnostics.StackTrace").GetNestedType("TraceFormat", BindingFlags.NonPublic);
        private static readonly MethodInfo TRACE_TO_STRING_MI = typeof(StackTrace).GetMethod("ToString", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { TRACE_FORMAT_TI }, null);

        private static Exception SetStackTrace2(this Exception target, StackTrace stack)
        {
            var getStackTraceString = TRACE_TO_STRING_MI.Invoke(stack, new object[] { Enum.GetValues(TRACE_FORMAT_TI).GetValue(0) });
            STACK_TRACE_STRING_FI.SetValue(target, getStackTraceString);
            return target;
        }

        private static readonly Action<Exception> _internalPreserveStackTrace =
            (Action<Exception>)Delegate.CreateDelegate(
                typeof(Action<Exception>),
                typeof(Exception).GetMethod(
                    "InternalPreserveStackTrace",
                    BindingFlags.Instance | BindingFlags.NonPublic));

        public static void PreserveStackTrace(Exception e)
        {
            _internalPreserveStackTrace(e);
        }
        private static readonly FieldInfo stackTraceString = typeof(Exception).GetField("_stackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo stackTrace = typeof(Exception).GetField("_stackTrace", BindingFlags.Instance | BindingFlags.NonPublic);

        private static void SetStackTracesString(this Exception exception, string value)
            => stackTraceString.SetValue(exception, value);
        private static void SetStackTrace(this Exception exception, StackTrace value)
            => stackTrace.SetValue(exception, value);

        [ThreadStatic] private static ErrorData errorData = null;

        public class ErrorData
        {
            public string Message { get; }
            public StackTrace StackTrace { get; }

            public ErrorData(string msg, StackTrace s) => (Message, StackTrace) = (msg, s);
            public void Deconstruct(out string message, out StackTrace stackTrace) => (message, stackTrace) = (Message, StackTrace);
            public override string ToString() => $"Error: {Message}\n{StackTrace}";
        }


        public static T? Fail<T>(string msg, int i = default)
            where T : class
        {
            errorData = new ErrorData(msg, new StackTrace(1, true));
            return null;
        }

        public static T? Fail<T>(string msg)
            where T : struct
        {
            errorData = new ErrorData(msg, new StackTrace(1, true));
            return null;
        }


    }
}
