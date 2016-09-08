using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using East.Tool.UseCaseTranslator.Models;

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace East.Tool.UseCaseTranslator.Controllers.Tests
{
    /// <summary>
    /// MarkdownConverterクラステスト
    /// </summary>
    [TestClass]
    public class MarkdownConverterTest
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// 参照ファイルと比較して検証する
        /// </summary>
        /// <param name="target">対象出力結果</param>
        /// <param name="path">パス</param>
        /// <param name="fileName">ファイル名</param>
        private static void AssertWithReferenceFile(string target, string path, string fileName)
        {
            var lines = target.Replace(Environment.NewLine, "\n").Split('\n');
            string[] referenceLines;
            using (var reader = new StreamReader(Path.Combine(path, "Reference." + fileName))) {
                referenceLines = reader.ReadToEnd().Replace(Environment.NewLine, "\n").Split('\n');
            }
            for (var index = 0; index < Math.Min(lines.Count(), referenceLines.Count()); ++index) {
                Trace.TraceInformation(lines[index]);
                Assert.IsTrue(lines[index] == referenceLines[index], string.Format("出力結果が想定と異なります: {0} <-> {1}", lines[index], referenceLines[index]));
            }
            Assert.IsTrue(lines.Count() == referenceLines.Count());
        }

        //
        // テストメソッド
        //

        /// <summary>
        /// テスト: CanOperateメソッド
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\ユースケースカタログテンプレート.md")]
        [DeploymentItem(@".\TestData\ユースケースシナリオセットテンプレート.md")]
        public void CanOperateTest()
        {
            var parameters = new Dictionary<string, object>();

            Assert.IsFalse(new MarkdownConverter(parameters).CanOperate());
            parameters.Add("input", "UseCaseTranslatorユースケースカタログ.yaml");
            Assert.IsTrue(new MarkdownConverter(parameters).CanOperate());

            parameters.Add("output", "NotExistPath");
            Assert.IsFalse(new MarkdownConverter(parameters).CanOperate());
            parameters["output"] = Path.GetFullPath(".");
            Assert.IsTrue(new MarkdownConverter(parameters).CanOperate());

            parameters.Add("apply", "invalidtemplat.md");
            Assert.IsFalse(new MarkdownConverter(parameters).CanOperate());
            parameters["apply"] = "invalidtemplat.md|invalidtemplat.md|invalidtemplat.md";
            Assert.IsFalse(new MarkdownConverter(parameters).CanOperate());
            parameters["apply"] = "invalidtemplat.md|invalidtemplat.md";
            Assert.IsFalse(new MarkdownConverter(parameters).CanOperate());
            parameters["apply"] = "ユースケースカタログテンプレート.md|ユースケースシナリオセットテンプレート.md";
            Assert.IsTrue(new MarkdownConverter(parameters).CanOperate());
        }

        /// <summary>
        /// テスト: デフォルトのテンプレートを利用した出力
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - Markdown.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - その他.yaml")]
        [DeploymentItem(@".\TestData\Reference.UseCaseTranslatorユースケースカタログ.md")]
        [DeploymentItem(@".\TestData\Reference.UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.md")]
        [DeploymentItem(@".\TestData\Reference.UseCaseTranslatorユースケースシナリオセット - Markdown.md")]
        [DeploymentItem(@".\TestData\Reference.UseCaseTranslatorユースケースシナリオセット - その他.md")]
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
                    {  "input", catalogFileName }
                };
                new MarkdownConverter(parameters).Operate();

                var generatedFiles = new List<string>();
                try {
                    var catalogMdFileName = Path.ChangeExtension(catalog.FileName, ".md");
                    var catalogMdPath = Path.Combine(path, catalogMdFileName);
                    Assert.IsTrue(File.Exists(catalogMdPath));
                    generatedFiles.Add(catalogMdPath);

                    string catalogMarkdown;
                    using (var reader = new StreamReader(catalogMdPath)) {
                        catalogMarkdown = reader.ReadToEnd();
                    }
                    Assert.IsNotNull(catalogMarkdown);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(catalogMarkdown));
                    AssertWithReferenceFile(catalogMarkdown, path, catalogMdFileName);

                    foreach (var scenarioSet in catalog.ScenarioSets) {
                        var scenarioSetMdFileName = Path.ChangeExtension(scenarioSet.FileName, ".md");
                        var scenarioSetMdPath = Path.Combine(path, scenarioSetMdFileName);
                        Assert.IsTrue(File.Exists(scenarioSetMdPath));
                        generatedFiles.Add(scenarioSetMdPath);

                        string scenarioSetMarkdown;
                        using (var reader = new StreamReader(scenarioSetMdPath)) {
                            scenarioSetMarkdown = reader.ReadToEnd();
                        }
                        Assert.IsNotNull(scenarioSetMarkdown);
                        Assert.IsFalse(string.IsNullOrWhiteSpace(scenarioSetMarkdown));
                        AssertWithReferenceFile(scenarioSetMarkdown, path, scenarioSetMdFileName);
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

        /// <summary>
        /// テスト: テンプレートを指定した出力
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - Markdown.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - その他.yaml")]
        [DeploymentItem(@".\TestData\Reference.UseCaseTranslatorユースケースカタログ.md")]
        [DeploymentItem(@".\TestData\Reference.UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.md")]
        [DeploymentItem(@".\TestData\Reference.UseCaseTranslatorユースケースシナリオセット - Markdown.md")]
        [DeploymentItem(@".\TestData\ユースケースカタログテンプレート.md")]
        [DeploymentItem(@".\TestData\ユースケースシナリオセットテンプレート.md")]
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
                    {  "input", catalogFileName },
                    {  "apply", "ユースケースカタログテンプレート.md|ユースケースシナリオセットテンプレート.md" }
                };
                new MarkdownConverter(parameters).Operate();
                var generatedFiles = new List<string>();
                try {
                    var catalogMdFileName = Path.ChangeExtension(catalog.FileName, ".md");
                    var catalogMdPath = Path.Combine(path, catalogMdFileName);
                    Assert.IsTrue(File.Exists(catalogMdPath));
                    generatedFiles.Add(catalogMdPath);

                    string catalogMarkdown;
                    using (var reader = new StreamReader(catalogMdPath)) {
                        catalogMarkdown = reader.ReadToEnd();
                    }
                    Assert.IsNotNull(catalogMarkdown);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(catalogMarkdown));
                    AssertWithReferenceFile(catalogMarkdown, path, catalogMdFileName);

                    foreach (var scenarioSet in catalog.ScenarioSets) {
                        var scenarioSetMdFileName = Path.ChangeExtension(scenarioSet.FileName, ".md");
                        var scenarioSetMdPath = Path.Combine(path, scenarioSetMdFileName);
                        Assert.IsTrue(File.Exists(scenarioSetMdPath));
                        generatedFiles.Add(scenarioSetMdPath);

                        string scenarioSetMarkdown;
                        using (var reader = new StreamReader(scenarioSetMdPath)) {
                            scenarioSetMarkdown = reader.ReadToEnd();
                        }
                        Assert.IsNotNull(scenarioSetMarkdown);
                        Assert.IsFalse(string.IsNullOrWhiteSpace(scenarioSetMarkdown));
                        AssertWithReferenceFile(scenarioSetMarkdown, path, scenarioSetMdFileName);
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
}
