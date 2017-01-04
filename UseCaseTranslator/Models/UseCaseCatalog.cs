using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using System.Diagnostics.Contracts;

namespace East.Tool.UseCaseTranslator.Models
{
    /// <summary>
    /// 更新情報
    /// </summary>
    public sealed class UseCaseUpdateInfo
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// インスタンスを生成する
        /// </summary>
        /// <param name="updateInfo">更新情報</param>
        /// <returns>生成したインスタンス</returns>
        public static UseCaseUpdateInfo CreateInstance(KeyValuePair<object, object> updateInfo)
        {
            var title = updateInfo.Key as string;

            var values = updateInfo.Value as Dictionary<object, object>;
            var date = values["更新日"] as string;

            var summaries = new List<string>();
            if (values["概要"] is string) {
                summaries.Add(values["概要"] as string);
            }
            else if (values["概要"] is IEnumerable<object>) {
                summaries.AddRange((values["概要"] as IEnumerable<object>).Select(detail => detail.ToString()));
            }

            return new UseCaseUpdateInfo { Title = title, Date = date, Summaries = summaries };
        }

        //
        // フィールド・プロパティ
        //

        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// 日付
        /// </summary>
        /// <remarks>多様な表記を許容するため文字列で取りあつかう</remarks>
        public string Date { get; private set; }

        /// <summary>
        /// サマリー
        /// </summary>
        public IEnumerable<string> Summaries { get; private set; }


        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private UseCaseUpdateInfo()
        {
        }
    }

    /// <summary>
    /// ユースケースシナリオアクション
    /// </summary>
    public sealed class UseCaseScenarioAction
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// インスタンスを生成する
        /// </summary>
        /// <param name="action">アクション</param>
        /// <param name="results">結果</param>
        internal static UseCaseScenarioAction CreateInstance(string action, IEnumerable<string> results)
        {
            Contract.Requires(string.IsNullOrWhiteSpace(action) == false);
            Contract.Requires(results != null && results.Any());
            Contract.Requires(results.Any(result => string.IsNullOrWhiteSpace(result)) == false);

            return new UseCaseScenarioAction { Action = action, Results = results.ToList().AsReadOnly() };
        }

        //
        // フィールド・プロパティ
        //

        /// <summary>
        /// アクション
        /// </summary>
        public string Action { get; private set; }

        /// <summary>
        /// 結果
        /// </summary>
        public IEnumerable<string> Results { get; private set; }

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private UseCaseScenarioAction()
        {
        }
    }

    /// <summary>
    /// ユースケースシナリオ
    /// </summary>
    public sealed class UseCaseScenario
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// インスタンスを生成する
        /// </summary>
        /// <param name="scenarioAsYaml">シナリオセットのYAML表現</param>
        /// <returns>生成インスタンス</returns>
        internal static UseCaseScenario CreateInstance(IDictionary<object, object> scenarioAsYaml)
        {
            Contract.Requires(scenarioAsYaml != null && scenarioAsYaml.Any());
            Contract.Ensures(Contract.Result<UseCaseScenario>() != null);

            var title = string.Empty;
            var summary = string.Empty;
            var baseScenario = string.Empty;
            var actions = new List<UseCaseScenarioAction>();
            var metadata = new Dictionary<string, object>();
            IEnumerable<string> preconditions = null;
            foreach (var pair in scenarioAsYaml) {
                var key = pair.Key as string;
                switch (key) {
                    case "タイトル":
                        title = pair.Value as string;
                        break;

                    case "サマリー":
                        summary = pair.Value as string;
                        break;

                    case "ベースシナリオ":
                        baseScenario = pair.Value as string;
                        break;

                    case "事前条件":
                        preconditions = pair.Value is string
                                        ? new List<string> { pair.Value as string }
                                        : (pair.Value as IEnumerable<object>).Select(precondition => precondition as string);
                        break;

                    case "アクション":
                        foreach (var actionsAsYaml in pair.Value as IEnumerable<object>) {
                            var actionsAsDictionary = actionsAsYaml as IDictionary<object, object>;
                            var resultsAsDictionary = actionsAsDictionary["結果"];
                            var actionResults = resultsAsDictionary is string
                                                ? new List<string> { resultsAsDictionary as string }
                                                : (resultsAsDictionary as IEnumerable<object>).Select(result => result as string);
                            actions.Add(UseCaseScenarioAction.CreateInstance(actionsAsDictionary["操作"] as string, actionResults));
                        }
                        break;

                    default:
                        metadata.Add(key, pair.Value);
                        break;
                }
            }
            if (preconditions == null) {
                throw new ApplicationException(string.Format(Resources.Resources.Exception_Format_NotFoundPreconditionKey, title));
            }

            return new UseCaseScenario {
                Title = title,
                Summary = summary,
                BaseScenario = baseScenario,
                Preconditions = preconditions.ToList().AsReadOnly(),
                Actions = actions.ToList().AsReadOnly(),
                metadata = new ReadOnlyDictionary<string, object>(metadata),
            };
        }

        //
        // フィールド・プロパティ
        //

        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// サマリー
        /// </summary>
        public string Summary { get; private set; }

        /// <summary>
        /// ベースシナリオ
        /// </summary>
        public string BaseScenario { get; private set; }

        /// <summary>
        /// 事前条件
        /// </summary>
        public IEnumerable<string> Preconditions { get; private set; }

        /// <summary>
        /// アクションのリスト
        /// </summary>
        public IEnumerable<UseCaseScenarioAction> Actions { get; private set; }

        /// <summary>
        /// その他メタデータ
        /// </summary>
        private IDictionary<string, object> metadata { get; set; }

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private UseCaseScenario()
        {
        }
    }

    /// <summary>
    /// ユースケースシナリオセット
    /// </summary>
    public sealed class UseCaseScenarioSet
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// インスタンスを生成する
        /// </summary>
        /// <param name="fileName">ファイル名(パスなし)</param>
        /// <param name="scenarioSetAsYaml">シナリオセットのYAML表現</param>
        /// <returns>生成インスタンス</returns>
        internal static UseCaseScenarioSet CreateInstance(string fileName, IDictionary<object, object> scenarioSetAsYaml)
        {
            Contract.Requires(scenarioSetAsYaml != null && scenarioSetAsYaml.Any());
            Contract.Ensures(Contract.Result<UseCaseScenarioSet>() != null);

            var title = string.Empty;
            var summary = string.Empty;
            var mainActor = string.Empty;
            var subActors = new List<string>();
            IEnumerable<UseCaseScenario> scenarios = null;
            var updateHistory = new List<UseCaseUpdateInfo>();
            var metadata = new Dictionary<string, object>();
            foreach (var pair in scenarioSetAsYaml) {
                var key = pair.Key as string;
                switch (key) {
                    case "シナリオセット":
                        title = pair.Value as string;
                        break;

                    case "説明":
                        summary = pair.Value as string;
                        break;

                    case "主アクター":
                        mainActor = pair.Value as string;
                        break;

                    case "副アクター":
                        if (pair.Value is string) {
                            subActors.Add(pair.Value as string);
                        }
                        else {
                            subActors.AddRange((pair.Value as IEnumerable<object>).Select(value => value.ToString()));
                        }
                        break;

                    case "シナリオ":
                        scenarios = (pair.Value as IEnumerable<object>).Select(scenarioAsYaml => UseCaseScenario.CreateInstance(scenarioAsYaml as Dictionary<object, object>));
                        break;

                    case "更新履歴":
                        updateHistory.AddRange((pair.Value as Dictionary<object, object>).Select(history => UseCaseUpdateInfo.CreateInstance(history)));
                        break;

                    default:
                        metadata.Add(key, pair.Value);
                        break;
                }
            }

            return new UseCaseScenarioSet {
                FileName = fileName,
                Title = title,
                Summary = summary,
                MainActor = mainActor,
                SubActors = subActors,
                Scenarios = scenarios.ToList().AsReadOnly(),
                UpdateHistory = updateHistory.ToList().AsReadOnly(),
                metadata = new ReadOnlyDictionary<string, object>(metadata),
            };
        }

        //
        // フィールド・プロパティ
        //

        /// <summary>
        /// ファイル名(パスなし)
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// サマリー
        /// </summary>
        public string Summary { get; private set; }

        /// <summary>
        /// 主アクター
        /// </summary>
        public string MainActor { get; private set; }

        /// <summary>
        /// 副アクター
        /// </summary>
        public IEnumerable<string> SubActors { get; private set; }

        /// <summary>
        /// シナリオのリスト
        /// </summary>
        public IEnumerable<UseCaseScenario> Scenarios { get; private set; }

        /// <summary>
        /// 更新履歴
        /// </summary>
        public IEnumerable<UseCaseUpdateInfo> UpdateHistory { get; private set; }

        /// <summary>
        /// その他メタデータ
        /// </summary>
        private IDictionary<string, object> metadata { get; set; }

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private UseCaseScenarioSet()
        {
        }
    }

    /// <summary>
    /// ユースケースカタログ
    /// </summary>
    public sealed class UseCaseCatalog
    {
        //
        // フィールド・プロパティ
        //

        /// <summary>
        /// ファイル名(パスなし)
        /// </summary>
        public string FileName
        {
            get {
                return string.IsNullOrWhiteSpace(fileName) ? Title : fileName;
            }
            private set {
                fileName = value;
            }
        }
        private string fileName;

        /// <summary>
        /// 最終更新日時
        /// </summary>
        /// <remarks>更新履歴情報ではなくファイルのタイムスタンプ情報</remarks>
        public readonly DateTime LastUpdateTime;

        /// <summary>
        /// タイトル
        /// </summary>
        public readonly string Title;

        /// <summary>
        /// シナリオセットの列挙
        /// </summary>
        public readonly IEnumerable<UseCaseScenarioSet> ScenarioSets;

        /// <summary>
        /// 更新履歴
        /// </summary>
        public readonly IEnumerable<UseCaseUpdateInfo> UpdateHistory;

        /// <summary>
        /// その他メタデータ
        /// </summary>
        private readonly IDictionary<object, object> metadata;

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="lastUpdateTime">最終更新日時</param>
        /// <param name="title">タイトル</param>
        /// <param name="scenarioSetsAsYaml">シナリオセットのYAML表現</param>
        /// <param name="updateHistory">更新履歴</param>
        /// <param name="md">メタデータ</param>
        internal UseCaseCatalog(string fileName, DateTime lastUpdateTime, string title, IEnumerable<KeyValuePair<string, Dictionary<object, object>>> scenarioSetsAsYaml, IEnumerable<UseCaseUpdateInfo> updateHistory, IDictionary<object, object> md)
        {
            Contract.Requires(string.IsNullOrWhiteSpace(fileName) == false);
            Contract.Requires(title != null);
            Contract.Requires(scenarioSetsAsYaml != null && scenarioSetsAsYaml.Any() && scenarioSetsAsYaml.Any(scenarioSet => scenarioSet.Value == null) == false);
            Contract.Requires(updateHistory != null);
            Contract.Requires(md != null);

            FileName = fileName;
            LastUpdateTime = lastUpdateTime;
            Title = title;
            ScenarioSets = scenarioSetsAsYaml.Select(scenario => UseCaseScenarioSet.CreateInstance(scenario.Key, scenario.Value));
            UpdateHistory = updateHistory;

            metadata = new ReadOnlyDictionary<object, object>(md);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="scenarioSetsAsYaml">シナリオセットのYAML表現</param>
        internal UseCaseCatalog(IEnumerable<KeyValuePair<string, Dictionary<object, object>>> scenarioSetsAsYaml)
        {
            Contract.Requires(scenarioSetsAsYaml != null && scenarioSetsAsYaml.Any());

            ScenarioSets = scenarioSetsAsYaml.Select(scenario => UseCaseScenarioSet.CreateInstance(scenario.Key, scenario.Value));

            Title = ScenarioSets.First().Title;
        }
    }
}
