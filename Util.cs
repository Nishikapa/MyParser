using System.Collections.Generic;

namespace MyParser
{
    public static partial class Util
    {
        public static int 数値へ変換(this char c) => int.Parse(c.ToString());
        public static int 数値へ変換(this IEnumerable<char> v) => int.Parse(string.Concat(v));
        public static string 文字列へ変換(this IEnumerable<char> param) => string.Concat(param);
    }
}
