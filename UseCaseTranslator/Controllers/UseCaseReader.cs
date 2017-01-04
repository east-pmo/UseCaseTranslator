using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using East.Tool.UseCaseTranslator.Models;

using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator.Controllers
{
    /// <summary>
    /// ユースケースリーダー
    /// </summary>
    public sealed class UseCaseReader
    {
        //
        // フィールド
        //

        /// <summary>
        /// 参照ディレクトリ
        /// </summary>
        private readonly string referenceDirectory;

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UseCaseReader()
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="refDirectory">参照ディレクトリのパス</param>
        public UseCaseReader(string refDirectory)
        {
            Contract.Requires(string.IsNullOrWhiteSpace(refDirectory) == false && Directory.Exists(refDirectory));

            referenceDirectory = Path.GetFullPath(refDirectory);
        }

        /// <summary>
        /// ユースケースカタログを読みこむ
        /// </summary>
        /// <param name="reader">読みこみ元</param>
        /// <param name="catalogFileName">ユースケースカタログファイル名</param>
        /// <param name="catalogLastUpdateTime">ユースケースカタログの最終更新日時</param>
        /// <returns>インスタンス</returns>
        public UseCaseCatalog ReadFrom(TextReader reader, string catalogFileName, DateTime catalogLastUpdateTime)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<UseCaseCatalog>() != null);

            UseCaseCatalog catalog = null;

            var lastUpdateTime = catalogLastUpdateTime;
			var deserializer = new Deserializer(null, new CamelCaseNamingConvention(), false);
			var asYaml = deserializer.Deserialize(reader) as Dictionary<object, object>;

            Contract.Assert(asYaml != null);
            if (asYaml.ContainsKey("ユースケースシナリオカタログ")) {
                // シナリオセットを読みこむ
                var scenarioSet = new List<KeyValuePair<string, Dictionary<object, object>>>();
                foreach (var scenarioFilePath in asYaml["シナリオセット"] as IEnumerable<object>) {
                    var path = scenarioFilePath as string;
                    if (path == null) {
                        throw new ArgumentOutOfRangeException(Resources.Resources.Exception_InvalidScenarioSetFileSpecification);
                    }
                    var exists = File.Exists(path);
                    if (exists == false && string.IsNullOrWhiteSpace(referenceDirectory) == false) {
                        path = Path.Combine(referenceDirectory, path);
                        exists = File.Exists(path);
                    }
                    if (exists == false) {
                        throw new FileNotFoundException(Resources.Resources.Exception_ScenarioSetFileNotFound, scenarioFilePath as string);
                    }

                    var scenarioSetLastUpdateTime = File.GetLastWriteTime(path);
                    if (lastUpdateTime < scenarioSetLastUpdateTime) {
                        lastUpdateTime = scenarioSetLastUpdateTime;
                    }

                    try {
                        using (var scenarioReader = new StreamReader(path)) {
                            var scenarioSetAsYaml = deserializer.Deserialize(scenarioReader);
                            Contract.Assert(scenarioSetAsYaml != null);
            		        scenarioSet.Add(new KeyValuePair<string, Dictionary<object, object>>(Path.GetFileName(path), scenarioSetAsYaml as Dictionary<object, object>));
                        }
                    }
                    catch (YamlException e) {
                        throw new ApplicationException(string.Format(Resources.Resources.Exception_Format_InvalidScenarioSetFileFormat, path), e);
                    }
                }
                asYaml.Remove("シナリオセット");

                var title = asYaml["ユースケースシナリオカタログ"] as string;
                asYaml.Remove("ユースケースシナリオカタログ");

                var updateHistory = new List<UseCaseUpdateInfo>();
                if (asYaml.ContainsKey("更新履歴")) {
                    updateHistory.AddRange((asYaml["更新履歴"] as Dictionary<object, object>).Select(history => UseCaseUpdateInfo.CreateInstance(history)));
                    asYaml.Remove("更新履歴");
                }

                catalog = new UseCaseCatalog(catalogFileName, lastUpdateTime, title, scenarioSet, updateHistory, asYaml);
            }
            else if (asYaml.ContainsKey("ユースケースシナリオセット")) {
                // 自身がシナリオセット
                catalog = new UseCaseCatalog(new List<KeyValuePair<string, Dictionary<object, object>>> { new KeyValuePair<string, Dictionary<object, object>>(catalogFileName, asYaml) });
            }

            return catalog;
        }
    }
}
