using System.Collections.Generic;
using System.Linq;
using static MyParser.共通;

namespace MyParser
{
    public static class 文字パーサー
    {
        public static パーサー<char, char> 指定の一文字(char c) => 値で確認<char>(c);
        public static パーサー<char, char> 指定の文字の何れか(params char[] candidates) => 関数で確認<char>(c => candidates.Contains(c));
        public static パーサー<char, string> 指定の文字列の何れか(params string[] candidates) =>
            何れか一つ(candidates.Select(s => 文字列(s))).Select(e => e.文字列へ変換());
        public static パーサー<char, char> 指定以外の一文字(params char[] candidates) => 関数で確認<char>(c => !candidates.Contains(c));
        public static パーサー<char, char> 任意の一文字 => 関数で確認<char>(c => true);
        public static パーサー<char, char> 空白一文字 => 指定の文字の何れか(' ', '\t');
        public static パーサー<char, char> 数字一文字 => 指定の文字の何れか('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');
        public static パーサー<char, char> 英大文字一文字 => 指定の文字の何れか(
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
            'U', 'V', 'W', 'X', 'Y', 'Z'
        );
        public static パーサー<char, IEnumerable<char>> 文字列(string s) => 値列で確認<char>(s);
        public static パーサー<char, Void> 末尾 => 末尾共通<char>();
    }
}
