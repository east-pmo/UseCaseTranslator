using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http.Headers;

using East.Tool.UseCaseTranslator.Controllers;
using East.Tool.UseCaseTranslator.Models;

using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator.WebAPI.Controllers.V1_0
{
    /// <summary>
    /// 変換コントローラー
    /// </summary>
    /// <remarks>
    /// 応答にHttpResponseMessageを利用するのはContentTypeとContentDispositionを手軽にあつかうため。
    /// IHttpActionResultをストレートに利用すると上書きされてしまい、設定するにはクラス定義等が必要になる。
    /// </remarks>
    [RoutePrefix("api/1.0/translations")]
    public class TranslationController : ApiController
    {
        //
        // 内部型定義
        //

        private class AttachmentsProvider
        {
            //
            // フィールド
            //

            /// <summary>
            /// ユースケースカタログファイル名の列挙
            /// </summary>
            /// <remarks>エラー判定のために列挙</remarks>
            private readonly IEnumerable<string> catalogFileNames;

            /// <summary>
            /// ユースケースシナリオセットファイル名の列挙
            /// </summary>
            private readonly IEnumerable<string> scenarioFileNames;

            /// <summary>
            /// テストスイートExcelファイルテンプレートファイル名の列挙
            /// </summary>
            /// <remarks>エラー判定のために列挙</remarks>
            private readonly IEnumerable<string> templateFileNames;

            //
            // プロパティ
            //

            /// <summary>
            /// ユースケースカタログファイル名
            /// </summary>
            public string CatalogFileName
            {
                get {
                    return catalogFileNames.Single();
                }
            }

            /// <summary>
            /// ユースケースシナリオセットファイル名の列挙
            /// </summary>
            public IEnumerable<string> ScenarioFileNames
            {
                get {
                    return scenarioFileNames;
                }
            }

            /// <summary>
            /// テストスイートExcelファイルテンプレートファイル名
            /// </summary>
            public string TemplateFileName
            {
                get {
                    return templateFileNames.SingleOrDefault();
                }
            }

            //
            // メソッド
            //

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="provider">MultipartFormDataStreamProviderインスタンス</param>
            internal AttachmentsProvider(MultipartFormDataStreamProvider provider)
            {
                Contract.Requires(provider != null);

                catalogFileNames = provider.FileData.Where(fileData => fileData.Headers.ContentDisposition.Name == "use-case-catalog").Select(fileData => fileData.LocalFileName);
                scenarioFileNames = provider.FileData.Where(fileData => fileData.Headers.ContentDisposition.Name.StartsWith("use-case-scenario-set-")).Select(fileData => fileData.LocalFileName);
                templateFileNames = provider.FileData.Where(fileData => fileData.Headers.ContentDisposition.Name == "test-suite-template").Select(fileData => fileData.LocalFileName);
            }

            /// <summary>
            /// 検証する
            /// </summary>
            /// <returns>検証結果のメッセージの列挙(空集合の時成功)</returns>
            internal IEnumerable<string> Validate()
            {
                var resultPhrases = new List<string>();
                if (catalogFileNames.Count() != 1) {
                    resultPhrases.Add(catalogFileNames.Any() ? "Too many use case catalog file." : "No use case catalog file.");
                }
                if (scenarioFileNames.Any() == false) {
                    resultPhrases.Add("No use case scenario set file.");
                }
                if (1 < templateFileNames.Count()) {
                    resultPhrases.Add("Too many test suite template file.");
                }

                return resultPhrases;
            }
        }

        //
        // クラスメソッド
        //

        /// <summary>
        /// テストスイートの応答コンテントを作成する
        /// </summary>
        /// <param name="testSuiteExcelFilePath">テストスイートExcelファイルパス</param>
        /// <returns>応答コンテントインスタンス</returns>
        [Pure]
        private static ByteArrayContent CreateTestSuiteResponseContent(string testSuiteExcelFilePath)
        {
            Contract.Requires(string.IsNullOrWhiteSpace(testSuiteExcelFilePath) == false && File.Exists(testSuiteExcelFilePath));
            Contract.Ensures(Contract.Result<ByteArrayContent>() != null);

            ByteArrayContent responseContent = null;
            using (var outputStream = new FileStream(testSuiteExcelFilePath, FileMode.Open, FileAccess.Read)) {
                using (var reader = new BinaryReader(outputStream)) {
                    responseContent = new ByteArrayContent(reader.ReadBytes((int)outputStream.Length));
                    responseContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    responseContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {
                        FileName = Path.GetFileName(testSuiteExcelFilePath),
                    };
                }
            }
            return responseContent;
        }

        /// <summary>
        /// ユースケースシナリオの応答コンテントを作成する
        /// </summary>
        /// <param name="useCaseScenarioDirPath">ユースケースシナリオファイルの存在するディレクトリのパス</param>
        /// <param name="catalogFileName">ユースケースカタログのファイル名</param>
        /// <param name="catalog">ユースケースカタログ</param>
        /// <returns>応答コンテントインスタンス</returns>
        private static MultipartContent CreateUseCaseScenarioResponseContent(string useCaseScenarioDirPath, string catalogFileName, UseCaseCatalog catalog)
        {
            Contract.Requires(string.IsNullOrWhiteSpace(useCaseScenarioDirPath) == false && Directory.Exists(useCaseScenarioDirPath));
            Contract.Requires(string.IsNullOrWhiteSpace(catalogFileName) == false);
            Contract.Requires(catalog != null);
            Contract.Ensures(Contract.Result<MultipartContent>() != null);

            var fileNames = new[] {
                Path.GetFileName(catalog.FileName),
            }.Concat(catalog.ScenarioSets.Select(scenarioSet => scenarioSet.FileName));

            var responseContent = new MultipartContent("mixed", "BOUNDARY");
            foreach (var file in fileNames) {
                var markdownFile = Path.ChangeExtension(file, ".md");
                ByteArrayContent markdownContent;
                using (var outputStream = new FileStream(Path.Combine(useCaseScenarioDirPath, markdownFile), FileMode.Open, FileAccess.Read)) {
                    using (var reader = new StreamReader(outputStream)) {
                        markdownContent = new ByteArrayContent(Encoding.UTF8.GetBytes(reader.ReadToEnd()));
                        markdownContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain") {
                            CharSet = "UTF-8",
                        };
                        markdownContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {
                            FileName = (file == Path.GetFileName(catalog.FileName) ? Path.ChangeExtension(catalogFileName, ".md") : markdownFile),
                        };
                    }
                }
                responseContent.Add(markdownContent);
            }
            return responseContent;
        }

        /// <summary>
        /// 他プロセスとの競合を避けるため作業用一時ディレクトリを作成する
        /// </summary>
        /// <returns>作成した作業ディレクトリのパス</returns>
        private static string CreateWorkingDirectory()
        {
            Contract.Ensures(Contract.Result<string>() != null && Directory.Exists(Contract.Result<string>()));

            var tempPath = Path.GetTempPath();
            var workingPath = Path.Combine(tempPath, Path.GetRandomFileName());
            while (Directory.Exists(workingPath)) {
                workingPath = Path.Combine(tempPath, Path.GetRandomFileName());
            }
            Directory.CreateDirectory(workingPath);
            return workingPath;
        }

        //
        // エントリ
        //

        /// <summary>
        /// 変換したテストスイートExcelファイルを返す
        /// </summary>
        /// <returns>HttpResponseMessageインスタンス</returns>
        [Route("test-suite")]
        [HttpPost]
        [SwaggerResponseContentType(responseType:"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Exclusive=true)]
        public async Task<HttpResponseMessage> TranslateToTestSuite()
        {
            if (Request.Content.IsMimeMultipartContent() == false) {
                return new HttpResponseMessage(HttpStatusCode.UnsupportedMediaType);
            }

            string outputFilePath = null;

            var workingPath = string.Empty;
            try {
                workingPath = CreateWorkingDirectory();

                var provider = new MultipartFormDataStreamProvider(workingPath);
                await Request.Content.ReadAsMultipartAsync(provider);

                var attachmentsProvider = new AttachmentsProvider(provider);
                var phrases = attachmentsProvider.Validate();
                if (phrases.Any()) {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest) {
                        ReasonPhrase = string.Join(Environment.NewLine, phrases)
                    };
                }

                using (var reader = new StreamReader(attachmentsProvider.CatalogFileName)) {
                    outputFilePath = ExcelTestSuiteBuilder.CreateExcelTestSuiteTo(new UseCaseReader(workingPath).ReadFrom(reader, attachmentsProvider.CatalogFileName, DateTime.Now), workingPath, attachmentsProvider.TemplateFileName);
                }

                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = CreateTestSuiteResponseContent(outputFilePath),
                };
            }
            catch (ApplicationException e) {
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = e.Message };
            }
            finally {
                try {
                    if (File.Exists(outputFilePath)) {
                        File.Delete(outputFilePath);
                    }
                    if (Directory.Exists(workingPath)) {
                        Directory.Delete(workingPath);
                    }
                }
                catch {
                    // Do nothing.
                }
            }
        }

        /// <summary>
        /// 変換したユースケースシナリオを返す
        /// </summary>
        /// <returns>HttpResponseMessageインスタンス</returns>
        [Route("use-case")]
        [HttpPost]
        public async Task<HttpResponseMessage> TranslateToUseCaseScenario()
        {
            if (Request.Content.IsMimeMultipartContent() == false) {
                return new HttpResponseMessage(HttpStatusCode.UnsupportedMediaType);
            }

            var workingPath = string.Empty;
            try {
                workingPath = CreateWorkingDirectory();

                var provider = new MultipartFormDataStreamProvider(workingPath);
                await Request.Content.ReadAsMultipartAsync(provider);

                var attachmentsProvider = new AttachmentsProvider(provider);
                var phrases = attachmentsProvider.Validate();
                if (phrases.Any()) {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest) {
                        ReasonPhrase = string.Join(Environment.NewLine, phrases)
                    };
                }

                UseCaseCatalog catalog;
                using (var reader = new StreamReader(attachmentsProvider.CatalogFileName)) {
                    catalog = new UseCaseReader(workingPath).ReadFrom(reader, attachmentsProvider.CatalogFileName, DateTime.Now);
                    MarkdownConverter.CreateConvertedMarkdownTo(catalog, workingPath);
                }

                return new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = CreateUseCaseScenarioResponseContent(workingPath, provider.Contents[0].Headers.ContentDisposition.FileName, catalog),
                };
            }
            catch (ApplicationException e) {
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { ReasonPhrase = e.Message };
            }
            finally {
                try {
                    if (Directory.Exists(workingPath)) {
                        Directory.Delete(workingPath);
                    }
                }
                catch {
                    // Do nothing.
                }
            }
        }
    }
}
