using System;
using System.Collections.Generic;
using System.Linq;

namespace MyParser
{
    public static class 共通
    {
        public static パーサー<T, T> 任意の値<T>() => stm =>
            (stm.HasValue) ?
                stm.Next.成功(stm.Current) :  // 値があったら場合、成功
                stm.失敗(default(T));         // 値がない場合、失敗

        public static パーサー<T, T> 値で確認<T>(T 確認する値) where T : IComparable => 関数で確認<T>(t => 0 == t.CompareTo(確認する値));

        public static パーサー<T, IEnumerable<T>> 値列で確認<T>(IEnumerable<T> 値列) where T : IComparable => 値列.Select(値で確認).指定順で全て();

        public static パーサー<T, T> 関数で確認<T>(Func<T, bool> 確認関数) =>
            from v1 in 任意の値<T>()
            where 確認関数(v1)
            select v1;

        public static パーサー<T, D> 関数で確認<T, D>(Func<D, bool> 確認関数) where D : class, T =>
            from v1 in 型で確認<T, D>()
            where 確認関数(v1)
            select v1;

        public static パーサー<T, D> 型で確認<T, D>() where D : class, T =>
            from v1 in 任意の値<T>()
            where v1 is D
            select v1 as D;

        public static パーサー<T, IEnumerable<D>> 指定順で全て<T, D>(this IEnumerable<パーサー<T, D>> parsers)
        {
            YCONV<IEnumerable<パーサー<T, D>>, パーサー<T, IEnumerable<D>>> lambda =
                f => ps =>
                    !ps.Any() ?
                        Enumerable.Empty<D>().Success(default(T)) :
                        from head in ps.First()
                        from tail in f(ps.Skip(1))
                        select (new[] { head }).Concat(tail);

            return lambda.YConv(parsers);
        }

        public static パーサー<T, IEnumerable<D>> 指定区切りでの繰り返し<T, D, S>(this パーサー<T, D> parser, パーサー<T, S> sep)
        {
            YCONV<パーサー<T, IEnumerable<D>>> lambda =
                f => () =>
                    from セパレータの取得に成功したか in sep.ToGetResult()
                    let ignore = !セパレータの取得に成功したか
                    from head in parser.SkipIf(ignore)
                    from tail in f().SkipIf(ignore)
                    select ignore ? Enumerable.Empty<D>() : new[] { head }.Concat(tail);

            return
                from head in parser.ToGetResultAndData() //  最初のparserの失敗は成功に倒す
                let ignore = !head.result
                from tail in lambda.YConv().SkipIf(ignore)
                select ignore ? Enumerable.Empty<D>() : new[] { head.data }.Concat(tail);
        }

        public static パーサー<T, D> 何れか一つ<T, D>(IEnumerable<パーサー<T, D>> parsers)
        {
            YCONV<IEnumerable<パーサー<T, D>>, パーサー<T, D>> lambda =
                f => ps =>
                    !ps.Any() ?
                        (stm1 => stm1.失敗(default(D))) :
                        from head in ps.First().ToGetResultAndData()
                        from tail in f(ps.Skip(1)).SkipIf(head.result)
                        select head.result ? head.data : tail;

            return lambda.YConv(parsers);
        }

        public static パーサー<T, D> 何れか一つ<T, D>(params パーサー<T, D>[] parsers) =>
            何れか一つ<T, D>(parsers.AsEnumerable());

        public static パーサー<T, IEnumerable<D>> 零回以上の繰り返し<T, D>(this パーサー<T, D> parser)
        {
            YCONV<パーサー<T, IEnumerable<D>>> lambda =
                f => () =>
                (
                    from head in parser
                    from tail in f()
                    select new[] { head }.Concat(tail)
                ).IfFailedReturn(Enumerable.Empty<D>());

            return lambda.YConv();
        }

        public static パーサー<T, IEnumerable<D>> 一回以上の繰り返し<T, D>(this パーサー<T, D> parser) =>
            from v1 in parser
            from v2 in parser.零回以上の繰り返し()
            select new[] { v1 }.Concat(v2);

        public static パーサー<T, IEnumerable<D>> 指定回数の繰り返し<T, D>(this パーサー<T, D> parser, int count) =>
            Enumerable.Repeat(parser, count).指定順で全て();

        public static パーサー<T, Void> 末尾共通<T>() => stm =>
            (!stm.HasValue) ?
                stm.成功(new Void()) :    // 値がない場合、成功
                stm.失敗(default(Void));  // 値があったら場合、失敗

        public static パーサー<T, D> 末尾まで<T, D>(this パーサー<T, D> param) =>
            from v1 in param
            from v2 in 末尾共通<T>()
            select v1;
    }
}
