using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static System.Console;
using static System.Linq.Enumerable;
using static FFSharp.Core;
using FFSharp;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace App
{
    static class Transfer
    {
        static class Errors
        {
            internal class BicError : ErrorData
            {
                public string Bic { get; }
                private BicError(string msg, string bic) : base(msg) { Bic = bic; }
                internal static BicError Create(string bic) => new BicError($"Bic not valid: {bic}", bic);
            }
            internal class TransferDate : ErrorData
            {
                internal TransferDate(string msg) : base(msg) { }
                internal static TransferDate Create(DateTime d) => new TransferDate($"Date not valid: {d}");
            }

        }

        public readonly struct BookTransfer
        {
            public readonly string Bic;
            public readonly DateTime Date;
            private BookTransfer(string bic, DateTime date) => (Bic, Date) = (bic, date);
            public override string ToString() => $"BookTransfer: Bic={Bic}, Date={Date}";

            public static BookTransfer Create(string bic, DateTime date) => new BookTransfer(bic, date);
        }

        static Regex bicRegex = new Regex("[A-Z]{11}");

        static BookTransfer? ValidateBIC(this BookTransfer cmd)
            => bicRegex.IsMatch(cmd.Bic) ? cmd : Fail<BookTransfer>(Errors.BicError.Create(cmd.Bic));

        static BookTransfer? ValidateDate(this BookTransfer cmd)
            => cmd.Date.Date <= DateTime.Now ? cmd : Fail<BookTransfer>(Errors.TransferDate.Create(cmd.Date));

        static Unit? Save(this BookTransfer cmd) => Unit.Default;

        static Unit? HandleAction(this BookTransfer cmd) =>
            cmd
            .ValidateBIC()
            ?.ValidateDate()
            ?.Save();

        static BookTransfer? HandleFunc(this BookTransfer cmd) =>
            cmd
            .ValidateBIC()
            ?.ValidateDate();

        class TransferDriver
        {
            static void Main()
            {
                var good    = BookTransfer.Create("AABCDEFGHIL", DateTime.Now.AddYears(-1));
                var badBic  = BookTransfer.Create("AABCDEFGHI1", DateTime.Now.AddYears(1));
                var badDate = BookTransfer.Create("AABCDEFGHIL", DateTime.Now.AddYears(1));

                static void HandleError(ErrorData error)
                {
                    switch (error)
                    {
                        case Errors.BicError e: WriteLine(e); WriteLine($"Shows adding data to an error type:{e.Bic}\n\n");  break;
                        case Errors.TransferDate e: WriteLine(e); break;
                    }

                }
                static void HandleOk(Unit _) => WriteLine("Transfer Ok");

                WriteLine("Check handling errors immediately after the pipeline with Match\n");

                good.HandleAction().Match(HandleOk, HandleError); WriteLine();
                badBic.HandleAction().Match(HandleOk, HandleError);
                badDate.HandleAction().Match(HandleOk, HandleError);

                var canMatchGood    = good.HandleFunc();
                WriteLine(canMatchGood.Match(x => x.ToString(), error => error.ToString()));

                var canMatchBadBic  = badBic.HandleFunc();
                WriteLine(canMatchBadBic.Match(x => x.ToString(), error => error.ToString()));

                var canMatchBadDate = badDate.HandleFunc();
                WriteLine(canMatchBadDate.Match(x => x.ToString(), error => error.ToString()));

                Trace.Assert(CheckHandledAllErrors());

                WriteLine("Check handling all errors in one shot at the end of multiple pipelines\n");

                good.HandleAction();
                badBic.HandleAction();
                badDate.HandleAction();
                var _1 = good.HandleFunc();
                var _2 = badBic.HandleFunc();
                var _3 = badDate.HandleFunc();

                PopAllErrors().ForEach<ErrorData>(WriteLine);

                Trace.Assert(CheckHandledAllErrors());

                WriteLine("Test throwing right exception when trying to consume too many errors\n");

                try
                {
                    var result = badBic.HandleFunc();
                    result.Match(x => x.ToString(), error => error.ToString());
                    result.Match(x => x.ToString(), error => error.ToString());
                }
                catch (ErrorsException ee)
                {
                    WriteLine(ee);
                }
                Trace.Assert(CheckHandledAllErrors());

            }
        }
    }
}