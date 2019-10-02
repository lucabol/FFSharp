using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static System.Console;
using static System.Linq.Enumerable;
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

    static void Main2()
    {
        var age = 30.AsAge()
                    ?.AddYears(1);
        var age2 = 10.AsAge()
                    ?.AddYears(2)
                    ?.AddYears(-50);

        age.ForEach(x => WriteLine(x.years));
        age2.ForEach(x => WriteLine(x));

        age.ForError(WriteLine);
        age2.ForError(WriteLine);

        var cand = new Candidate();

        Func<Candidate, bool> IsEligible = x => false;
        Func<Candidate, Candidate?> TechTest = c => c;
        Func<Candidate, Candidate?> Interview = c => c;

        var c = cand
            ?.Where(IsEligible)
            ?.Bind(TechTest);

        c?.ForEach(WriteLine);
        c.ForError(WriteLine);
    }

}
