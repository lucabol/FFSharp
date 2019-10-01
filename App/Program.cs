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

public struct Age { public int years; }


static class Program
{
    private static bool ValidAge(int years) => years > 0 && years < 200;

    static Age? AsAge(this int years) => ValidAge(years)
        ? new Age { years = years }
        : Fail<Age>("Age not correct");

    static Age? AddYears(this Age age, int years) => (age.years + years).AsAge();

    class Candidate { };

    static void Main()
    {
        var age = 30.AsAge()
                    ?.AddYears(1);
        var age2 = 10.AsAge()
                    ?.AddYears(2)
                    ?.AddYears(-50)
                    .Throw();

        age.Foreach(x => WriteLine(x.years));
        age2.Foreach(x => WriteLine(x));

        var cand = new Candidate();

        Func<Candidate, bool> IsEligible = x => true;
        Func<Candidate, Candidate?> TechTest = c => c;
        Func<Candidate, Candidate?> Interview = c => c;

        cand
            ?.Where(IsEligible)
            ?.Bind(TechTest)
            ?.Foreach(WriteLine);

    }

}
