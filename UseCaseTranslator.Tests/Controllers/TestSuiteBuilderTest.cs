using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using ClosedXML.Excel;
using East.Tool.UseCaseTranslator.Models;

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace East.Tool.UseCaseTranslator.Controllers.Tests
{
    /// <summary>
    /// CsvFileTestSuiteBuilderクラステスト
    /// </summary>
    [TestClass]
    public class CsvFileTestSuiteBuilderTest
    {
        //
        // テストメソッド
        //

        /// <summary>
        /// 出力テスト
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - Markdown.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - その他.yaml")]
        public void OperateTest()
        {
            string[] catalogFileNames = {
                "UseCaseTranslatorユースケースカタログ.yaml",
            };

            var path = Path.GetFullPath(".");
            foreach (var catalogFileName in catalogFileNames) {
                UseCaseCatalog catalog;
                using (var reader = new StreamReader(catalogFileName)) {
                    catalog = new UseCaseReader().ReadFrom(reader, catalogFileName, File.GetLastWriteTime(catalogFileName));
                }

                var parameters = new Dictionary<string, object> {
                    { "input", catalogFileName },
                };
                var op = new CsvFileTestSuiteBuilder(parameters);
                op.Operate();

                // CSVはシナリオごとにファイル生成
                var generatedFiles = new List<string>();
                try {
                    foreach (var scenarioSet in catalog.ScenarioSets) {
                        var fileName = string.Format("{0}-テストスイート-{1}.csv", catalog.Title, scenarioSet.Title);
                        var scenarioSetPath = Path.Combine(path, fileName);
                        Assert.IsTrue(File.Exists(scenarioSetPath));
                        generatedFiles.Add(scenarioSetPath);

                        // 前後の空白をトリムするので注意
                        using (var parser = new TextFieldParser(scenarioSetPath)) {
                            parser.SetDelimiters(",");

                            Assert.IsFalse(parser.EndOfData);

                            var asHeader = true;
                            var scenarioIndex = 0;
                            var actionIndex = 0;
                            UseCaseScenario scenario = null;
                            UseCaseScenarioAction action = null;
                            while (parser.EndOfData == false) {
                                var fields = parser.ReadFields();
                                Assert.IsTrue(fields.Length == 9);
                                if (asHeader) {
                                    asHeader = false;

                                    Assert.IsTrue(fields[0] == "テストケース名");
                                    Assert.IsTrue(fields[1] == "サマリー");
                                    Assert.IsTrue(fields[2] == "Preconditions");
                                    Assert.IsTrue(fields[3] == "アクションNo");
                                    Assert.IsTrue(fields[4] == "アクション");
                                    Assert.IsTrue(fields[5] == "期待結果");
                                    Assert.IsTrue(fields[6] == "実行種別");
                                    Assert.IsTrue(fields[7] == "結果");
                                    Assert.IsTrue(fields[8] == "備考・説明");
                                }
                                else {
                                    if (string.IsNullOrWhiteSpace(fields[0]) == false) {
                                        scenario = scenarioSet.Scenarios.Skip(scenarioIndex).First();
                                        ++scenarioIndex;
                                        actionIndex = 0;
                                        Assert.IsTrue(fields[0] == scenario.Title);
                                        Assert.IsTrue(fields[1] == scenario.Summary);
                                        Assert.IsTrue(fields[2] == TestSuiteBuilder.ConvertCollectionValue(scenario.Preconditions));
                                    }
                                    action = scenario.Actions.Skip(actionIndex).First();
                                    ++actionIndex;
                                    Assert.IsTrue(fields[3] == actionIndex.ToString());
                                    Assert.IsTrue(fields[4] == action.Action);
                                    Assert.IsTrue(fields[5] == TestSuiteBuilder.ConvertCollectionValue(action.Results));
                                    Assert.IsTrue(fields[6] == "手動");
                                    Assert.IsTrue(string.IsNullOrWhiteSpace(fields[7]));
                                    Assert.IsTrue(string.IsNullOrWhiteSpace(fields[8]));
                                }
                            }
                        }
                    }
                }
                finally {
                    foreach (var file in generatedFiles) {
                        try {
                            File.Delete(file);
                        }
                        catch {
                            // Do nothing.
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// ExcelTestSuiteBuilderクラステスト
    /// </summary>
    [TestClass]
    public class ExcelTestSuiteBuilderTest
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// 結果を検証する
        /// </summary>
        /// <param name="path">出力ファイルパス</param>
        /// <param name="catalog">ユースケースカタログ</param>
        /// <param name="op">テストスイートビルダー</param>
        private static void AssertResult(string path, UseCaseCatalog catalog, ExcelTestSuiteBuilder op)
        {
            op.Operate();

            var fileName = string.Format("{0}-テストスイート.xlsx", catalog.Title);
            var testSuitePath = Path.Combine(path, fileName);
            Assert.IsTrue(File.Exists(testSuitePath));
            try {
                using (var testSuite = new XLWorkbook(testSuitePath)) {
                    Assert.IsTrue(2 <= testSuite.Worksheets.Count());

                    var summarySheet = testSuite.Worksheets.First();

                    Assert.IsTrue(summarySheet.Cell(1, 1).Value.ToString() == string.Format("{0} テストスイート", catalog.Title));
                    Assert.IsTrue(summarySheet.Cell(2, 1).Value.ToString() == string.Format("最終更新日時: {0:yyyy-MM-dd}", catalog.LastUpdateTime));

                    var scenarioSetIndex = 0;
                    foreach (var testCaseSetSheet in testSuite.Worksheets.Skip(1)) {
                        var scenarioSet = catalog.ScenarioSets.Skip(scenarioSetIndex).First();
                        ++scenarioSetIndex;

                        Assert.IsTrue(testCaseSetSheet.Cell(1, 2).Value.ToString() == scenarioSet.Title);
                        Assert.IsTrue(testCaseSetSheet.Cell(2, 2).Value.ToString() == scenarioSet.Summary);

                        var scenarioIndex = 0;
                        var actionIndex = 0;
                        UseCaseScenario scenario = null;
                        UseCaseScenarioAction action = null;
                        var rowIndex = 6;
                        while ((scenarioIndex + 1) < scenarioSet.Scenarios.Count() || (actionIndex + 1) < scenario.Actions.Count()) {
                            var row = testCaseSetSheet.Row(rowIndex);
                            ++rowIndex;
                            if (row.CellCount() == 0 || row.Cells().All(cell => string.IsNullOrWhiteSpace(cell.Value.ToString()))) {
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(row.Cell(1).Value.ToString()) == false) {
                                scenario = scenarioSet.Scenarios.Skip(scenarioIndex).First();
                                ++scenarioIndex;
                                actionIndex = 0;
                                Assert.IsTrue(row.Cell(1).Value.ToString() == scenario.Title);
                                Assert.IsTrue(row.Cell(2).Value.ToString() == scenario.Summary);
                                var testCasePreconditions = row.Cell(3).Value.ToString();
                                foreach (var precondition in scenario.Preconditions) {
                                    Assert.IsTrue(testCasePreconditions.Contains(precondition));
                                }
                            }
                            action = scenario.Actions.Skip(actionIndex).First();
                            ++actionIndex;
                            Assert.IsTrue(row.Cell(4).Value.ToString() == actionIndex.ToString());
                            Assert.IsTrue(row.Cell(5).Value.ToString() == action.Action);
                            var testCaseResults = row.Cell(6).Value.ToString();
                            foreach (var result in action.Results) {
                                Assert.IsTrue(testCaseResults.Contains(result));
                            }
                            Assert.IsTrue(row.Cell(7).Value.ToString() == "手動");
                            Assert.IsTrue(string.IsNullOrWhiteSpace(row.Cell(8).Value.ToString()));
                            Assert.IsTrue(string.IsNullOrWhiteSpace(row.Cell(9).Value.ToString()));
                        }
                    }
                }
            }
            finally
            {
                try {
                    File.Delete(testSuitePath);
                }
                catch {
                    // Do nothing.
                }
            }
        }

        //
        // テストメソッド
        //

        /// <summary>
        /// テスト: CanOperateメソッド
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\テストスイートテンプレート.xlsx")]
        public void CanOperateTest()
        {
            var parameters = new Dictionary<string, object>();

            Assert.IsFalse(new ExcelTestSuiteBuilder(parameters).CanOperate());
            parameters.Add("input", "UseCaseTranslatorユースケースカタログ.yaml");
            Assert.IsTrue(new ExcelTestSuiteBuilder(parameters).CanOperate());

            parameters.Add("output", "NotExistPath");
            Assert.IsFalse(new ExcelTestSuiteBuilder(parameters).CanOperate());
            parameters["output"] = Path.GetFullPath(".");
            Assert.IsTrue(new ExcelTestSuiteBuilder(parameters).CanOperate());

            parameters.Add("apply", "invalidtemplat.xlsxd");
            Assert.IsFalse(new ExcelTestSuiteBuilder(parameters).CanOperate());
            parameters["apply"] = "テストスイートテンプレート.xlsx";
            Assert.IsTrue(new ExcelTestSuiteBuilder(parameters).CanOperate());
        }

        /// <summary>
        /// テスト: デフォルトのテンプレートを利用した出力
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - Markdown.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - その他.yaml")]
        public void OperateWithDefaultTemplateTest()
        {
            string[] catalogFileNames = {
                "UseCaseTranslatorユースケースカタログ.yaml",
            };

            var path = Path.GetFullPath(".");
            foreach (var catalogFileName in catalogFileNames) {
                UseCaseCatalog catalog;
                using (var reader = new StreamReader(catalogFileName)) {
                    catalog = new UseCaseReader().ReadFrom(reader, catalogFileName, File.GetLastWriteTime(catalogFileName));
                }
                var parameters = new Dictionary<string, object> {
                    { "input", catalogFileName },
                };
                AssertResult(path, catalog, new ExcelTestSuiteBuilder(parameters));
            }
        }

        /// <summary>
        /// テスト: テンプレートを指定した出力
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - Markdown.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - その他.yaml")]
        [DeploymentItem(@".\TestData\テストスイートテンプレート.xlsx")]
        public void OperateWithExternalTemplateTest()
        {
            string[] catalogFileNames = {
                "UseCaseTranslatorユースケースカタログ.yaml",
            };

            var path = Path.GetFullPath(".");
            foreach (var catalogFileName in catalogFileNames) {
                UseCaseCatalog catalog;
                using (var reader = new StreamReader(catalogFileName)) {
                    catalog = new UseCaseReader().ReadFrom(reader, catalogFileName, File.GetLastWriteTime(catalogFileName));
                }

                var parameters = new Dictionary<string, object> {
                    { "input", catalogFileName },
                    { "apply", Path.Combine(path, "テストスイートテンプレート.xlsx") },
                };
                AssertResult(path, catalog, new ExcelTestSuiteBuilder(parameters));
            }
        }
    }
}
