using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using System.Diagnostics.Contracts;

namespace East.Tool
{
    /// <summary>
    /// 操作種別
    /// </summary>
    public abstract class OperationType
    {
        //
        // 内部型定義
        //

        /// <summary>
        /// パラメーター定義
        /// </summary>
        public sealed class ParameterDefinition
        {
            //
            // フィールド・プロパティ
            //

            /// <summary>
            /// 値指定
            /// </summary>
            internal readonly bool ValueSpecified;

            /// <summary>
            /// パラメーター名
            /// </summary>
            internal readonly string Name;

            /// <summary>
            /// パラメーター短縮名
            /// </summary>
            internal readonly char ShortName;

            /// <summary>
            /// 説明
            /// </summary>
            internal readonly string Description;

            //
            // メソッド
            //

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="valueSpecified">値指定</param>
            /// <param name="name">パラメーター名</param>
            /// <param name="shortName">パラメーター短縮名</param>
            /// <param name="description">説明</param>
            internal ParameterDefinition(bool valueSpecified, string name, char shortName, string description)
            {
                Contract.Requires(string.IsNullOrWhiteSpace(name) == false);
                Contract.Requires(string.IsNullOrWhiteSpace(description) == false);

                ValueSpecified = valueSpecified;
                Name = name;
                ShortName = shortName;
                Description =description;
            }
        }

        //
        // クラスメソッド
        //

        /// <summary>
        /// 表記に対応する値を返す
        /// </summary>
        /// <typeparam name="T">操作の型</typeparam>
        /// <param name="operationTypes">値の列挙</param>
        /// <param name="typeLiteral">値の表記</param>
        /// <returns>対応する値(存在しないときnull</returns>
        protected static T ValueOf<T>(IEnumerable<T> operationTypes, string typeLiteral)
            where T : OperationType
        {
            Contract.Requires(string.IsNullOrWhiteSpace(typeLiteral) == false);

            return operationTypes.SingleOrDefault(type => string.Compare(type.OperationTypeLiteral, typeLiteral, true) == 0);
        }

        //
        // フィールド・プロパティ
        //

        /// <summary>
        /// 種別の表記
        /// </summary>
        public readonly string OperationTypeLiteral;

        /// <summary>
        /// 説明
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// パラメーター定義
        /// </summary>
        private readonly IEnumerable<ParameterDefinition> parameterDefinitions;

        /// <summary>
        /// オペレーター生成
        /// </summary>
        private readonly Func<IDictionary<string, object>, Operator> createOperator;

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="typeLiteral">種別の表記</param>
        /// <param name="description">説明</param>
        /// <param name="paramDefs">パラメーター定義</param>
        /// <param name="createOperatorFunction">オペレーター生成関数</param>
        protected OperationType(string typeLiteral, string description, IEnumerable<ParameterDefinition> paramDefs, Func<IDictionary<string, object>, Operator> createOperatorFunction)
        {
            Contract.Requires(string.IsNullOrWhiteSpace(typeLiteral) == false);
            Contract.Requires(createOperatorFunction != null);
            Contract.Requires(paramDefs != null);

            OperationTypeLiteral = typeLiteral;
            Description = description;
            parameterDefinitions = paramDefs;
            createOperator = createOperatorFunction;
        }

        /// <summary>
        /// オペレーターを返す
        /// </summary>
        /// <param name="arguments">引数</param>
        /// <returns>生成したオペレーター</returns>
        public Tuple<T, IList<string>> GetOperator<T>(IEnumerable<string> arguments)
            where T : Operator
        {
            Contract.Requires(arguments != null);
            Contract.Ensures(Contract.Result<Tuple<T, IList<string>>>() != null);

            var parameters = GetParameter(arguments);
            return new Tuple<T, IList<string>>(parameters.Item2.Any() ? null : createOperator(parameters.Item1) as T, parameters.Item2);
        }

        /// <summary>
        /// パラメーターを返す
        /// </summary>
        /// <param name="arguments">引数</param>
        /// <returns>パラメーターとその値の辞書</returns>
        private Tuple<IDictionary<string, object>, IList<string>> GetParameter(IEnumerable<string> arguments)
        {
            Contract.Requires(arguments != null);
            Contract.Ensures(Contract.Result<Tuple<IDictionary<string, object>, IList<string>>>() != null);

            var parameters = new Dictionary<string, object>();
            var invalidArguments = new List<string>();
            foreach (var argument in arguments) {
                var accept = false;
                foreach (var parameter in parameterDefinitions) {
                    if (argument.StartsWith("--" + parameter.Name, true, CultureInfo.CurrentCulture)
                    || argument.StartsWith("-" + parameter.ShortName, true, CultureInfo.CurrentCulture)) {
                        if (parameter.ValueSpecified) {
                            var splitPos = argument.IndexOf(":");
                            if (0 <= splitPos) {
                                var value = argument.Substring(splitPos + 1);
                                if (0 < value.Length) {
                                   parameters.Add(parameter.Name, value);
                                }
                            }
                        }
                        else {
                            parameters.Add(parameter.Name, null);
                        }
                    }
                    if (parameters.ContainsKey(parameter.Name)) {
                        accept = true;
                        break;
                    }
                }
                if (accept == false) {
                    invalidArguments.Add(argument);
                }
            }

            return new Tuple<IDictionary<string, object>, IList<string>>(parameters, invalidArguments);
        }
    }
}
