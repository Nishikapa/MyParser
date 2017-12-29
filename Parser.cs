using System;
using System.Collections.Generic;

namespace MyParser
{
    public interface IResult<T, out D>
    {
        R 結果取得関数呼び出し<R>(
            Func<パーサー用ストリーム<T>, D, R> 成功時,
            Func<パーサー用ストリーム<T>, R> 失敗時
        );
    }
    public delegate IResult<T, D> パーサー<T, out D>(パーサー用ストリーム<T> stream);
    public readonly struct IResult実装<T, D> : IResult<T, D>
    {
        public static IResult<T, D> 成功(in パーサー用ストリーム<T> s, D d) => new IResult実装<T, D>(s, d, true);
        public static IResult<T, D> 失敗(in パーサー用ストリーム<T> s) => new IResult実装<T, D>(s, default, false);
        private IResult実装(in パーサー用ストリーム<T> s, D d, bool f)
        {
            this.stream = s;
            this.data = d;
            this.fSuccuess = f;
        }
        private readonly bool fSuccuess;
        private readonly パーサー用ストリーム<T> stream;
        private readonly D data;
        R IResult<T, D>.結果取得関数呼び出し<R>(
            Func<パーサー用ストリーム<T>, D, R> 成功時に呼び出す関数,
            Func<パーサー用ストリーム<T>, R> 失敗時に呼び出す関数
            ) =>
            this.fSuccuess ?
                成功時に呼び出す関数(this.stream, this.data) :
                失敗時に呼び出す関数(this.stream);
    }
    public class Void { }
    public static class LINQ用サポート
    {
        public static パーサー<T, S> Where<T, S>(this パーサー<T, S> パーサー, Func<S, bool> predicate) => stm =>
            パーサー(stm).結果取得関数呼び出し(
                成功時: (_stm, data) =>
                    predicate(data) ?
                    IResult実装<T, S>.成功(_stm, data) :
                    IResult実装<T, S>.失敗(_stm),
                失敗時: _stm =>
                    IResult実装<T, S>.失敗(_stm)
                );
        public static パーサー<T, R> Select<T, S, R>(this パーサー<T, S> パーサー, Func<S, R> selector) => stm =>
            パーサー(stm).結果取得関数呼び出し(
                成功時: (_stm, data) =>
                    IResult実装<T, R>.成功(_stm, selector(data)),
                失敗時: _stm =>
                    IResult実装<T, R>.失敗(_stm)
                );
        public static パーサー<T, R> SelectMany<T, S, C, R>(
            this パーサー<T, S> パーサー１,
            Func<S, パーサー<T, C>> パーサー２の生成,
            Func<S, C, R> resultSelector) => stm1 =>
                パーサー１(stm1).結果取得関数呼び出し
                (
                    成功時: (stm2, パーサー１の結果) =>
                        パーサー２の生成(パーサー１の結果)(stm2).結果取得関数呼び出し
                        (
                            成功時: (stm3, パーサー２の結果) =>
                                IResult実装<T, R>.成功(stm3, resultSelector(パーサー１の結果, パーサー２の結果)),
                            失敗時: stm3 =>
                                IResult実装<T, R>.失敗(stm3)
                        ),
                    失敗時: stm2 =>
                        IResult実装<T, R>.失敗(stm2)
                );
    }
    public static partial class Util
    {
        public static Func<IEnumerable<T>, D> パーサーを関数に変換<T, D>(this パーサー<T, D> parser) => stm =>
            parser(パーサー用ストリームへ変換(stm)).結果取得関数呼び出し
            (
                成功時:
                    (resultStream, data) =>
                        resultStream.HasValue ?
                            throw new Exception("すべてのデータが処理されておらず、データが残っています。") :
                            data,
                失敗時:
                    resultStream => throw new Exception("残念ながらパースに失敗してしまいました")
            );
    }
}
