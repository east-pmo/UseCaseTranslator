using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace East.Tool.UseCaseTranslator.WebAPI.Tests.Controllers.V1_0
{
    /// <summary>
    /// VersionControllerテスト
    /// </summary>
    [TestClass]
    public class VersionControllerTest
    {
        //
        // テストメソッド
        //

        /// <summary>
        /// GETメソッドテスト
        /// </summary>
        [TestMethod]
        public void GetTest()
        {
            var result = new East.Tool.UseCaseTranslator.WebAPI.Controllers.V1_0.VersionController().Get();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.version == "0.2.1.0");
        }
    }
}
