using System;
using System.Collections.Generic;
using System.Linq;

namespace MyParser
{
    public readonly struct パーサー用ストリーム<T>
    {
        public パーサー用ストリーム(IEnumerator<T> paramEnum)
        {
            this.hasValue = paramEnum.MoveNext();
            this.current = (this.hasValue) ? paramEnum.Current : default;
            this.lazyNext = new Lazy<パーサー用ストリーム<T>>(() => new パーサー用ストリーム<T>(paramEnum));
        }

        readonly bool hasValue;
        readonly T current;
        readonly Lazy<パーサー用ストリーム<T>> lazyNext;

        public bool HasValue { get => this.hasValue; }
        public T Current { get => this.current; }
        public パーサー用ストリーム<T> Next { get => this.lazyNext.Value; }
    }
    public static partial class Util
    {
        // 文字列をパーサー用ストリームへ変換
        static パーサー用ストリーム<char> パーサー用ストリームへ変換(this string input) => input.AsEnumerable().パーサー用ストリームへ変換();
        // 任意のIEnumerableをパーサー用ストリームへ変換
        static パーサー用ストリーム<T> パーサー用ストリームへ変換<T>(this IEnumerable<T> input) => new パーサー用ストリーム<T>(input.GetEnumerator());
    }
}
