using System;
using System.Web.Http;
using East.Tool.UseCaseTranslator.WebAPI.Models;

namespace East.Tool.UseCaseTranslator.WebAPI.Controllers.V1_0
{
    /// <summary>
    /// バージョンコントローラー
    /// </summary>
    public class VersionController : ApiController
    {
        //
        // エントリ
        //

        /// <summary>
        /// UseCaseTranslatorコアのバージョンを返す
        /// </summary>
        /// <returns>バージョン情報</returns>
        [Route("api/1.0/version")]
        public VersionModel Get()
        {
            return new VersionModel();
        }
    }
}
