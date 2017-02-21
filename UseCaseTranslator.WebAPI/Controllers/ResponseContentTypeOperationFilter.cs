using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

// 次のURLの記述を利用させていただいた
// http://stackoverflow.com/questions/34990291/swashbuckle-swagger-how-to-annotate-content-types

namespace East.Tool.UseCaseTranslator.WebAPI.Controllers
{
    /// <summary>
    /// 応答コンテンツタイプ指定属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SwaggerResponseContentTypeAttribute : Attribute
    {
        //
        // プロパティ
        //

        /// <summary>
        /// コンテンツタイプのテキスト表現
        /// </summary>
        public string ResponseType { get; private set; }

        /// <summary>
        /// 排他指定
        /// </summary>
        public bool Exclusive { get; set; }

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="responseType">コンテンツタイプのテキスト表現</param>
        public SwaggerResponseContentTypeAttribute(string responseType)
        {
            ResponseType = responseType;
        }
    }

    /// <summary>
    /// 応答コンテンツタイプ操作フィルター
    /// </summary>
    public class ResponseContentTypeOperationFilter : IOperationFilter
    {
        //
        // メソッド
        //

        /// <summary>
        /// 適用
        /// </summary>
        /// <param name="operation">操作</param>
        /// <param name="schemaRegistry">スキームレジストリ</param>
        /// <param name="apiDescription">API説明</param>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var requestAttributes = apiDescription.GetControllerAndActionAttributes<SwaggerResponseContentTypeAttribute>().FirstOrDefault();
            if (requestAttributes != null) {
                if (requestAttributes.Exclusive) {
                    operation.produces.Clear();
                }

                operation.produces.Add(requestAttributes.ResponseType);
            }
        }
    }
}