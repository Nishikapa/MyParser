using System;
using System.Collections.Generic;
using System.Linq;

namespace MyParser
{
    public static class 共通
    {
        public static パーサー<T, T> 任意の値<T>() => stm =>
            (stm.HasValue) ?
                IResult実装<T, T>.成功(stm.Next, stm.Current) :    // 値がない場合、成功
                IResult実装<T, T>.失敗(stm);                 // 値があったら場合、失敗 => 関数で確認<T, D>(t => true);

        public static パーサー<T, T> 値で確認<T>(T 確認する値) where T : IComparable => 関数で確認<T>(t => 0 == t.CompareTo(確認する値));

        public static パーサー<T, IEnumerable<T>> 値列で確認<T>(IEnumerable<T> 値列) where T : IComparable => 値列.Select(値で確認).指定順で全て();

        public static パーサー<T, T> 関数で確認<T>(Func<T, bool> 確認関数) =>
            from v1 in 任意の値<T>()
            where 確認関数(v1)
            select v1;

        public static パーサー<T, D> 関数で確認<T, D>(Func<D, bool> 確認関数) where D : class, T =>
            from d1 in 型で確認<T, D>()
            where 確認関数(d1)
            select d1;

        public static パーサー<T, D> 型で確認<T, D>() where D : class, T =>
            from v1 in 任意の値<T>()
            where v1 is D
            let d1 = v1 as D
            select d1;

        public static パーサー<T, IEnumerable<D>> 指定順で全て<T, D>(this IEnumerable<パーサー<T, D>> parsers) => stm =>
        {
            bool _f = true;

            IEnumerable<D> 列挙関数()
            {
                foreach (var parser in parsers)
                {
                    D _data = default;

                    _f = parser(stm).結果取得関数呼び出し
                        (
                           成功時: (stm2, data) =>
                           {
                               _data = data;
                               stm = stm2;
                               return true;
                           },
                           失敗時: stm2 =>
                           {
                               stm = stm2;
                               return false;
                           }
                       );

                    if (!_f)
                    {
                        yield break;
                    }

                    yield return _data;
                }
            }

            var result = 列挙関数().ToList();

            return _f ?
                IResult実装<T, IEnumerable<D>>.成功(stm, result) :   // 全部成功した場合だけ成功
                IResult実装<T, IEnumerable<D>>.失敗(stm);            // 一個でも失敗したら失敗
        };

        public static パーサー<T, IEnumerable<D>> 指定区切りでの繰り返し<T, D, S>(this パーサー<T, D> parser, パーサー<T, S> sep) => stm =>
        {
            bool _f = true;

            IEnumerable<D> 列挙関数()
            {
                D _data = default;

                if (!parser(stm).結果取得関数呼び出し
                (
                    成功時: (_stm, data) =>
                    {
                        _data = data;
                        stm = _stm;
                        return true;
                    },
                    失敗時: _stm => false
                ))
                {
                    _f = true;
                    yield break;
                }

                yield return _data;

                while (true)
                {
                    if (!sep(stm).結果取得関数呼び出し
                        (
                            成功時: (_stm, data) =>
                            {
                                stm = _stm;
                                return true;
                            },
                            失敗時: _stm => false
                        )
                    )
                    {
                        _f = true;
                        yield break;
                    }

                    if (!parser(stm).結果取得関数呼び出し
                        (
                            成功時: (_stm, data) =>
                            {
                                _data = data;
                                stm = _stm;
                                return true;
                            },
                            失敗時: _stm =>
                            {
                                stm = _stm;
                                return false;
                            }
                        )
                    )
                    {
                        _f = false;
                        yield break;
                    }
                    yield return _data;
                }
            }

            var result = 列挙関数().ToList();

            return _f ?
                IResult実装<T, IEnumerable<D>>.成功(stm, result) :   // 全部成功した場合だけ成功
                IResult実装<T, IEnumerable<D>>.失敗(stm);            // 一個でも失敗したら失敗
        };

        public static パーサー<T, D> 何れか一つ<T, D>(IEnumerable<パーサー<T, D>> parsers) => stm =>
        {
            var s = default(パーサー用ストリーム<T>);

            foreach (var パーサー in parsers)
            {
                var 結果 = default(D);

                if (パーサー(stm).結果取得関数呼び出し(
                        成功時: (_stm, data) =>
                        {
                            結果 = data;
                            s = _stm;
                            return true;
                        },
                        失敗時: _stm =>
                        {
                            s = _stm;
                            return false;
                        }
                    )
                )
                {
                    return IResult実装<T, D>.成功(s, 結果);
                }
            }
            // 全部失敗したら失敗。
            return IResult実装<T, D>.失敗(stm);
        };

        public static パーサー<T, D> 何れか一つ<T, D>(params パーサー<T, D>[] parsers) =>
            何れか一つ<T, D>(parsers.AsEnumerable());

        public static パーサー<T, IEnumerable<D>> 零回以上の繰り返し<T, D>(this パーサー<T, D> parser) => stm =>
        {
            IEnumerable<D> 列挙関数()
            {
                D _data = default;

                while
                (
                    parser(stm).結果取得関数呼び出し
                    (
                        成功時: (stm2, data) =>
                        {
                            _data = data;
                            stm = stm2;
                            return true;
                        },
                        失敗時: stm2 => false
                    )
                )
                {
                    yield return _data;
                }
            }
            return IResult実装<T, IEnumerable<D>>.成功(stm, 列挙関数().ToList());
        };

        public static パーサー<T, IEnumerable<D>> 一回以上の繰り返し<T, D>(this パーサー<T, D> parser) =>
            from v1 in parser
            from v2 in parser.零回以上の繰り返し()
            select new[] { v1 }.Concat(v2);

        public static パーサー<T, IEnumerable<D>> 指定回数の繰り返し<T, D>(this パーサー<T, D> parser, int count) =>
            Enumerable.Repeat(parser, count).指定順で全て();

        public static パーサー<T, Void> 末尾共通<T>() => stm =>
            (!stm.HasValue) ?
                IResult実装<T, Void>.成功(stm, new Void()) :    // 値がない場合、成功
                IResult実装<T, Void>.失敗(stm);                 // 値があったら場合、失敗

        public static パーサー<T, D> 末尾まで<T, D>(this パーサー<T, D> param) =>
            from v1 in param
            from v2 in 末尾共通<T>()
            select v1;
    }
}
