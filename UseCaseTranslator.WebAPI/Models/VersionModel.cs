using System;
using East.Tool.UseCaseTranslator.Controllers;

namespace East.Tool.UseCaseTranslator.WebAPI.Models
{
    /// <summary>
    /// バージョン情報
    /// </summary>
    public sealed class VersionModel
    {
        //
        // フィールド・プロパティ
        //

        /// <summary>
        /// バージョン
        /// </summary>
        public string version
        {
            get {
                return HelpReporter.GetAssemblyVersionString();
            }
        } 

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VersionModel()
        {
        }
    }
}