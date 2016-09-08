using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using East.Tool.UseCaseTranslator.Models;

using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator.Controllers
{
    /// <summary>
    /// ヘルプレポート
    /// </summary>
    public sealed class HelpReporter : UseCaseTranslatorOperator
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// ヘルプを出力する
        /// </summary>
        public static void ReportHelp()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();
            var usage = new List<string> {
                string.Format("{0} {1}({2})", assemblyName.Name, assemblyName.Version, File.GetLastWriteTimeUtc(assembly.Location).ToString("yyyyMMddTHHmmss")),
                string.Format("Usage: {0} [Command] [Options]", assemblyName.Name),
                "Command:",
            };
            usage.AddRange(UseCaseTranslatorOperationType.OperationTypes.Select(type => string.Format("\t{0} - {1}", type.OperationTypeLiteral, type.Description)));
            usage.Add("Options;");
            usage.AddRange(UseCaseTranslatorOperator.CommonParameters.Select(definition => string.Format("\t--{0}(-{1}) - {2}", definition.Name, definition.ShortName, definition.Description)));
            foreach (var line in usage) {
                Console.Error.WriteLine(line);
            }
        }

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public HelpReporter(IDictionary<string, object> parameters)
            : base(parameters, false)
        {
        }

        //
        // 再定義メソッド
        //

        /// <summary>
        /// 操作可能かを返す
        /// </summary>
        /// <returns>操作の可否</returns>
        protected override bool DoCanOperate()
        {
            return true;
        }

        /// <summary>
        /// 操作を実行する
        /// </summary>
        /// <param name="catalog">ユースケース</param>
        protected override void DoOperate(UseCaseCatalog catalog)
        {
            ReportHelp();
        }
    }
}
