# QvPen Safe Clear

> [!WARNING]
> このプロジェクトは非推奨です。最新版の QvPen を代わりに使用できます。

QvPen Safe Clear は ureishi 氏開発の QvPen の動作を変更するパッチです。ペンに付属する Clear ボタン (そのペンの軌跡をすべて消去するボタン) をダブルクリックによって発動する設定を追加します。

## 導入方法

1. `/QvPenSafeClear` 以下のファイルをプロジェクトに読み込みます (unity package など)
2. Unity メニューの `Window/QvPen Safe Clear Installer` からインストーラ画面を開きます。
3. 「シーン内を探す」ボタンをクリックしてアクティブなシーン内のすべての QvPen の Clear ボタンを探します。
  - リストを手動で変更することによりインストール先を指定することも可能です。
4. 「すべてインストール」ボタンをクリックしてコンポーネントを置き換えます。
  - リストアイテムの右にある「インストール」ボタンにより個別にインストールすることも可能です。

### 手動で導入する方法

Clear ボタンの `QvPen_InteractButton` コンポーネントを `QvPenInteractionButtonExtended` コンポーネントに置き換え、設定値を書き写します。

## コンポーネントに追加された設定

- Require Double Click: アクションの発動にダブルクリックを必須にします。
- Double Click Duration: ダブルクリックの間隔を設定します。
