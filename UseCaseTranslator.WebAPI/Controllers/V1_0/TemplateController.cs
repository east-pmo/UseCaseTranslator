using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;

using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator.WebAPI.Controllers.V1_0
{
    /// <summary>
    /// テンプレートコントローラー
    /// </summary>
    [RoutePrefix("api/1.0/templates")]
    public class TemplateController : ApiController
    {
        //
        // メソッド
        //

        /// <summary>
        /// ファイル添付応答メッセージを返す
        /// </summary>
        /// <param name="contentAsBytes">添付内容のバイト表現</param>
        /// <param name="mimeHeaderLiteral">MIMEヘッダーの表記</param>
        /// <param name="attachmentFileName">添付ファイル名</param>
        /// <returns>HttpResponseMessageインスタンス</returns>
        /// <remarks>
        /// HttpResponseMessageを利用するのはContentTypeとContentDispositionを手軽にあつかうため。
        /// IHttpActionResultをストレートに利用すると上書きされてしまい、設定するにはクラス定義等が必要になる。
        /// </remarks>
        private HttpResponseMessage CreateAttachmentResponseMessage(byte[] contentAsBytes, string mimeHeaderLiteral, string attachmentFileName)
        {
            Contract.Requires(contentAsBytes != null);
            Contract.Requires(string.IsNullOrWhiteSpace(mimeHeaderLiteral) == false);
            Contract.Requires(string.IsNullOrWhiteSpace(attachmentFileName) == false);
            Contract.Ensures(Contract.Result<HttpResponseMessage>() != null);
                
            var content = new ByteArrayContent(contentAsBytes);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeHeaderLiteral);
            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {
                FileName = attachmentFileName,
            };
            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = content,
            };
        }

        //
        // エントリ
        //

        /// <summary>
        /// ユースケースカタログテンプレートを返す
        /// </summary>
        /// <returns>IHttpActionResultインターフェース実装クラスインスタンス</returns>
        [Route("use-case/catalog")]
        [HttpGet]
        [ResponseType(typeof(string))]
        [SwaggerResponseContentType(responseType:"text/plain", Exclusive=true)]
        public HttpResponseMessage GetUseCaseCatalogTemplate()
        {
            return CreateAttachmentResponseMessage(Encoding.UTF8.GetBytes(Resources.Resources.ユースケースカタログテンプレート), "text/plain; charset=utf-8", "UseCaseCatalogTemplate.md");
        }

        /// <summary>
        /// ユースケースシナリオセットテンプレートを返す
        /// </summary>
        /// <returns>IHttpActionResultインターフェース実装クラスインスタンス</returns>
        [Route("use-case/scenario-set")]
        [HttpGet]
        [ResponseType(typeof(string))]
        [SwaggerResponseContentType(responseType:"text/plain", Exclusive=true)]
        public HttpResponseMessage GetUseCaseScenarioSetTemplate()
        {
            return CreateAttachmentResponseMessage(Encoding.UTF8.GetBytes(Resources.Resources.ユースケースシナリオセットテンプレート), "text/plain; charset=utf-8", "UseCaseScenarioSetTemplate.md");
        }

        /// <summary>
        /// テストスイートExcelファイルテンプレートを返す
        /// </summary>
        /// <returns>IHttpActionResultインターフェース実装クラスインスタンス</returns>
        [Route("test-suite/excel")]
        [HttpGet]
        [SwaggerResponseContentType(responseType:"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Exclusive=true)]
        public HttpResponseMessage GetTestSuiteExcelTemplate()
        {
            return CreateAttachmentResponseMessage(Resources.Resources.テストスイートテンプレート, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TestSuiteTemplate.xlsx");
        }
    }
}
