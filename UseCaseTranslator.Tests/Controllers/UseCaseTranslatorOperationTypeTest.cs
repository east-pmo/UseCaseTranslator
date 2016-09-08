using System;
using System.Collections.Generic;

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace East.Tool.UseCaseTranslator.Controllers.Tests
{
    /// <summary>
    /// UseCaseTranslatorOperationTypeクラステスト
    /// </summary>
    [TestClass]
    public class UseCaseTranslatorOperationTypeTest
    {
        //
        // クラスフィールド・プロパティ
        //

        /// <summary>
        /// 有効な種別表記の列挙
        /// </summary>
        private static readonly IEnumerable<string> validTypeLiterals = new string[] {
            "UseCaseCatalog",
            "TestSuite",
            "Help",
        };

        //
        // テストメソッド
        //

        /// <summary>
        /// テスト: ValueOfメソッド
        /// </summary>
        [TestMethod]
        public void ValueOfTest()
        {
            foreach (var typeLiteral in validTypeLiterals) {
                Assert.IsNotNull(UseCaseTranslatorOperationType.ValueOf(typeLiteral));
            }
            Assert.IsNull(UseCaseTranslatorOperationType.ValueOf("InvalidTypeLiteral"));
        }

        /// <summary>
        /// テスト: GetOperatorメソッド
        /// </summary>
        [TestMethod]
        public void GetOperatorTest()
        {
            string[] arguments = {
                "--input:UseCaseCatalog.yaml",
                "-a:UseCaseCatalogTemplate.xlsx",
                "-output:C:\\",
            };
            foreach (var typeLiteral in validTypeLiterals) {
                Assert.IsNotNull(UseCaseTranslatorOperationType.ValueOf(typeLiteral).GetOperator<UseCaseTranslatorOperator>(arguments));
            }
        }
    }
}
