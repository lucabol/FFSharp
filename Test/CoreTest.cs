using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;
using static FFSharp.Core;

namespace Test
{
    public class CoreTest
    {
        /* Select */

        [Theory]
        [InlineData(null, null)]
        [InlineData(3, 6)]
        public void CanSelectOnNullInt(int? a, int? r) =>
            Assert.Equal(r, a.Select<int, int>(x => x * 2));

        [Theory]
        [InlineData(null, null)]
        [InlineData("luca", "LUCA")]
        public void CanSelectOnNullString(string? a, string? r) =>
            Assert.Equal(r, a.Select(x => x.ToUpper()));

        [Theory]
        [InlineData(null, null)]
        [InlineData(3, "3")]
        public void CanSelectToDifferentType1(int? a, string? r) =>
            Assert.Equal(r, a.Select<int, string>(x => x.ToString()));

        [Theory]
        [InlineData(null, null)]
        [InlineData("3", 3)]
        public void CanSelectToDifferentType2(string? a, int? r) =>
            Assert.Equal(r, a.Select<string, int>(x => int.Parse(x)));

        /* Bind */

        static int? BindF(int i)       => i * 2;
        static string? BindF(string s) => s.ToUpper();
        static string? BindFi(int i) => i.ToString();
        static int? BindFs(string s) => int.Parse(s);

        [Theory]
        [InlineData(null, null)]
        [InlineData(3, 6)]
        public void CanBindOnNullInt(int? a, int? r) =>
            Assert.Equal(r, a.Bind(BindF));

        [Theory]
        [InlineData(null, null)]
        [InlineData("luca", "LUCA")]
        public void CanBindOnNullSting(string? a, string? r) =>
            Assert.Equal(r, a.Bind(BindF));

        [Theory]
        [InlineData(null, null)]
        [InlineData(3, "3")]
        public void CanBindToDifferentTypes1(int? a, string? r) =>
            Assert.Equal(r, a.Bind(BindFi));

        [Theory]
        [InlineData(null, null)]
        [InlineData("3", 3)]
        public void CanBindToDifferentTypes2(string? a, int? r) =>
            Assert.Equal(r, a.Bind(BindFs));

        /* Foreach */

        [Theory]
        [InlineData(null, 0)]
        [InlineData(3, 3)]
        public void CanForeachValueType(int? a, int? r)
        {
            int acc = 0;
            a.Foreach(x => acc += x); ;
            Assert.Equal(r, acc);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("a", "a")]
        public void CanForeachClassype(string? a, string? r)
        {
            string acc = "";
            a.Foreach(x => acc += x); ;
            Assert.Equal(r, acc);
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3 }, 6)]
        [InlineData(new int[] {}, 0)]
        public void CanForeachIEnumerable(IEnumerable<int> a, int r)
        {
            var acc = 0;
            a.ForEach(x => acc += x);
            Assert.Equal(r, acc);
        }

        [Theory]
        [InlineData(new[] { 1, 2, 3 }, 6)]
        [InlineData(new int[] { }, 0)]
        public void CanForeachSpan(int[] a, int r)
        {
            var sp = new Span<int>(a);
            var acc = 0;
            sp.ForEach(x => acc += x);
            Assert.Equal(r, acc);
        }

    }
}
