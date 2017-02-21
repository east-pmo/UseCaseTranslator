UseCaseTranslator
=================

UseCaseTranslatorはYAMLで記述したユースケースシナリオカタログをテストスイートExcelファイルやMarkdownフォーマットに変換します。

コマンドラインアプリケーション
------------------------------

コマンドラインアプリケーションはWindowsのコマンドラインプロンプトまたはPowerShellコンソールで動作します。

### 書式

UseCaseTranslator [Command] [Options]

#### コマンド

出力形式を指定します。次のいずれかを指定します:

* TestSuite - Excelファイルのテストスイート
* UseCaseCatalog - Markdownフォーマットのユースケースドキュメント

他に"Help"を指定するとヘルプテキストを標準エラー出力に出力します。

#### オプション

##### [必須] -i / --input

入力ファイルのパスを指定します。

##### [オプション] -o / --output

変換結果の出力先を指定します。指定のないときは入力ファイルの存在するディレクトリに出力します。

##### [オプション] -a / --apply

テンプレートとするファイルのパスを指定します。指定のないときは既定のテンプレートを使用します。

出力形式がTestSuiteのときは単一のテンプレートExcelファイルを指定します。出力形式がUseCaseCatalogのときはユースケースカタログテンプレートファイルのパスとユースケースシナリオセットテンプレートファイルのパスを"|"で結合して指定します。

#### 出力結果のサンプル

　出力結果のサンプルは配布パッケージ"Sample"ディレクトリに存在する、"UseCaseTranslator"ではじまる拡張子".xslx"".md"のファイルを参照してください。

### FAQ

#### 例外「シナリオセットファイル{0}の書式に誤りがあります」が報告される

　シナリオセットファイルの書式がYAMLとしては正しくないため解析できません。代表的なケースとして次が考えられます:

* インデントにタブ(\t)が使われている - YAMLのインデントは空白(U+0020)のみが有効で、タブは利用できません。
* マッピングの値にYAMLのトークン(':'や'-')が含まれる - 値が文字列であることを明示するためにクオーテーションで囲んでください。
* エイリアスに存在しないアンカーを指定している - エイリアスがアンカーとして存在するか確認してください。

Web API
-------

Web APIはASP.NETアプリケーションで、IISで動作します。仕様はSwagger UIを参照してください(URLは"http://{{デプロイホスト}}/{{デプロイパス}}//swagger/ui/index")。

共通の仕様
----------

### 入力ファイルの書式 

入力ファイルは次の種類のファイルの集合です:

* ユースケースカタログファイル(一つ)
* ユースケースシナリオセットファイル(一つ以上)

ユースケースカタログファイルはユースケースシナリオセットファイルの参照を一つ以上含みます。参照ファイルのパスは、ユースケースカタログファイルの存在するディレクトリからの相対パス、または絶対パスで指定します。

入力ファイルの書式は配布パッケージ"Sample"ディレクトリの拡張子".yaml"のファイルを参照してください。

### テンプレートファイルの書式 

* テストスイートExcelファイルのテンプレートは配布パッケージ"Sample"ディレクトリの"テストスイートテンプレート.xlsx"を参照してください。
* Markdownフォーマットのテンプレートは配布パッケージ"Sample"ディレクトリの"ユースケースカタログテンプレート.md"および"ユースケースシナリオセットテンプレート.md"を参照してください。

参考情報
--------

### 参照ライブラリ

ASP.NET Web API 2関連は省略します。

#### オープンソースソフトウェア

##### [YamlDotNet](http://aaubry.net/pages/yamldotnet.html)

[NuGet公開パッケージ](https://www.nuget.org/packages/YamlDotNet/)を利用。

ライセンスは[MIT](http://aaubry.net/pages/license.html)。

##### [ClosedXML](https://closedxml.codeplex.com/)

[NuGet公開パッケージ](https://www.nuget.org/packages/ClosedXML)を利用。

ライセンスは[MIT](https://closedxml.codeplex.com/license)。

##### [RazorEngine](https://github.com/Antaris/RazorEngine)

[NuGet公開パッケージ](https://www.nuget.org/packages/RazorEngine/)を利用。

ライセンスは[Apache License 2.0](https://github.com/Antaris/RazorEngine/blob/master/LICENSE.md)。

##### [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle)

[NuGet公開パッケージ](https://www.nuget.org/packages/Swashbuckle)を利用。

ライセンスは[三条項BSDライセンス](https://github.com/domaindrivendev/Swashbuckle/blob/master/LICENSE)。

#### プロプライエタリ

##### [Open XML SDK 2.5 for Microsoft Office](https://www.microsoft.com/en-us/download/details.aspx?id=30425)

ClosedXMLの依存関係ライブラリとしてインストールされる([DocumentFormat.OpenXml](https://www.nuget.org/packages/DocumentFormat.OpenXml/))。

##### [Microsoft.AspNet.Razor](http://www.asp.net/web-pages)

RazorEngineの依存関係ライブラリとしてインストールされる([Microsoft ASP.NET Razor ](https://www.nuget.org/packages/Microsoft.AspNet.Razor/))。

### 将来の構想

#### テストスイート出力の充実

次の形式での出力を可能とします:

* [TestLink](http://testlink.org/) XML
* TSV
* CSV

#### ID発番

指定に応じてIDを発番して付加するようにします。書式指定や手動・自動切り替えも検討します。
