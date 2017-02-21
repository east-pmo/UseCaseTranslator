using System;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;

using East.Tool.UseCaseTranslator.WebAPI.Controllers.V1_0;

using System.Diagnostics;
using System.Diagnostics.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace East.Tool.UseCaseTranslator.WebAPI.Tests.Controllers.V1_0
{
    /// <summary>
    /// TranslationControllerテスト
    /// </summary>
    [TestClass]
    public class TranslationControllerTest
    {
        //
        // クラスフィールド
        //

        /// <summary>
        /// ユースケースシナリオファイル名の列挙
        /// </summary>
        private static readonly string[] USECASE_SCENARIOS = {
            "UseCaseTranslatorユースケースカタログ.yaml",
            "UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml",
            "UseCaseTranslatorユースケースシナリオセット - Markdown.yaml",
            "UseCaseTranslatorユースケースシナリオセット - その他.yaml",
        };

        //
        // クラスメソッド
        //

        /// <summary>
        /// ユースケースの要求コンテントを作成する
        /// </summary>
        /// <returns>MultipartContentインスタンス</returns>
        private static MultipartContent CreateUseCaseContent()
        {
            Contract.Ensures(Contract.Result<MultipartContent>() != null);

            var multipartContent = new MultipartContent();
            multipartContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");

            var name = "use-case-catalog";
            var index = 0;
            foreach (var useCaseFile in USECASE_SCENARIOS) {
                using (var reader = new StreamReader(useCaseFile)) {
                    var content = new StringContent(reader.ReadToEnd(), Encoding.UTF8);
                    content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") {
                        Name = name,
                        FileName = useCaseFile,
                    };
                    multipartContent.Add(content);
                }
                name = string.Format("use-case-scenario-set-{0}", ++index);
            }

            return multipartContent;
        }

        /// <summary>
        /// テストスイートテンプレートを含む要求コンテントを作成する
        /// </summary>
        /// <returns>MultipartContentインスタンス</returns>
        private static MultipartContent CreateUseCaseWithTemplateContent()
        {
            Contract.Ensures(Contract.Result<MultipartContent>() != null);

            var multipartContent = CreateUseCaseContent();
            using (var stream = new FileStream("テストスイートテンプレート.xlsx", FileMode.Open, FileAccess.Read)) {
                using (var reader = new BinaryReader(stream)) {
                    var content = new ByteArrayContent(reader.ReadBytes((int)stream.Length));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") {
                        Name = "test-suite-template",
                        FileName = "テストスイートテンプレート.xlsx",
                    };
                    multipartContent.Add(content);
                }
            }
            return multipartContent;
        }

        /// <summary>
        /// テストスイート変換応答を検証する
        /// </summary>
        /// <param name="response">検証対象応答</param>
        private static void AssertTranslateToTestSuiteResponse(HttpResponseMessage response)
        {
            Contract.Requires(response != null);

            if (response.StatusCode != HttpStatusCode.OK) {
                Trace.TraceError(response.StatusCode.ToString());
            }
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            Assert.IsTrue(response.Content.Headers.ContentDisposition.DispositionType == "attachment");
            Assert.IsTrue(response.Content.Headers.ContentDisposition.FileName == "UseCaseTranslator-テストスイート.xlsx");
        }

        /// <summary>
        /// ユースケースシナリオ変換応答を検証する
        /// </summary>
        /// <param name="response">検証対象応答</param>
        private static void AssertTranslateToUseCaseScenarioResponse(HttpResponseMessage response)
        {
            Contract.Requires(response != null);

            if (response.StatusCode != HttpStatusCode.OK) {
                Trace.TraceError(response.StatusCode.ToString());
            }
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "multipart/mixed");
            var contentsProvider = response.Content.ReadAsMultipartAsync().Result;

            Assert.IsTrue(contentsProvider.Contents.Count() == USECASE_SCENARIOS.Count());
            Assert.IsTrue(contentsProvider.Contents.All(content => {
                return content.Headers.ContentType.MediaType == "text/plain"
                    && content.Headers.ContentDisposition.DispositionType == "attachment"
                    && content.Headers.ContentDisposition.FileName.EndsWith(".md", true, CultureInfo.InvariantCulture);
            }));

            var firstContent = contentsProvider.Contents.First();
            Assert.IsTrue(string.Compare(firstContent.Headers.ContentDisposition.FileName, Path.ChangeExtension(USECASE_SCENARIOS.First(), "md"), true) == 0);
            var markdown = firstContent.ReadAsStringAsync().Result;
            Assert.IsFalse(string.IsNullOrWhiteSpace(markdown));
            Trace.TraceInformation(markdown);

            for (var index = 1; index < USECASE_SCENARIOS.Count(); ++index) {
                var content = contentsProvider.Contents.Skip(index).First();
                Assert.IsTrue(string.Compare(content.Headers.ContentDisposition.FileName, Path.ChangeExtension(USECASE_SCENARIOS[index], "md"), true) == 0);
                markdown = content.ReadAsStringAsync().Result;
                Assert.IsFalse(string.IsNullOrWhiteSpace(markdown));
                Trace.TraceInformation(markdown);
            }
        }

        /// <summary>
        /// TranslationControllerインスタンスを生成する
        /// </summary>
        /// <param name="content">送信コンテンツ</param>
        /// <returns>TranslationControllerインスタンス</returns>
        private static TranslationController CreateTranslationController(HttpContent content)
        {
            Contract.Requires(content != null);
            Contract.Ensures(Contract.Result<TranslationController>() != null);

            return new TranslationController {
                ControllerContext = new HttpControllerContext {
                    Request = new HttpRequestMessage {
                        Content = content,
                    }
                }
            };
        }

        /// <summary>
        /// テストスイート変換メソッドアクセスHttpRequestMessageインスタンスを生成する
        /// </summary>
        /// <param name="content">送信コンテンツ</param>
        /// <returns>HttpRequestMessageインスタンス</returns>
        private static HttpRequestMessage CreateTestSuiteRequest(HttpContent content)
        {
            Contract.Requires(content != null);
            Contract.Ensures(Contract.Result<HttpRequestMessage>() != null);

            return ControllerTestUtilities.CreateRequest("translations/test-suite", HttpMethod.Post, content);
        }

        /// <summary>
        /// ユースケースシナリオ変換メソッドアクセスHttpRequestMessageインスタンスを生成する
        /// </summary>
        /// <param name="content">送信コンテンツ</param>
        /// <returns>HttpRequestMessageインスタンス</returns>
        private static HttpRequestMessage CreateUseCaseScenarioRequest(HttpContent content)
        {
            Contract.Requires(content != null);
            Contract.Ensures(Contract.Result<HttpRequestMessage>() != null);

            return ControllerTestUtilities.CreateRequest("translations/use-case", HttpMethod.Post, content);
        }

        //
        // テストメソッド
        //

        /// <summary>
        /// TranslateToTestSuiteメソッドテスト(直接呼びだし、テンプレート指定なし)
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - Markdown.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - その他.yaml")]
        public void TranslateToTestSuiteTest()
        {
            var task = CreateTranslationController(CreateUseCaseContent()).TranslateToTestSuite();
            Assert.IsNotNull(task);
            AssertTranslateToTestSuiteResponse(task.Result);
        }

        /// <summary>
        /// TranslateToTestSuiteメソッドテスト(HttpServer経由呼びだし、テンプレート指定なし)
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - Markdown.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - その他.yaml")]
        public void TranslateToTestSuiteUsingHttpServerTest()
        {
            using (var server = ControllerTestUtilities.CreateTestHttpServer()) {
                using (var client = new HttpClient(server)) {
                    using (var response = client.SendAsync(CreateTestSuiteRequest(CreateUseCaseContent())).Result) {
                        AssertTranslateToTestSuiteResponse(response);
                    }
                }
            }
        }

        /// <summary>
        /// TranslateToTestSuiteメソッドテスト(直接呼びだし、テンプレート指定あり)
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - Markdown.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - その他.yaml")]
        [DeploymentItem(@".\TestData\テストスイートテンプレート.xlsx")]
        public void TranslateToTestSuiteWithTemplateTest()
        {
            var task = CreateTranslationController(CreateUseCaseWithTemplateContent()).TranslateToTestSuite();
            Assert.IsNotNull(task);
            AssertTranslateToTestSuiteResponse(task.Result);
        }

        /// <summary>
        /// TranslateToTestSuiteメソッドテスト(HttpServer経由呼びだし、テンプレート指定あり)
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - Markdown.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - その他.yaml")]
        [DeploymentItem(@".\TestData\テストスイートテンプレート.xlsx")]
        public void TranslateToTestSuiteWithTemplateUsingHttpServerTest()
        {
            using (var server = ControllerTestUtilities.CreateTestHttpServer()) {
                using (var client = new HttpClient(server)) {
                    using (var response = client.SendAsync(CreateTestSuiteRequest(CreateUseCaseWithTemplateContent())).Result) {
                        AssertTranslateToTestSuiteResponse(response);
                    }
                }
            }
        }

        /// <summary>
        /// TranslateToUseCaseScenarioメソッドテスト(直接呼びだし)
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - Markdown.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - その他.yaml")]
        public void TranslateToUseCaseScenarioTest()
        {
            var task = CreateTranslationController(CreateUseCaseContent()).TranslateToUseCaseScenario();
            Assert.IsNotNull(task);
            AssertTranslateToUseCaseScenarioResponse(task.Result);
        }

        /// <summary>
        /// TranslateToUseCaseScenarioメソッドテスト(HttpServer経由呼びだし)
        /// </summary>
        [TestMethod]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースカタログ.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - テストスイートExcelファイル.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - Markdown.yaml")]
        [DeploymentItem(@".\TestData\UseCaseTranslatorユースケースシナリオセット - その他.yaml")]
        public void TranslateToUseCaseScenarioUsingHttpServerTest()
        {
            using (var server = ControllerTestUtilities.CreateTestHttpServer()) {
                using (var client = new HttpClient(server)) {
                    using (var response = client.SendAsync(CreateUseCaseScenarioRequest(CreateUseCaseContent())).Result) {
                        AssertTranslateToUseCaseScenarioResponse(response);
                    }
                }
            }
        }

        /// <summary>
        /// TranslateToUseCaseScenarioメソッドテスト(直接呼びだし、無効要求コンテンツ指定)
        /// </summary>
        [TestMethod]
        public void TranslateToUseCaseScenarioWithInvalidRequestContentTest()
        {
            var task = CreateTranslationController(new ByteArrayContent(new byte[0])).TranslateToUseCaseScenario();
            Assert.IsNotNull(task);
            Assert.IsTrue(task.Result.StatusCode == HttpStatusCode.UnsupportedMediaType);
        }

        /// <summary>
        /// TranslateToUseCaseScenarioメソッドテスト(HttpServer経由呼びだし、無効要求コンテンツ指定)
        /// </summary>
        [TestMethod]
        public void TranslateToUseCaseScenarioWithInvalidRequestContentUsingHttpServerTest()
        {
            using (var server = ControllerTestUtilities.CreateTestHttpServer()) {
                using (var client = new HttpClient(server)) {
                    using (var response = client.SendAsync(CreateUseCaseScenarioRequest(new ByteArrayContent(new byte[0]))).Result) {
                        Assert.IsTrue(response.StatusCode == HttpStatusCode.UnsupportedMediaType);
                    }
                }
            }
        }
    }
}
