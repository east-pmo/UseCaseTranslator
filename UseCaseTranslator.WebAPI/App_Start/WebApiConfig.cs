using System.Web.Http;

namespace East.Tool.UseCaseTranslator.WebAPI
{
    /// <summary>
    /// Web API設定
    /// </summary>
    public static class WebApiConfig
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// 登録する
        /// </summary>
        /// <param name="config">HTTP設定</param>
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
