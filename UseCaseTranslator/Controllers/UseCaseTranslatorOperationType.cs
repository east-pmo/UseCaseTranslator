using System;
using System.Collections.Generic;

using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator.Controllers
{

    /// <summary>
    /// UseCaseTranslator操作種別
    /// </summary>
    public sealed class UseCaseTranslatorOperationType : OperationType
    {
        //
        // クラスフィールド・プロパティ
        //

        /// <summary>
        /// 種別の列挙
        /// </summary>
        internal static UseCaseTranslatorOperationType[] OperationTypes
        {
            get {
                return new[] {
                    new UseCaseTranslatorOperationType("UseCaseCatalog", Resources.Resources.CommandDescription_UseCaseCatalog, UseCaseTranslatorOperator.CommonParameters, (parameters => new MarkdownConverter(parameters))),
                    new UseCaseTranslatorOperationType("TestSuite", Resources.Resources.CommandDescription_TestSuite, UseCaseTranslatorOperator.CommonParameters, (parameters => new ExcelTestSuiteBuilder(parameters))),
                    new UseCaseTranslatorOperationType("Help", Resources.Resources.CommandDescription_Help, UseCaseTranslatorOperator.CommonParameters, (parameters => new HelpReporter(parameters))),
                };
            }
        }

        //
        // クラスメソッド
        //

        /// <summary>
        /// 表記から種別を返す
        /// </summary>
        /// <param name="typeLiteral">表記</param>
        /// <returns>種別(存在しないときnull)</returns>
        public static UseCaseTranslatorOperationType ValueOf(string typeLiteral)
        {
            Contract.Requires(string.IsNullOrWhiteSpace(typeLiteral) == false);

            return ValueOf(OperationTypes, typeLiteral);
        }

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="typeLiteral">操作の表記</param>
        /// <param name="description">説明</param>
        /// <param name="paramDefs">パラメーター定義</param>
        /// <param name="createOperatorFunction">オペレーター生成関数</param>
        private UseCaseTranslatorOperationType(string typeLiteral, string description, IEnumerable<ParameterDefinition> paramDefs, Func<IDictionary<string, object>, Operator> createOperatorFunction)
            : base(typeLiteral, description, paramDefs, createOperatorFunction)
        {
        }
    }
}
