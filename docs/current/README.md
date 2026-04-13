# テンプルラン（町版）Unityセットアップガイド (v2: TMPro対応版)

このプロジェクトはUnity 6 (6000.3.10f1) 以上に最適化され、TextMeshProを使用しています。

## 1. プロジェクトの読み込み
1. Unity Hubで「Add project from disk」から `c:\Users\haryp\game\10` を選択して追加し、プロジェクトを開きます。

## 2. TextMeshProの初期設定（初回のみ）
1. プロジェクトを開くと「TMP Importer」というウィンドウが表示される場合があります。
2. **Import TMP Essentials** ボタンを押して、必須アセットをインポートしてください。
   - 表示されない場合は、上部メニューの **Window > TextMeshPro > Import TMP Essential Resources** を実行してください。

## 3. シーンの自動構築
1. 上部メニューの **TownRun > Setup Scene** をクリックします。
2. これにより、UI (TextMeshPro) を含めたすべてのオブジェクトが自動構成されます。

## 4. 実行
- **Playボタン** を押して開始。矢印キーで操作可能です。

## エラーが解消されない場合
- Unity Editorの右下にある赤いエラーアイコンをダブルクリックし、Consoleウィンドウで「Clear」を押してください。
- `Packages/manifest.json` が正しく読み込まれていれば、数秒で解決します。
