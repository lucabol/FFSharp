using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static System.Console;
using static System.Linq.Enumerable;
using Unit = System.ValueTuple;
using static FFSharp.Core;

public struct Age
{
    private int Value { get; }
    private Age(int value)
    {
        if (!IsValid(value))
            throw new ArgumentException($"{value} is not a valid age");

        Value = value;
    }

    private static bool IsValid(int age)
       => 0 <= age && age < 120;

    public static Age? Of(int age)
       => IsValid(age) ? new Age?(new Age(age)) : null;

    public static bool operator <(Age l, Age r) => l.Value < r.Value;
    public static bool operator >(Age l, Age r) => l.Value > r.Value;

    public static bool operator <(Age l, int r) => l < new Age(r);
    public static bool operator >(Age l, int r) => l > new Age(r);

    public override string ToString() => Value.ToString();
}

struct A { public int X; public string Y; }
//struct Unit { }
static class Program
{
    public static class Int
    {
        public static int? Parse(string s)
        {
            return int.TryParse(s, out int result)
               ? (int?)(result) : null;
        }
    }

    static void Main()
    {
        Func<string, Age?> parseAge = s => Int.Parse(s).Bind(Age.Of);
        var a1 = parseAge("26");
        a1.Foreach(x => WriteLine(x));

        string input = "3";
        int? optI = Int.Parse(input);
        var ageOpt = optI.Bind(Age.Of);

        var a = new A { X = 1, Y = "test" };
        var ii = new[] { 1, 2, 3 };
        var l = new List<int> { 1, 2, 3 };
        Range(1, 10).ForEach(WriteLine);
        Span<int> s = new[] { 1, 2, 3 };
        s.ForEach(WriteLine);
    }
}
