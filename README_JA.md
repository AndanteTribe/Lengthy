# Lengthy
[![Releases](https://img.shields.io/github/release/AndanteTribe/Lengthy.svg)](https://github.com/AndanteTribe/Lengthy/releases)
[![GitHub license](https://img.shields.io/github/license/AndanteTribe/Lengthy.svg)](./LICENSE)
[![openupm](https://img.shields.io/npm/v/jp.andantetribe.lengthy?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/jp.andantetribe.lengthy/)
[![Unity](https://img.shields.io/badge/Unity-6000.0%2B-black?logo=unity)](https://unity.com/releases/unity-6)

[English](README.md) | 日本語

## 概要
**Lengthy** は、Unity で長文を表示するための UI Toolkit ライブラリです。

<img width="1600" height="900" alt="lengthy-license" src="https://github.com/user-attachments/assets/3383467a-7567-40cf-95f8-9504fb0f44be" />

`TextAsset`・`string`・段落ごとの文字列配列を表示し、閉じられるまで `await` で待機できます。既存の Visual Tree に `LengthyView` を直接追加することもできます。

- 動的な行高に対応する `ListView` の仮想化によるスクロール表示。
- 省略可能なタイトルと閉じるボタン。
- デフォルト USS を同梱し、独自のスタイルシートにも対応。
- `ValueTask` による非同期の完了待機。

## 要件
- Unity 6.0（6000.0）以上

## インストール
`Window > Package Manager` から Package Manager を開き、`[+] > Add package from git URL` を選択して以下の URL を入力します。

```text
https://github.com/AndanteTribe/Lengthy.git?path=src/Lengthy.Unity/Packages/jp.andantetribe.lengthy
```

## クイックスタート

### TextAsset を表示する

以下のコンポーネントを GameObject に追加し、Inspector でテキストアセットを指定します。`ShowAsync` を使う場合、シーンへの `UIDocument` の配置や、UXML・PanelSettings アセットの用意は不要です。

```csharp
using System;
using Lengthy;
using UnityEngine;

public class LengthySample : MonoBehaviour
{
    [SerializeField]
    private TextAsset _textAsset;

    private async void Start()
    {
        var cancellationToken = destroyCancellationToken;
        try
        {
            await LengthyView.ShowAsync(_textAsset, "ドキュメント", cancellationToken);
            Debug.Log("ドキュメントを閉じました。");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 表示中にこのコンポーネントが破棄された場合。
        }
    }
}
```

### 文字列・段落配列を表示する

```csharp
await LengthyView.ShowAsync("短いメッセージです。");

await LengthyView.ShowAsync(
    new[] { "最初の段落です。", "次の段落です。", "最後の段落です。" },
    title: "ドキュメント",
    cancellationToken: destroyCancellationToken);
```

`string[]` の各要素が、独立したリスト項目になります。長い文書は段落ごとに分割して渡すことで、仮想化を活用できます。`TextAsset` と `string` は全体をひとつの項目として表示し、改行による項目分割は自動では行いません。

### 既存の UIDocument に追加する

設定済みの `UIDocument` がある場合は、直接ビューを追加できます。

```csharp
var view = new LengthyView(
    new[] { "最初の段落です。", "次の段落です。" },
    title: "ドキュメント",
    alwaysIncludeDefaultStyleSheet: true);

document.rootVisualElement.Add(view);

// コードから取り除く場合：
// view.RemoveFromHierarchy();
```

閉じるボタンを押すと、ビューが親から取り除かれます。既存の `UIDocument` と PanelSettings は呼び出し側で管理します。

## 権利表記作成ツール

`Tools > Lengthy > Rights Notation Creator` から、Lengthy で表示するサードパーティーのライセンス・権利表記をまとめて作成できます。

<!-- ここに権利表記作成ツールの日本語スクリーンショットを挿入してください。 -->

導入済みの UPM・NuGetForUnity パッケージを間接依存も含めて取得し、パッケージマネージャーごとに作者別で表示します。Unity 製の UPM パッケージは除外します。

1. 各パッケージのライセンス文・権利表記を入力します。本文の自動取得は行いません。
2. 出力に含めないパッケージはチェックを外します。入力内容を保持したまま入力欄を折りたたみます。
3. 次のいずれかのボタンで、結果をクリップボードへコピーします。
   - **string[] で生成してコピー**：パッケージのタイトル・本文を交互に並べた配列です。Unity Inspector のシリアライズされた `string[]` プロパティを右クリックし、**Paste** で配列を貼り付けられます。
   - **string で生成してコピー**：すべてのタイトル・本文を空行で結合した文字列です。テキストファイルなどに貼り付けられます。

## API

### `LengthyView.ShowAsync`

すべてのオーバーロードが `ValueTask` を返します。

| メソッド | 説明 |
|----------|------|
| `ShowAsync(TextAsset textAsset, string title = "", CancellationToken cancellationToken = default)` | テキストアセットをひとつの項目として表示します。 |
| `ShowAsync(string value, string title = "", CancellationToken cancellationToken = default)` | 文字列をひとつの項目として表示します。 |
| `ShowAsync(string[] values, string title = "", CancellationToken cancellationToken = default)` | 配列の各要素を別々の段落として表示します。 |

- 閉じるボタンでは正常完了します。キャンセル時は表示を終了し、`OperationCanceledException` を送出します。
- 完了・キャンセル時に、生成した GameObject・PanelSettings・ThemeStyleSheet を破棄します。
- 生成した GameObject はシーン切り替えをまたいで存続します。呼び出し元と寿命を合わせる場合は、`destroyCancellationToken` などを渡してください。
- API の呼び出しとトークンのキャンセルは、Unity のメインスレッドから行ってください。
- 戻り値の `ValueTask` は一度だけ `await` してください。

### `LengthyView` コンストラクター

`TextAsset`・`string`・`string[]` のいずれかを第1引数に受け取り、続けて `string title = ""` と `bool alwaysIncludeDefaultStyleSheet = false` を指定できます。

タイトルが空文字の場合はタイトルラベルのみ非表示になり、閉じるボタンは利用できます。直接生成したビューは、`UIDocument` や描画設定を生成しません。

## スタイルのカスタマイズ

同梱の `DefaultLengthyUss.uss` は、Unity の標準ランタイムテーマを import しています。`ShowAsync` では、実行時生成する PanelSettings と空の ThemeStyleSheet に組み合わせて使用します。

ビューが初めてパネルに接続された際、親に直接設定されたスタイルシートがなければ、デフォルト USS を親に追加します。`alwaysIncludeDefaultStyleSheet: true` を指定すると、既存のスタイルシートの前にデフォルト USS を挿入し、その上から独自のスタイルを重ねられます。

主な USS クラス：

| クラス | 対象 |
|--------|------|
| `.lengthy-root` | ビューのルート |
| `.lengthy-window` | ウィンドウのコンテナー |
| `.lengthy-title` | タイトルラベル |
| `.lengthy-close-button` | 閉じるボタン |
| `.lengthy-list` | 段落リスト |
| `.lengthy-paragraph` | 段落ラベル |

## ライセンス
このライブラリは、[MIT ライセンス](LICENSE)で公開しています。
