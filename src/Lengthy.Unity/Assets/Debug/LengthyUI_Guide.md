# Lengthy-UI構築手順書

`DefaultLengthyUss.uss` に定義されているUSSクラスを使用して以下のように構築してください

結構リッチにできてるはずです

なにかおかしいところとか、変更してほしいところがあれば教えてください

リリース時はこのmdがあるAssets/Debugフォルダごと消してもらって構いません

## 構築ステップ

1. **ルートの作成**
   - `VisualElement` を配置し、クラスに `lengthy-root` を付けます。

2. **ウィンドウの作成**
   - 1の中に `VisualElement` を配置し、クラスに `lengthy-window` を付けます。
   - これが長文表示の実際のウィンドウ部分です

3. **ヘッダーの作成**
   - 2の中に更に `VisualElement` を配置し、タイトルを表示**しない**場合は `lengthy-top-bar`、表示したいときは `lengthy-top-bar--with-title` を付けます。

4. **タイトル文字の配置**
   - タイトルを表示する場合、3の中に `Label` を配置し、クラスに `lengthy-title` を付けます。

5. **閉じるボタンの配置**
   - タイトルを表示する場合、3の中に `Button` を配置し、クラスに `lengthy-close-button` を付けます（右上に自動配置されます）。

6. **リストビューの配置**
   - 2の直下に `ListView` を配置し、クラスに `lengthy-list` を付けます。

7. **リストアイテムの設定**
   - ListViewが生成するUI要素は`Label`を設定し、その`Label`に `lengthy-paragraph`クラス を付けます。

### ツリー構造のイメージ
```text
VisualElement (lengthy-root)
  └── VisualElement (lengthy-window)
        ├── VisualElement (lengthy-top-bar or lengthy-top-bar--with-title)
        │     └── Label (lengthy-title) ※タイトル表示する場合のみ
        │     └── Button (lengthy-close-button) #タイトルを表示する場合のみ
        └── ListView (lengthy-list)
              └── Label (lengthy-paragraph) ※ListViewが生成する要素
```

