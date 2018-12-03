using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using East.Tool.UseCaseTranslator;
using East.Tool.UseCaseTranslator.Controllers;
using East.Tool.UseCaseTranslator.Models;

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace East.Tool.UseCaseTranslator.Tests
{
    /// <summary>
    /// コンソールアプリケーションエントリクラステスト
    /// </summary>
    [TestClass]
    public class ProgramTest
    {
        //
        // テストメソッド
        //

        /// <summary>
        /// 例外捕捉・出力テスト
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
        public void ProrgamOperateTest()
        {

            string[] catalogFileNames = {
                "無効ユースケースカタログ.yaml",
            };

            var path = Path.GetFullPath(".");
            foreach (var catalogFileName in catalogFileNames) {
                var errorOutput = string.Empty;
                using (var writer = new StringWriter()) {
                    System.Console.SetError(writer);

                    string[] args = {
                        "TestSuite",
                        "--input:" + catalogFileName,
                    };
                    Assert.IsFalse(Program.Operate(args, UseCaseTranslatorOperationType.ValueOf(args[0])));
                    errorOutput = writer.ToString();
                }
                Assert.IsFalse(string.IsNullOrWhiteSpace(errorOutput));
                Assert.IsTrue(errorOutput == "シナリオセットファイル\"無効YAML書式検証シナリオセット.yaml\"の書式に誤りがあります: (Line: 15, Col: 11, Idx: 258) - (Line: 15, Col: 36, Idx: 283): Anchor 'DuplicateAnchor' already defined\r\n");
            }
        }
    }
}
