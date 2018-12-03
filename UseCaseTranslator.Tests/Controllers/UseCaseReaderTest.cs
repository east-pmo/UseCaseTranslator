using System;
using System.IO;
using System.Linq;

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace East.Tool.UseCaseTranslator.Controllers.Tests
{
    /// <summary>
    /// ユースケースカタログテスト
    /// </summary>
    [TestClass]
    public class UseCaseReaderTest
    {
        //
        // テストメソッド
        //

        /// <summary>
        /// 読みこみテスト
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - Markdown.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - その他.yaml")]
        public void ReadFromTest()
        {
            string[] catalogYamlFiles = {
                "UseCaseTranslatorユースケースカタログ.yaml",
            };

            foreach (var catalogYamlFile in catalogYamlFiles) {
                using (var reader = new StreamReader(catalogYamlFile)) {
                    var catalog = new UseCaseReader().ReadFrom(reader, catalogYamlFile, File.GetLastWriteTime(catalogYamlFile));

                    Assert.IsNotNull(catalog);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(catalog.Title));
                    Assert.IsNotNull(catalog.ScenarioSets);
                    Assert.IsTrue(0 < catalog.ScenarioSets.Count());
                    foreach (var scenarioSet in catalog.ScenarioSets) {
                        Assert.IsFalse(string.IsNullOrWhiteSpace(scenarioSet.Title));
                        Assert.IsFalse(string.IsNullOrWhiteSpace(scenarioSet.Summary));
                        Assert.IsNotNull(scenarioSet.Scenarios);
                        Assert.IsTrue(0 < scenarioSet.Scenarios.Count());
                        foreach (var scenario in scenarioSet.Scenarios) {
                            Assert.IsFalse(string.IsNullOrWhiteSpace(scenario.Title));
                            Assert.IsFalse(string.IsNullOrWhiteSpace(scenario.Summary));
                            Assert.IsNotNull(scenario.Actions);
                            Assert.IsTrue(0 < scenario.Actions.Count());
                            foreach (var action in scenario.Actions) {
                                Assert.IsFalse(string.IsNullOrWhiteSpace(action.Action));
                                Assert.IsNotNull(action.Results);
                                Assert.IsTrue(0 < action.Results.Count());
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@".\TestData\無効ユースケースカタログ(2).yaml")]
        [DeploymentItem(@".\TestData\マッピングキー重複検証シナリオセット.yaml")]
        [ExpectedException(typeof(ApplicationException))]
        public void ReadInvalidMappingKeyTest()
        {
            string[] catalogYamlFiles = {
                "無効ユースケースカタログ(2).yaml",
            };

            foreach (var catalogYamlFile in catalogYamlFiles) {
                using (var reader = new StreamReader(catalogYamlFile)) {
                    var catalog = new UseCaseReader().ReadFrom(reader, catalogYamlFile, File.GetLastWriteTime(catalogYamlFile));
                }
            }
        }

        /// <summary>
        /// 無効書式読み込みテスト
        /// </summary>
        /// <remarks>
        /// YamlDotNetは4.1.0でYAML 1.1および1.2の仕様に準拠して重複エイリアスを許容する(あとから登場したほうを
        /// 有効とみなす)よう処理を変更した。
        /// [Allow anchors redefinition/overwriting #222](https://github.com/aaubry/YamlDotNet/pull/222)
        /// 本ユニットテスト記述時はYamlDotNet 4.0.0を参照していた。用をなさなくなったユニットテストだが、備忘録
        /// として無効化して残す
        /// </remarks>
        [Ignore]
        [TestMethod]
        [DeploymentItem(@".\TestData\無効ユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\無効YAML書式検証シナリオセット.yaml")]
        [ExpectedException(typeof(ApplicationException))]
        public void ReadInvalidFormatTest()
        {
            string[] catalogYamlFiles = {
                "無効ユースケースカタログ.yaml",
            };

            foreach (var catalogYamlFile in catalogYamlFiles) {
                using (var reader = new StreamReader(catalogYamlFile)) {
                    var catalog = new UseCaseReader().ReadFrom(reader, catalogYamlFile, File.GetLastWriteTime(catalogYamlFile));
                }
            }
        }
    }
}
