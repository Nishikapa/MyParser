using MyParser;
using System.Collections.Generic;
using System.Linq;
using static MyParser.共通;
using static MyParser.文字パーサー;

namespace MyParser
{
    abstract class 既定 { }
    class コメント : 既定 { }
    class ブランク : 既定 { }
    class 繰り返し始まり : 既定
    {
        public int 繰り返し回数 { get; private set; }
        public 繰り返し始まり(int i)
        {
            this.繰り返し回数 = i;
        }
    }
    class 繰り返し終わり : 既定 { }
    class 表示 : 既定
    {
        public string 表示文字列 { get; private set; }
        public 表示(string s)
        {
            this.表示文字列 = s;
        }
    }

    class 繰り返し : 既定
    {
        public int 繰り返し回数 { get; private set; }
        public IEnumerable<既定> 処理 { get; private set; }
        public 繰り返し(int i, IEnumerable<既定> children)
        {
            this.繰り返し回数 = i;
            this.処理 = children;
        }
    }


    static partial class Program
    {
        // 一段目パーサー ////////////////////////////////////////////////

        static パーサー<char, コメント> コメントのパーサー =>
            from v1 in 文字列("//")
            from v2 in 任意の一文字.零回以上の繰り返し()
            from v3 in 末尾
            select new コメント();

        static パーサー<char, ブランク> ブランクのパーサー =>
            from v1 in 空白一文字.零回以上の繰り返し()
            from v2 in 末尾
            select new ブランク();

        static パーサー<char, 繰り返し始まり> 繰り返し始まりのパーサー =>
            from v1 in 文字列("#FOR")
            from v2 in 空白一文字.零回以上の繰り返し()
            from v3 in 数字一文字.一回以上の繰り返し()
            from v4 in 空白一文字.零回以上の繰り返し()
            from v5 in 末尾
            select new 繰り返し始まり(v3.数値へ変換());

        static パーサー<char, 繰り返し終わり> 繰り返し終わりのパーサー =>
            from v1 in 文字列("#ENDFOR")
            from v2 in 空白一文字.零回以上の繰り返し()
            from v3 in 末尾
            select new 繰り返し終わり();

        static パーサー<char, 表示> 表示のパーサー =>
            from v1 in 文字列("#PRINT")
            from v2 in 空白一文字.零回以上の繰り返し()
            from v3 in 任意の一文字.零回以上の繰り返し()
            from v4 in 末尾
            select new 表示(v3.文字列へ変換());

        static パーサー<char, 既定> 一行のパーサー =>
            何れか一つ<char, 既定>(
                コメントのパーサー,
                ブランクのパーサー,
                繰り返し始まりのパーサー,
                繰り返し終わりのパーサー,
                表示のパーサー
            );

        public static IEnumerable<既定> Remove<T>(this IEnumerable<既定> source) =>
            source.Where(item => !(item is T));

        // 二段目パーサー ////////////////////////////////////////////////

        static パーサー<既定, 繰り返し> 繰り返しのパーサー =>
            from v1 in 型で確認<既定, 繰り返し始まり>()
            from v2 in 何れか一つ<既定, 既定>
                (
                    型で確認<既定, 表示>(),
                    繰り返しのパーサー
                ).零回以上の繰り返し()
            from v3 in 型で確認<既定, 繰り返し終わり>()
            select new 繰り返し(v1.繰り返し回数, v2);

        static パーサー<既定, IEnumerable<既定>> 二段目のパーサー =>
            from v1 in 何れか一つ<既定, 既定>(
                    型で確認<既定, 表示>(),
                    繰り返しのパーサー
                ).零回以上の繰り返し()
            from v2 in 末尾共通<既定>()
            select v1;

        static void Main(string[] args)
        {
            var すべての行 = System.IO.File.ReadAllLines(@"..\..\test.txt");

            var パーサー関数 = 一行のパーサー.パーサーを関数に変換();

            var パースの結果 = すべての行.Select(パーサー関数).ToList();

            var ブランクとコメントを削除した結果 =
                パースの結果
                .Remove<コメント>()
                .Remove<ブランク>()
                .ToList()
                ;

            var 二段目パーサー関数 = 
                二段目のパーサー.パーサーを関数に変換();

            var 二段目パースの結果 = 
                二段目パーサー関数(ブランクとコメントを削除した結果).ToList();

            var 式木 = 
                既定のツリーから式木を生成(二段目パースの結果);

            //var 実行形式関数 = 式木.Compile();

            //実行形式関数();

            式木からExeの生成(式木);
        }
    }
}
