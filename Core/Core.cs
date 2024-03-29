﻿using System;
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

        public static IEnumerable<TR> Map<T, TR>(this IEnumerable<T> t, Func<T, TR> f) => t.Select(f);
        public static IEnumerable<TR> Map<T, TR>(this Span<T> t, Func<T, TR> f) => t.ToArray().Select(f);

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
        public static void ForEach<T>(this T? t, Action<T> action)
            where T : struct
        {
            if (t is null || action is null) return;
            action(t.Value);
        }
        public static void ForEach<T>(this T? t, Action<T> action)
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
            => a != null ? Repeat(a, 1) : Empty<T>();

        /* Error management */

        public class ErrorsException : Exception {
            internal ErrorsException(string msg, Exception e) : base(msg, e) { }
        }

        private class ErrorStack<T> : Stack<T>
        {
            internal ErrorStack() : base() { }
            internal new T Pop()
            {
                try
                {
                    return base.Pop();

                } catch(InvalidOperationException e)
                {
                    throw new ErrorsException("You have already retrieved all the errors in this thread", e);
                }
            }
        }

        [ThreadStatic] private static ErrorStack<ErrorData> errors = new ErrorStack<ErrorData>();

        public class ErrorData
        {
            public string Message { get; }
            public StackTrace StackTrace { get; }

            public ErrorData(string msg)
            {
                Message = msg;
                StackTrace = new StackTrace(3, true);
            }
            public void Deconstruct(out string message, out StackTrace stackTrace) => (message, stackTrace) = (Message, StackTrace);
            public override string ToString() => $"Error: {Message}\n{StackTrace}";
        }

        public static T? Fail<T>(ErrorData e) where T : struct
        {
            errors.Push(e);
            return null;
        }

        public static T? Fail<T>(ErrorData e, int i = 0) where T : class
        {
            errors.Push(e);
            return null;
        }

        public static T? Fail<T>(string msg, int i = default) where T : class
            => Fail<T>(new ErrorData(msg));

        public static T? Fail<T>(string msg) where T : struct
            => Fail<T>(new ErrorData(msg));

        public static void ForError<T>(this T? t, Action<ErrorData> f)
            where T : class
        {
            if (t is null && !(f is null)) f(errors.Pop());
            return;
        }

        public static void ForError<T>(this T? t, Action<ErrorData> f)
            where T : struct
        {
            if (t is null && !(f is null)) f(errors.Pop());
            return;
        }

        /* Unit */
        // From: https://github.com/dotnet/reactive/blob/master/Rx.NET/Source/src/System.Reactive/Unit.cs
        public struct Unit : IEquatable<Unit>
        {
            public bool Equals(Unit other) => true;
            public override bool Equals(object obj) => obj is Unit;
            public override int GetHashCode() => 0;
            public override string ToString() => "()";
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "first", Justification = "Parameter required for operator overloading."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second", Justification = "Parameter required for operator overloading.")]
            public static bool operator ==(Unit first, Unit second) => true;
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "first", Justification = "Parameter required for operator overloading."), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "second", Justification = "Parameter required for operator overloading.")]
            public static bool operator !=(Unit first, Unit second) => false;

            public static Unit Default => default;
        }

        public static TR Match<T, TR>(this T? t, Func<T, TR> onSuccess,Func<ErrorData, TR> onError) where T:class
            => t switch
            {
                null    => onError(errors.Pop()),
                { }     => onSuccess(t)
            };

        public static TR Match<T, TR>(this T? t, Func<T, TR> onSuccess, Func<ErrorData, TR> onError) where T :struct
            => t switch
            {
                null => onError(errors.Pop()),
                { } => onSuccess(t.Value)
            };
        public static void Match<T>(this T? t, Action<T> onSuccess, Action<ErrorData> onError) where T : class
        {
            if (t is null) onError(errors.Pop());
            else onSuccess(t);
        }
        public static void Match<T>(this T? t, Action<T> onSuccess, Action<ErrorData> onError) where T : struct
        {
            if (t is null) onError(errors.Pop());
            else onSuccess(t.Value);
        }

        public static IEnumerable<ErrorData> PopAllErrors()
        {
            var a = errors.ToArray();
            errors.Clear();
            return a;
        }

        public static bool CheckHandledAllErrors() => errors.Count == 0;

    }
}
