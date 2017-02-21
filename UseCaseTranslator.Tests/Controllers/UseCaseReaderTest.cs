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

        /// <summary>
        /// 無効書式読み込みテスト
        /// </summary>
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
