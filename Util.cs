using System;
using System.Collections.Generic;
using System.Linq;

namespace MyParser
{
    public static partial class Util
    {
        private delegate Func<T1> Recursive<T1>(Recursive<T1> f);
        private delegate Func<T1, T2> Recursive<T1, T2>(Recursive<T1, T2> f);

        public static T1 YConv<T1>(this YCONV<T1> input) =>
            ((Recursive<T1>)
            (f => () => input(f(f))()))
            (f => () => input(f(f))())
            ();

        public static T2 YConv<T1, T2>(this YCONV<T1, T2> input, T1 t1) =>
            ((Recursive<T1, T2>)
            (f => (_t1) => input(f(f))(_t1)))
            (f => (_t1) => input(f(f))(_t1))
            (t1);

        public static int 数値へ変換(this char c) => int.Parse(c.ToString());
        public static int 数値へ変換(this IEnumerable<char> v) => int.Parse(string.Concat(v));
        public static string 文字列へ変換(this IEnumerable<char> param) => string.Concat(param);

        public static パーサー<T, D> IfFailedReturn<T, D>(this パーサー<T, D> parser, D data) => stm1 =>
             parser(stm1).GetResult(
                 (stm2, data2) => stm2.成功(data2),
                 stm2 => stm1.成功(data)
             );

        public static パーサー<T, D> Success<T, D>(this D data, T _ = default) => stm1 => stm1.成功(data);

        public static パーサー<T, bool> ToGetResult<T, D>(this パーサー<T, D> parser) => stm1 =>
             parser(stm1).GetResult(
                 (stm2, data2) => stm2.成功(true),
                 stm2 => stm1.成功(false)
             );

        public static パーサー<T, (bool result, D data)> ToGetResultAndData<T, D>(this パーサー<T, D> parser, D data = default) => stm1 =>
            parser(stm1).GetResult(
                (stm2, data2) => stm2.成功((true, data2)),
                stm2 => stm1.成功((false, data))
            );

        public static パーサー<T, パーサー用ストリーム<T>> GetCurrentStm<T>() => stm1 => stm1.成功(stm1);

        public static パーサー<T, D> SkipIf<T, D>(this パーサー<T, D> parser, bool skip, D data = default) =>
            skip ? data.Success<T, D>() : parser;

        public static IResult<T, D> 成功<T, D>(this in パーサー用ストリーム<T> s, D d) => IResult実装<T, D>.成功(s, d);
        public static IResult<T, D> 失敗<T, D>(this in パーサー用ストリーム<T> s, D _ = default) => IResult実装<T, D>.失敗(s);

        public static IEnumerable<TSource> Concat<TSource>(this TSource first, IEnumerable<TSource> second) => new[] { first }.Concat(second);
    }
}
