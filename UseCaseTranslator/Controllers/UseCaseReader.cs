using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeTypeResolvers;

using East.Tool.UseCaseTranslator.Models;
using East.Tool.UseCaseTranslator.Utilities;

using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator.Controllers
{
    /// <summary>
    /// カスタムノードタイプリゾルバー
    /// </summary>
    /// <remarks>
    /// キーの重複を禁止するためDictionaryの代わりにValueOverwriteDisallowDictionaryを返す
    /// </remarks>
    public sealed class CustomNodeTypeResolver : INodeTypeResolver
    {
        //
        // クラスフィールド
        //

        /// <summary>
        /// デフォルトのリゾルバー
        /// </summary>
        private static readonly INodeTypeResolver defaultResolver = new DefaultContainersNodeTypeResolver();

        //
        // 再定義メソッド
        //

        /// <summary>
        /// 解決する
        /// </summary>
        /// <param name="nodeEvent">ノードイベント</param>
        /// <param name="currentType">種別</param>
        /// <returns>解決したときtrue</returns>
        public bool Resolve(NodeEvent nodeEvent, ref Type currentType)
        {
            if (currentType == typeof(object) && nodeEvent is MappingStart) {
                currentType = typeof(ValueOverwriteDisallowDictionary<object, object>);
                return true;
            }

            return defaultResolver.Resolve(nodeEvent, ref currentType);
        }
    }

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
            var deserializer = new DeserializerBuilder()
                                .WithNamingConvention(new CamelCaseNamingConvention())
                                .WithNodeTypeResolver(new CustomNodeTypeResolver())
                                .IgnoreUnmatchedProperties()
                                .Build();
            var asYaml = deserializer.Deserialize<ValueOverwriteDisallowDictionary<object, object>>(reader);

            Contract.Assert(asYaml != null);
            if (asYaml.ContainsKey("ユースケースシナリオカタログ")) {
                // シナリオセットを読みこむ
                var scenarioSet = new List<KeyValuePair<string, Dictionary<object, object>>>();
                foreach (var scenarioFilePath in asYaml["シナリオセット"] as IEnumerable<object>) {
                    var path = scenarioFilePath as string;
                    if (path == null) {
                        throw new ArgumentOutOfRangeException(Resources.Resources.Exception_InvalidScenarioSetFileSpecification);
                    }
                    path = Utilities.TryToNormalizeFilePath(path);
                    var exists = File.Exists(path);
                    if (exists == false && string.IsNullOrWhiteSpace(referenceDirectory) == false) {
                        path = Utilities.TryToNormalizeFilePath(Path.Combine(referenceDirectory, path));
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
                            var scenarioSetAsYaml = deserializer.Deserialize<ValueOverwriteDisallowDictionary<object, object>>(scenarioReader);
                            Contract.Assert(scenarioSetAsYaml != null);
            		        scenarioSet.Add(new KeyValuePair<string, Dictionary<object, object>>(Path.GetFileName(path), scenarioSetAsYaml.AsDictionary));
                        }
                    }
                    catch (YamlException e) {
                        throw new ApplicationException(string.Format(Resources.Resources.Exception_Format_InvalidScenarioSetFileFormat, path, e.Message), e);
                    }
                }
                asYaml.Remove("シナリオセット");

                var title = asYaml["ユースケースシナリオカタログ"] as string;
                asYaml.Remove("ユースケースシナリオカタログ");

                var updateHistory = new List<UseCaseUpdateInfo>();
                if (asYaml.ContainsKey("更新履歴")) {
                    updateHistory.AddRange((asYaml["更新履歴"] as ValueOverwriteDisallowDictionary<object, object>).AsDictionary.Select(history => UseCaseUpdateInfo.CreateInstance(history)));
                    asYaml.Remove("更新履歴");
                }

                catalog = new UseCaseCatalog(catalogFileName, lastUpdateTime, title, scenarioSet, updateHistory, asYaml.AsDictionary);
            }
            else if (asYaml.ContainsKey("ユースケースシナリオセット")) {
                // 自身がシナリオセット
                catalog = new UseCaseCatalog(new List<KeyValuePair<string, Dictionary<object, object>>> { new KeyValuePair<string, Dictionary<object, object>>(catalogFileName, asYaml.AsDictionary) });
            }

            return catalog;
        }
    }
}
