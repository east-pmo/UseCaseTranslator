using System;
using System.Linq;

using East.Tool.UseCaseTranslator.Controllers;

using System.Diagnostics.CodeAnalysis;

namespace East.Tool.UseCaseTranslator
{
    /// <summary>
    /// エントリ
    /// </summary>
    [ExcludeFromCodeCoverage]
    class Program
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// エントリ
        /// </summary>
        /// <param name="args">コマンドライン引数</param>
        static void Main(string[] args)
        {
            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0])) {
                HelpReporter.ReportHelp();
                Environment.Exit(1);
            }
            var operationType = UseCaseTranslatorOperationType.ValueOf(args[0]);
            if (operationType == null) {
                HelpReporter.ReportHelp();
                Environment.Exit(1);
            }
            var tuple = operationType.GetOperator<UseCaseTranslatorOperator>(args.Skip(1).Select(arg => arg.Trim('"')));
            if (tuple.Item2.Any()) {
                foreach (var invalidParameter in tuple.Item2) {
                    Console.WriteLine(Resources.Resources.Message_Format_InvalidParameter, invalidParameter);
                }
                Environment.Exit(1);
            }
            var op = tuple.Item1;
            if (op == null) {
                HelpReporter.ReportHelp();
                Environment.Exit(1);
            }

            if (op.CanOperate() == false) {
                // エラーの内容はここまでで表示されている
                Environment.Exit(1);
            }
            op.Operate();
            Environment.Exit(op is HelpReporter ? 1 : 0);
        }
    }
}
