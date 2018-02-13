using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using RazorEngine.Text;

using East.Tool.UseCaseTranslator.Models;

using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator.Controllers
{
    /// <summary>
    /// Markdown変換出力
    /// </summary>
    /// <remarks>
    /// 変換にはRazorEngineを使用。
    /// https://github.com/Antaris/RazorEngine
    /// http://antaris.github.io/RazorEngine/
    /// </remarks>
    public sealed class MarkdownConverter : UseCaseTranslatorOperator
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// 変換結果を指定ディレクトリに作成する
        /// </summary>
        /// <param name="catalog">対象ユースケースカタログ</param>
        /// <param name="outputDirectoryPath">出力ディレクトリ</param>
        public static void CreateConvertedMarkdownTo(UseCaseCatalog catalog, string outputDirectoryPath)
        {
            var parameters = new Dictionary<string, object> {
                { PARAMETER_OUTPUT, outputDirectoryPath }
            };
            var converter = new MarkdownConverter(parameters);
            converter.CreateTranslation(catalog);
        }

        //
        // フィールド・プロパティ
        //

        /// <summary>
        /// RazorEngineサービス
        /// </summary>
        private readonly IRazorEngineService razorEngineService;

        /// <summary>
        /// ユースケースカタログテンプレートファイル
        /// </summary>
        private string UseCaseCatalogTemplateFilePath
        {
            get {
                return string.IsNullOrWhiteSpace(TemplateFileParam) == false && TemplateFileParam.Contains("|")
                        ? Path.GetFullPath(TemplateFileParam.Trim('"', ' ').Split('|')[0])
                        : null;
            }
        }

        /// <summary>
        /// ユースケースシナリオセットテンプレートファイル
        /// </summary>
        private string UseCaseScenarioSetTemplateFilePath
        {
            get {
                return string.IsNullOrWhiteSpace(TemplateFileParam) == false && TemplateFileParam.Contains("|")
                        ? Path.GetFullPath(TemplateFileParam.Trim('"', ' ').Split('|')[1])
                        : null;
            }
        }

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MarkdownConverter(IDictionary<string, object> parameters)
            : base(parameters)
        {
            // DisableTempFileLockingとCachingProviderの指定については次のURL参照:
            // https://github.com/Antaris/RazorEngine#temporary-files
            var config = new TemplateServiceConfiguration {
                Language = Language.CSharp,
                EncodedStringFactory = new RawStringFactory(),
                DisableTempFileLocking = true,
                CachingProvider = new DefaultCachingProvider(t => {}), 
            };
            razorEngineService = RazorEngineService.Create(config);
        }

        /// <summary>
        /// RazorEngineのテンプレートを初期化する
        /// </summary>
        /// <param name="useCaseCatalogTemplate">ユースケースカタログテンプレート文字列</param>
        /// <param name="useCaseScenarioSetTemplate">ユースケースシナリオセットテンプレート文字列</param>
        private void InitializeRazorEngineTemplate(string useCaseCatalogTemplate, string useCaseScenarioSetTemplate)
        {
            try {
                razorEngineService.Compile(useCaseCatalogTemplate, "UseCaseCatalogTemplateKey",typeof(UseCaseCatalog));
                razorEngineService.Compile(useCaseScenarioSetTemplate, "UseCaseScenarioSetTemplateKey",typeof(UseCaseScenarioSet));
            }
            catch (TemplateCompilationException e) {
                throw new ApplicationException(Resources.Resources.Exception_RazorEngineTemplateCompilation, e);
            }
        }

        /// <summary>
        /// ユースケースカタログを出力する
        /// </summary>
        /// <param name="catalog">ユースケースカタログ</param>
        private void CreateUseCaseCatalog(UseCaseCatalog catalog)
        {
            Contract.Requires(catalog != null);

            var outputFilePath = Path.Combine(OutputDirectoryPath, Path.ChangeExtension(catalog.FileName, ".md"));
            Console.Error.WriteLine(Resources.Resources.Message_Format_WriteFileTo_UseCaseCatalog, outputFilePath);
            using (var writer = new StreamWriter(outputFilePath)) {
                writer.Write(razorEngineService.Run("UseCaseCatalogTemplateKey",typeof(UseCaseCatalog), catalog));
            }
        }

        /// <summary>
        /// ユースケースシナリオセットを出力する
        /// </summary>
        /// <param name="scenarioSet">シナリオセット</param>
        private void CreateUseCaseScenarioSet(UseCaseScenarioSet scenarioSet)
        {
            Contract.Requires(scenarioSet != null);

            var outputFilePath = Path.Combine(OutputDirectoryPath, Path.ChangeExtension(scenarioSet.FileName, ".md"));
            Console.Error.WriteLine(Resources.Resources.Message_Format_WriteFileTo_UseCaseScenarioSet, outputFilePath);
            using (var writer = new StreamWriter(outputFilePath)) {
                writer.Write(razorEngineService.Run("UseCaseScenarioSetTemplateKey",typeof(UseCaseScenarioSet), scenarioSet));
            }
        }

        /// <summary>
        /// 変換結果を作成する
        /// </summary>
        /// <param name="catalog">ユースケースカタログ</param>
        private void CreateTranslation(UseCaseCatalog catalog)
        {
            var useCaseCatalogTemplate = Resources.Resources.ユースケースカタログテンプレート;
            if (string.IsNullOrWhiteSpace(UseCaseCatalogTemplateFilePath) == false) {
                using (var reader = new StreamReader(UseCaseCatalogTemplateFilePath)) {
                    useCaseCatalogTemplate = reader.ReadToEnd();
                }
            }
            var useCaseScenarioSetTemplate = Resources.Resources.ユースケースシナリオセットテンプレート;
            if (string.IsNullOrWhiteSpace(UseCaseScenarioSetTemplateFilePath) == false) {
                using (var reader = new StreamReader(UseCaseScenarioSetTemplateFilePath)) {
                    useCaseScenarioSetTemplate = reader.ReadToEnd();
                }
            }
            InitializeRazorEngineTemplate(useCaseCatalogTemplate, useCaseScenarioSetTemplate);

            var targetDirPath = OutputDirectoryPath;
            CreateUseCaseCatalog(catalog);
            foreach (var scenarioSet in catalog.ScenarioSets) {
                CreateUseCaseScenarioSet(scenarioSet);
            }
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
                if (TemplateFileParam.Contains("|") == false) {
                    Console.Error.WriteLine(Resources.Resources.Message_InvalidMarkdownTemlateParameter);
                    return false;
                }
                var templates = TemplateFileParam.Trim('"', ' ').Split('|');
                if (templates.Length != 2) {
                    Console.Error.WriteLine(Resources.Resources.Message_InvalidMarkdownTemlateParameter);
                    return false;
                }
                if (templates.Any(template => string.IsNullOrWhiteSpace(template))) {
                    Console.Error.WriteLine(Resources.Resources.Message_InvalidMarkdownTemlateParameter);
                    return false;
                }
                foreach (var template in templates) {
                    var fullPath = Path.GetFullPath(Utilities.TryToNormalizeFilePath(template));
                    if (File.Exists(fullPath) == false) {
                        Console.Error.WriteLine(string.Format(Resources.Resources.Message_Format_NotFoundUseCaseTemplate, fullPath));
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 操作を実行する
        /// </summary>
        /// <param name="catalog">ユースケース</param>
        protected override void DoOperate(UseCaseCatalog catalog)
        {
            CreateTranslation(catalog);
        }
    }
}
