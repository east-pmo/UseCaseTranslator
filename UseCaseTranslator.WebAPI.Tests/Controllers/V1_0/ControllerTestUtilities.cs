using System;
using System.Net.Http;
using System.Web.Http;

using East.Tool.UseCaseTranslator.WebAPI;

using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator.WebAPI.Tests.Controllers.V1_0
{
    /// <summary>
    /// コントローラーテストユーティリティ
    /// </summary>
    internal static class ControllerTestUtilities
    {
        //
        // 定数
        //

        /// <summary>
        /// テスト用インメモリHTTPサーバーアクセス時のベースURL
        /// </summary>
        /// <remarks>UseCaseTranslator.WebAPIプロジェクト"Web"プロパティにあわせる</remarks>
        private const string BASE_URL = "http://localhost:20907/";

        //
        // クラスメソッド
        //

        /// <summary>
        /// テスト用インメモリHTTPサーバーを生成する
        /// </summary>
        /// <returns>HttpServerインスタンス</returns>
        internal static HttpServer CreateTestHttpServer()
        {
            Contract.Ensures(Contract.Result<HttpServer>() != null);

            var config = new HttpConfiguration();
            WebApiConfig.Register(config);
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            return new HttpServer(config);
        }

        /// <summary>
        /// HttpRequestMessageインスタンスを生成する
        /// </summary>
        /// <param name="pathFragment">パスフラグメント</param>
        /// <param name="content">送信コンテンツ</param>
        /// <returns>HttpRequestMessageインスタンス</returns>
        internal static HttpRequestMessage CreateRequest(string pathFragment, HttpMethod method, HttpContent content)
        {
            Contract.Requires(string.IsNullOrWhiteSpace(pathFragment) == false);
            Contract.Ensures(Contract.Result<HttpRequestMessage>() != null);

            var requestMessage = new HttpRequestMessage {
                RequestUri = new Uri(string.Format("{0}{1}{2}", BASE_URL, "api/1.0/", pathFragment)),
                Method = method,
            };
            if (content != null) {
                requestMessage.Content = content;
            }
            return requestMessage;
        }
    }
}
