using System;
using System.IO;
using System.Text;

using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator.Controllers
{
    /// <summary>
    /// 各種ユーティリティー
    /// </summary>
    public static class Utilities
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// Unicode正規化によるファイルパス正規化を試みる
        /// </summary>
        /// <param name="path">対象のパス</param>
        /// <returns>正規化した文字列</returns>
        /// <remarks>
        /// Mac OS Xはファイル保存時に独自UnicodeE正規化(NFDの変形)を行うため、ユースケースシナリオに記述したファイル名と
        /// 実際のファイルの名前のコードポイントの並びが異なってしまうことがある。NFDの変形とはいえ正しいNFDとの差異は
        /// 一般的なファイル名ではおおきくはないため、正規化適用での救済を試みる
        /// </remarks>
        public static string TryToNormalizeFilePath(string path)
        {
            Contract.Requires(string.IsNullOrWhiteSpace(path) == false);

            var strictPath = path;
            var forms = (NormalizationForm[])Enum.GetValues(typeof(NormalizationForm));
            var count = 0;
            while (count < forms.Length && File.Exists(strictPath) == false) {
                strictPath = path.Normalize(forms[count]);
                ++count;
            }
            return count < forms.Length ? strictPath : path;
        }
    }
}
