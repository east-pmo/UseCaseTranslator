using System;
using System.Collections.Generic;
using System.IO;
using East.Tool.UseCaseTranslator.Models;

using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator.Controllers
{
    /// <summary>
    /// UseCaseTranslator操作抽象基本クラス
    /// </summary>
    public abstract class UseCaseTranslatorOperator : Operator
    {
        //
        // 定数
        //

        /// <summary>
        /// パラメーター: 入力ファイル
        /// </summary>
        protected const string PARAMETER_INPUT = "input";

        /// <summary>
        /// パラメーター: 出力ディレクトリ
        /// </summary>
        protected const string PARAMETER_OUTPUT = "output";

        /// <summary>
        /// パラメーター: 適用テンプレート
        /// </summary>
        protected const string PARAMETER_APPLY = "apply";

        //
        // クラスフィールド・プロパティ
        //

        /// <summary>
        /// 共通パラメーター定義
        /// </summary>
        internal static readonly IEnumerable<OperationType.ParameterDefinition> CommonParameters = new [] {
            new OperationType.ParameterDefinition(true, PARAMETER_INPUT, 'i', Resources.Resources.HelpDescription_Input),
            new OperationType.ParameterDefinition(true, PARAMETER_OUTPUT, 'o', Resources.Resources.HelpDescription_Output),
            new OperationType.ParameterDefinition(true, PARAMETER_APPLY, 'a', Resources.Resources.HelpDescription_ApplyTemplate),
        };

        //
        // フィールド・プロパティ
        //

        /// <summary>
        /// 実操作フラグ
        /// </summary>
        private readonly bool effective;

        /// <summary>
        /// ユースケースカタログのファイルパス
        /// </summary>
        protected string UseCaseCatalogFilePath
        {
            get {
                return OperationParameters.ContainsKey(PARAMETER_INPUT) && OperationParameters[PARAMETER_INPUT] is string
                        ? Path.GetFullPath(OperationParameters[PARAMETER_INPUT] as string): null;
            }
        }

        /// <summary>
        /// テンプレートファイルのパラメーター
        /// </summary>
        /// <remarks>指定文字列の解釈はオペレーションによって異なる</remarks>
        protected string TemplateFileParam
        {
            get {
                return OperationParameters.ContainsKey(PARAMETER_APPLY) ? OperationParameters[PARAMETER_APPLY] as string: null;
            }
        }

        /// <summary>
        /// 出力ディレクトリパス
        /// </summary>
        protected string OutputDirectoryPath
        {
            get {
                if (OperationParameters.ContainsKey(PARAMETER_OUTPUT)) {
                    var outputDirectoryPath = OperationParameters[PARAMETER_OUTPUT] as string;
                    if (string.IsNullOrWhiteSpace(outputDirectoryPath) == false) {
                        return outputDirectoryPath;
                    }
                }

                // 出力ディレクトリ指定が省略されたときは入力ファイルの存在するディレクトリ
                if (string.IsNullOrWhiteSpace(UseCaseCatalogFilePath) == false) {
                    return Path.GetDirectoryName(UseCaseCatalogFilePath);
                }

                // 指定がないときは実行ファイルのディレクトリ
                return Environment.CurrentDirectory;
            }
        }

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parameters">パラメーター</param>
        protected UseCaseTranslatorOperator(IDictionary<string, object> parameters)
            : this(parameters, true)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parameters">パラメーター</param>
        /// <param name="e">効果</param>
        protected UseCaseTranslatorOperator(IDictionary<string, object> parameters, bool e)
            : base(parameters)
        {
            effective = e;
        }

        /// <summary>
        /// ユースケースを読みこむ
        /// </summary>
        /// <returns>読みこんだユースケース</returns>
        private UseCaseCatalog ReadUseCase()
        {
            TextReader reader = null;
            string currentDirectory = null;
            Models.UseCaseCatalog catalog = null;
            try {
                DateTime lastUpdateTime;
                if (string.IsNullOrWhiteSpace(UseCaseCatalogFilePath)) {
                    reader = System.Console.In;
                    lastUpdateTime = DateTime.MinValue;
                }
                else {
                    currentDirectory = Path.GetFullPath(Environment.CurrentDirectory);
                    Environment.CurrentDirectory = Path.GetDirectoryName(UseCaseCatalogFilePath);
                    reader = new StreamReader(UseCaseCatalogFilePath);
                    lastUpdateTime = File.GetLastWriteTime(UseCaseCatalogFilePath);
                }
                catalog = new UseCaseReader().ReadFrom(reader, Path.GetFileName(UseCaseCatalogFilePath), lastUpdateTime);
            }
            finally {
                if (string.IsNullOrWhiteSpace(UseCaseCatalogFilePath) == false && reader != null) {
                    try {
                        reader.Close();
                    }
                    catch {
                        // Do nothing.
                    }
                }
                if (currentDirectory != null) {
                    Environment.CurrentDirectory = currentDirectory;
                }
            }
            return catalog;
        }

        //
        // 再定義メソッド
        //

        /// <summary>
        /// 操作可能かを返す
        /// </summary>
        /// <returns>操作の可否</returns>
        public override sealed bool CanOperate()
        {
            if (effective == false) {
                return true;
            }

            if (OperationParameters.ContainsKey(PARAMETER_INPUT) == false || string.IsNullOrWhiteSpace(UseCaseCatalogFilePath)) {
                Console.Error.WriteLine(Resources.Resources.Message_InputParameterIsRequired);
                return false;
            }
            var useCaseCatalogFilePath = Path.GetFullPath(UseCaseCatalogFilePath);
            if (File.Exists(useCaseCatalogFilePath) == false) {
                Console.Error.WriteLine(string.Format(Resources.Resources.Message_Format_NotFoundUseCaseCatalog, useCaseCatalogFilePath));
                return false;
            }
            OperationParameters[PARAMETER_INPUT] = useCaseCatalogFilePath;

            if (OperationParameters.ContainsKey(PARAMETER_OUTPUT)) {
                var outputDirPath = OperationParameters[PARAMETER_OUTPUT] as string;
                if (string.IsNullOrWhiteSpace(outputDirPath)) {
                    Console.Error.WriteLine(Resources.Resources.Message_OutputParameterRequiresPath);
                    return false;
                }

                var outputDirFullPath = Path.GetFullPath(outputDirPath);
                if (Directory.Exists(outputDirFullPath) == false) {
                    Console.Error.WriteLine(string.Format(Resources.Resources.Message_Format_NotExistOutputDirectory, outputDirFullPath));
                    return false;
                }
                OperationParameters[PARAMETER_OUTPUT] = outputDirFullPath;
            }

            return DoCanOperate();
        }

        /// <summary>
        /// 操作を実行する
        /// </summary>
        public override sealed void Operate()
        {
            DoOperate(effective ? ReadUseCase() : null);
        }

        //
        // 再定義必須メソッド
        //

        /// <summary>
        /// 操作可能かを返す
        /// </summary>
        /// <returns>操作の可否</returns>
        protected abstract bool DoCanOperate();

        /// <summary>
        /// 操作を実行する
        /// </summary>
        /// <param name="catalog">ユースケース</param>
        protected abstract void DoOperate(UseCaseCatalog catalog);

    }
}
