using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

using East.Tool.UseCaseTranslator.Models;

using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator.Controllers
{
    /// <summary>
    /// テストスイート構築抽象基本クラス
    /// </summary>
    public abstract class TestSuiteBuilder : UseCaseTranslatorOperator
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// 出力値に変換する
        /// </summary>
        /// <param name="value">対象値</param>
        /// <returns>出力値</returns>
        public static string ConvertCollectionValue(object value)
        {
            Contract.Requires(value != null);
            Contract.Ensures(Contract.Result<string>() != null);

            string convertedValue;
            if (value is IEnumerable<string>) {
                var enumValue = value as IEnumerable<string>;
                var count = enumValue.Count();
                if (1 < count) {
                    convertedValue = string.Join(Environment.NewLine, (value as IEnumerable<string>).Select(v => string.Format("* {0}", v)));
                }
                else if (count == 1){
                    convertedValue = enumValue.First();
                }
                else {
                    convertedValue = string.Empty;
                }
            }
            else if (value is string) {
                convertedValue = value as string;
            }
            else {
                throw new NotImplementedException();
            }
            return convertedValue;
        }

        /// <summary>
        /// テストケースの最初のアクションの値を返す
        /// </summary>
        /// <param name="scenario">ユースケースシナリオ</param>
        /// <returns>最初のアクションの値の列挙</returns>
        protected static IEnumerable<object> MakeTestCaseFirstActionValues(UseCaseScenario scenario)
        {
            Contract.Requires(scenario != null);
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null && Contract.Result<IEnumerable<object>>().Count() == 9);

            var firstAction = scenario.Actions.First();
            return new object[] {
                scenario.Title,
                scenario.Summary,
                ConvertCollectionValue(scenario.Preconditions),
                1,
                firstAction.Action,
                ConvertCollectionValue(firstAction.Results),
                Resources.Resources.Literal_TestSuite_Action_Manual,
                string.Empty,
                string.Empty,
            };
        }

        /// <summary>
        /// テストケースの二番目以降のアクションの値を返す
        /// </summary>
        /// <param name="actionNo">アクションNo</param>
        /// <param name="action">アクション</param>
        /// <returns>アクションの値の列挙</returns>
        protected static IEnumerable<object> MakeTestCaseFollowingActionValues(int actionNo, UseCaseScenarioAction action)
        {
            Contract.Requires(1 < actionNo);
            Contract.Requires(action != null);
            Contract.Ensures(Contract.Result<IEnumerable<object>>() != null && Contract.Result<IEnumerable<object>>().Count() == 9);

            return new object[] {
                string.Empty,
                string.Empty,
                string.Empty,
                actionNo,
                action.Action,
                ConvertCollectionValue(action.Results),
                Resources.Resources.Literal_TestSuite_Action_Manual,
                string.Empty,
                string.Empty,
            };
        }

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parameters">パラメーター</param>
        protected TestSuiteBuilder(IDictionary<string, object> parameters)
            : base(parameters)
        {
        }
    }

    /// <summary>
    /// CSVファイルテストスイート構築
    /// </summary>
    /// <remarks>
    /// * CSV形式で出力する。CSVの詳細は[RFC 4180](https://www.ietf.org/rfc/rfc4180.txt)に沿う
    ///     * ダブルクオーテーションを含むフィールドのみダブルクオーテーションで囲む
    ///     * ダブルクオーテーションのエスケープはダブルクオーテーションの二重化で行う
    ///     * 改行他の制御文字はそのままとする
    /// </remarks>
    public sealed class CsvFileTestSuiteBuilder : TestSuiteBuilder
    {
        //
        // クラスフィールド・プロパティ
        //

        /// <summary>
        /// ヘッダーフィールド
        /// </summary>
        private static readonly string[] headers = {
            Resources.Resources.Literal_TestSuite_Heading_TestCaseName,
            Resources.Resources.Literal_TestSuite_Heading_TestCaseSummary,
            Resources.Resources.Literal_TestSuite_Heading_TestCasePreconditions,
            Resources.Resources.Literal_TestSuite_Heading_TestCaseActionNo,
            Resources.Resources.Literal_TestSuite_Heading_TestCaseAction,
            Resources.Resources.Literal_TestSuite_Heading_TestCaseExpected,
            Resources.Resources.Literal_TestSuite_Heading_TestCaseType,
            Resources.Resources.Literal_TestSuite_Heading_TestCaseResult,
            Resources.Resources.Literal_TestSuite_Heading_TestCaseNote,
        };

        //
        // クラスメソッド
        //

        /// <summary>
        /// CSVフィールド向けに値を書式化する
        /// </summary>
        /// <param name="value">書式化対象値</param>
        /// <returns>書式化した値</returns>
        private static string FormatForCsvFields(string value)
        {
            Contract.Requires(value != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return value.Contains("\"") || value.Contains(Environment.NewLine) ? string.Format("\"{0}\"", value.Replace("\"", "\"\"")) : value;
        }

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parameters">パラメーター</param>
        public CsvFileTestSuiteBuilder(IDictionary<string, object> parameters)
            : base(parameters)
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
        override protected void DoOperate(UseCaseCatalog catalog)
        {
            // シナリオごとにファイル生成
            foreach (var scenarioSet in catalog.ScenarioSets) {
                var fileName = string.Format("{0}-テストスイート-{1}.csv", catalog.Title, scenarioSet.Title);
                var scenarioPath = Path.Combine(OutputDirectoryPath, fileName);
                Console.Error.WriteLine(Resources.Resources.Message_Format_WriteFileTo_TestSuiteCsv_ScenarioSet, scenarioPath);

                using (var writer = new StreamWriter(fileName)) {
                    // ヘッダー出力
                    writer.WriteLine(string.Join(",", headers));

                    foreach (var scenario in scenarioSet.Scenarios) {
                        // シナリオ出力
                        var actionNo = 1;
                        writer.WriteLine(string.Join(",", MakeTestCaseFirstActionValues(scenario).Select(value => FormatForCsvFields(value.ToString()))));
                        foreach (var action in scenario.Actions.Skip(1)) {
                            ++actionNo;
                            writer.WriteLine(string.Join(",", MakeTestCaseFollowingActionValues(actionNo, action).Select(value => FormatForCsvFields(value.ToString()))));
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Excelテストスイート構築
    /// </summary>
    public sealed class ExcelTestSuiteBuilder : TestSuiteBuilder
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// Excelテストスイートを指定ディレクトリに作成する
        /// </summary>
        /// <param name="catalog">対象ユースケースカタログ</param>
        /// <param name="outputDirectoryPath">出力ディレクトリ</param>
        /// <param name="templateFilePath">テンプレートのパス</param>
        /// <returns>出力ファイルの完全パス</returns>
        public static string CreateExcelTestSuiteTo(UseCaseCatalog catalog, string outputDirectoryPath, string templateFilePath)
        {
            Contract.Requires(catalog != null);
            Contract.Requires(string.IsNullOrWhiteSpace(outputDirectoryPath) == false && Directory.Exists(outputDirectoryPath));
            Contract.Ensures(string.IsNullOrWhiteSpace(Contract.Result<string>()) == false && File.Exists(Contract.Result<string>()));

            string outputFilePath = string.Empty;
            using (Stream stream = (string.IsNullOrWhiteSpace(templateFilePath) == false
                                    ? new FileStream(templateFilePath, FileMode.Open, FileAccess.Read) as Stream
                                    : new MemoryStream(Resources.Resources.テストスイートテンプレート) as Stream)) {
                using (var template = new XLWorkbook(stream, XLEventTracking.Disabled)) {
                    var summarySheet = template.Worksheet(1);
                    summarySheet.Cell(1, 1).SetValue(string.Format("{0} テストスイート", catalog.Title));
                    summarySheet.Cell(2, 1).SetValue(string.Format("最終更新日時: {0:yyyy-MM-dd}", catalog.LastUpdateTime));

                    var testCaseTemplateSheet = template.Worksheet(2);
                    var tooLongTitles = catalog.ScenarioSets.Select(scenarioSet => scenarioSet.Title).Where(title => 31 <= title.Length);
                    if (tooLongTitles.Any()) {
                        throw new ApplicationException(string.Format(Resources.Resources.Exception_Format_TooLongScenarioSetTitle, string.Join("\n\t", tooLongTitles)));
                    }
                    foreach (var scenarioSet in catalog.ScenarioSets) {
                        testCaseTemplateSheet.CopyTo(scenarioSet.Title);

                        var testCaseSetSheet = template.Worksheet(template.Worksheets.Count());
                        testCaseSetSheet.Cell(1, 2).SetValue(scenarioSet.Title);
                        testCaseSetSheet.Cell(2, 2).SetValue(scenarioSet.Summary);

                        var templateRow = testCaseSetSheet.Row(6);
                        IXLRow testCaseRow = null;
                        foreach (var scenario in scenarioSet.Scenarios) {
                            var firstActionValues = MakeTestCaseFirstActionValues(scenario).ToList();

                            var actionNo = 1;
                            testCaseRow = (testCaseRow != null ? testCaseRow.InsertRowsBelow(1).First() : templateRow);
                            for (var cellIndex = 1; cellIndex <= firstActionValues.Count(); ++cellIndex) {
                                testCaseRow.Cell(cellIndex).SetValue(firstActionValues[cellIndex - 1]);
                            }

                            foreach (var action in scenario.Actions.Skip(1)) {
                                ++actionNo;
                                var actionValues = MakeTestCaseFollowingActionValues(actionNo, action).ToList();

                                testCaseRow = testCaseRow.InsertRowsBelow(1).First();
                                for (var cellIndex = 1; cellIndex <= actionValues.Count(); ++cellIndex) {
                                    testCaseRow.Cell(cellIndex).SetValue(actionValues[cellIndex - 1]);
                                }
                            }

                            // 最後に空行を入れる
                            testCaseRow = testCaseRow.InsertRowsBelow(1).First();
                        }
                        testCaseRow.Delete();
                    }
                    template.Worksheet(2).Delete();

                    outputFilePath = Path.Combine(outputDirectoryPath, string.Format(Resources.Resources.FileName_Format_TestSuiteExcel, catalog.Title));
                    template.SaveAs(outputFilePath);
                }
            }
            return outputFilePath;
        }

        //
        // フィールド
        //

        /// <summary>
        /// テンプレートファイルのパス
        /// </summary>
        private string templateFilePath
        {
            get {
                return TemplateFileParam;
            }
        }

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parameters">パラメーター</param>
        public ExcelTestSuiteBuilder(IDictionary<string, object> parameters)
            : base(parameters)
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
            if (string.IsNullOrWhiteSpace(TemplateFileParam) == false) {
                var fullPath = Path.GetFullPath(TemplateFileParam);
                if (File.Exists(fullPath) == false) {
                    Console.Error.WriteLine(string.Format(Resources.Resources.Message_Format_NotFoundExcelTemplate, fullPath));
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 操作を実行する
        /// </summary>
        /// <param name="catalog">ユースケース</param>
        override protected void DoOperate(UseCaseCatalog catalog)
        {
            Console.Error.WriteLine(Resources.Resources.Message_Format_WriteFileTo_TestSuiteExcel, CreateExcelTestSuiteTo(catalog, OutputDirectoryPath, templateFilePath));
        }
    }
}
