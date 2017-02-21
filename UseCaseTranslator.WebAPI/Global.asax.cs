using System.Web.Http;
using System.Web.Mvc;

namespace East.Tool.UseCaseTranslator.WebAPI
{
    /// <summary>
    /// アプリケーション
    /// </summary>
    public class WebApiApplication : System.Web.HttpApplication
    {
        //
        // イベントハンドラ
        //

        /// <summary>
        /// アプリケーション開始
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
