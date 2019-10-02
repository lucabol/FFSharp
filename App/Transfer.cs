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

namespace App
{
    static class Transfer
    {
        static class Errors
        {
            internal class BicError: ErrorData
            {
                internal BicError(string msg) : base(msg) { }
                internal static BicError Default => new BicError("Bic error");
            }
            internal class TransferDataIsPast : ErrorData
            {
                internal TransferDataIsPast(string msg) : base(msg) { }
                internal static TransferDataIsPast Default => new TransferDataIsPast("Bic error");
            }

        }

        public struct BookTransfer { public string Bic; public DateTime Date; }

        static BookTransfer? Validate(this BookTransfer cmd) => cmd;
        static Unit? Save(this BookTransfer cmd) => Unit.Default;

        static Regex bicRegex = new Regex("[A-Z]{11}");

        static BookTransfer? ValidateBIC(this BookTransfer cmd)
            => !bicRegex.IsMatch(cmd.Bic) ? Fail<BookTransfer>(Errors.BicError.Default) : cmd;

        static BookTransfer? ValidateDate(this BookTransfer cmd)
            => cmd.Date.Date <= DateTime.Now ? Fail<BookTransfer>(Errors.TransferDataIsPast.Default) : cmd;

        static Unit? Handle(this BookTransfer cmd) =>
            cmd
            .ValidateBIC()
            ?.ValidateDate()
            ?.Save();

        class TransferDriver
        {
            static void Main()
            {
                var cmd = new BookTransfer();

                cmd.Handle().Match(
                    onError:    () => WriteLine("Failure"),
                    onSuccess:  () => WriteLine("Ok")
                    );
            }
        }
    }
}
