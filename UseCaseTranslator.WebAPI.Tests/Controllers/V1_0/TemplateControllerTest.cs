using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Results;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace East.Tool.UseCaseTranslator.WebAPI.Tests.Controllers.V1_0
{
    /// <summary>
    /// TemplateControllerテスト
    /// </summary>
    [TestClass]
    public class TemplateControllerTest
    {
        //
        // テストメソッド
        //

        /// <summary>
        /// GetUseCaseCatalogTemplateメソッドテスト
        /// </summary>
        [TestMethod]
        public void GetUseCaseCatalogTemplateTest()
        {
            var response = new East.Tool.UseCaseTranslator.WebAPI.Controllers.V1_0.TemplateController().GetUseCaseCatalogTemplate();
            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "text/plain");
            Assert.IsTrue(response.Content.Headers.ContentType.CharSet == "utf-8");
            Assert.IsTrue(response.Content.Headers.ContentDisposition.DispositionType == "attachment");
            Assert.IsTrue(response.Content.Headers.ContentDisposition.FileName == "UseCaseCatalogTemplate.md");
        }

        /// <summary>
        /// GetUseCaseCatalogTemplateメソッドテスト(HttpServer経由呼びだし)
        /// </summary>
        [TestMethod]
        public void GetUseCaseCatalogTemplateUsingHttpServerTest()
        {
            using (var server = ControllerTestUtilities.CreateTestHttpServer()) {
                using (var client = new HttpClient(server)) {
                    using (var response = client.SendAsync(ControllerTestUtilities.CreateRequest("templates/use-case/catalog", HttpMethod.Get, null)).Result) {
                        Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
                        Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "text/plain");
                        Assert.IsTrue(response.Content.Headers.ContentType.CharSet == "utf-8");
                        Assert.IsTrue(response.Content.Headers.ContentDisposition.DispositionType == "attachment");
                        Assert.IsTrue(response.Content.Headers.ContentDisposition.FileName == "UseCaseCatalogTemplate.md");
                    }
                }
            }
        }

        /// <summary>
        /// GetUseCaseScenarioSetTemplateメソッドテスト
        /// </summary>
        [TestMethod]
        public void GetUseCaseScenarioSetTemplateTest()
        {
            var response = new East.Tool.UseCaseTranslator.WebAPI.Controllers.V1_0.TemplateController().GetUseCaseScenarioSetTemplate();
            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "text/plain");
            Assert.IsTrue(response.Content.Headers.ContentType.CharSet == "utf-8");
            Assert.IsTrue(response.Content.Headers.ContentDisposition.DispositionType == "attachment");
            Assert.IsTrue(response.Content.Headers.ContentDisposition.FileName == "UseCaseScenarioSetTemplate.md");
        }

        /// <summary>
        /// GetUseCaseScenarioSetTemplateメソッドテスト(HttpServer経由呼びだし)
        /// </summary>
        [TestMethod]
        public void GetUseCaseScenarioSetTemplateUsingHttpServerTest()
        {
            using (var server = ControllerTestUtilities.CreateTestHttpServer()) {
                using (var client = new HttpClient(server)) {
                    using (var response = client.SendAsync(ControllerTestUtilities.CreateRequest("templates/use-case/scenario-set", HttpMethod.Get, null)).Result) {
                        Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
                        Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "text/plain");
                        Assert.IsTrue(response.Content.Headers.ContentType.CharSet == "utf-8");
                        Assert.IsTrue(response.Content.Headers.ContentDisposition.DispositionType == "attachment");
                        Assert.IsTrue(response.Content.Headers.ContentDisposition.FileName == "UseCaseScenarioSetTemplate.md");
                    }
                }
            }
        }

        /// <summary>
        /// GetTestSuiteExcelTemplateメソッドテスト
        /// </summary>
        [TestMethod]
        public void GetTestSuiteExcelTemplateTest()
        {
            var response = new East.Tool.UseCaseTranslator.WebAPI.Controllers.V1_0.TemplateController().GetTestSuiteExcelTemplate();
            Assert.IsNotNull(response);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            Assert.IsTrue(response.Content.Headers.ContentDisposition.DispositionType == "attachment");
            Assert.IsTrue(response.Content.Headers.ContentDisposition.FileName == "TestSuiteTemplate.xlsx");
        }

        /// <summary>
        /// GetTestSuiteExcelTemplateメソッドテスト(HttpServer経由呼びだし)
        /// </summary>
        [TestMethod]
        public void GetTestSuiteExcelTemplateUsingHttpServerTest()
        {
            using (var server = ControllerTestUtilities.CreateTestHttpServer()) {
                using (var client = new HttpClient(server)) {
                    using (var response = client.SendAsync(ControllerTestUtilities.CreateRequest("templates/test-suite/excel", HttpMethod.Get, null)).Result) {
                        Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
                        Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                        Assert.IsTrue(response.Content.Headers.ContentDisposition.DispositionType == "attachment");
                        Assert.IsTrue(response.Content.Headers.ContentDisposition.FileName == "TestSuiteTemplate.xlsx");
                    }
                }
            }
        }
    }
}
