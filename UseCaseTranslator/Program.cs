using System;
using System.Linq;

using East.Tool.UseCaseTranslator.Controllers;

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator
{
    /// <summary>
    /// エントリ
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Program
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// 操作する
        /// </summary>
        /// <param name="args">引数</param>
        /// <param name="operationType">操作種別</param>
        /// <returns>操作の成否</returns>
        public static bool Operate(string[] args, UseCaseTranslatorOperationType operationType)
        {
            Contract.Requires(args != null);

            var success = false;
            try {
                var tuple = operationType.GetOperator<UseCaseTranslatorOperator>(args.Skip(1).Select(arg => arg.Trim('"')));
                if (tuple.Item2.Any()) {
                    foreach (var invalidParameter in tuple.Item2) {
                        Console.WriteLine(Resources.Resources.Message_Format_InvalidParameter, invalidParameter);
                    }
                    return false;
                }

                var op = tuple.Item1;
                if (op == null) {
                    HelpReporter.ReportHelp();
                    return false;
                }

                if (op.CanOperate()) {
                    op.Operate();
                    success = true && (op is HelpReporter) == false;
                }
            }
            catch (ApplicationException e) {
                Console.Error.WriteLine(e.Message);
            }
            return success;
        }

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

            Environment.Exit(Operate(args, operationType) == false ? 1 : 0);
        }
    }
}
