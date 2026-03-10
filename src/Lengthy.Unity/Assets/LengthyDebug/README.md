# Lengthy Quick Tester

## 目的
`Lengthy` の `Show(TextAsset)` と `Show(string fileName)` を Unity の Inspector からすぐ試すための簡易ハーネスです。

## 使い方
1. `UIDocument` と `Lengthy` が付いた GameObject をシーンに置きます。
2. 同じ GameObject か別の GameObject に `LengthyQuickTester` を追加します。
3. `Target` に `Lengthy` を割り当てます。
4. `サンプル設定を自動入力` を押すと、同梱サンプルが入ります。
5. Play モードに入り、`Show(TextAsset) を実行` または `Show(fileName) を実行` を押します。

## 同梱サンプル
- TextAsset 用: `Assets/LengthyDebug/LengthySampleText.txt`
- StreamingAssets 用: `Assets/StreamingAssets/Lengthy/sample.txt`

## 補足
- `Lengthy` 側で `PanelSettings` が未設定でも、現在の `UIDocument` やシーン内の `UIDocument` から解決するようにしてあります。
- 右クリックの Context Menu からも実行できますが、基本は Play モードで使ってください。

