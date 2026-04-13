# ビルド手順の整理 (stack-game)

C:\Users\haryp\stack-game のビルドに関する情報をここに集約します。

## プロジェクト概要
- **種類**: Expo (React Native)
- **パッケージ名**: カラフルSTACK (com.yourname.stackgame)
- **ビルドツール**: EAS Build

## 現在の状況
- **ビルド対象**: iOS
- **package.json**: `build:ios` スクリプトを追加済み

## 実行するコマンド
ターミナルで以下のコマンドを順番に実行してください：

1.  **依存関係のインストール** (まだの場合)
    ```bash
    npm install
    ```

2.  **EAS ログイン** (初回または未ログイン時)
    ```bash
    npx eas login
    ```

3.  **iOS ビルドの実行**
    ```bash
    npm run build:ios
    ```

詳細な状況は `implementation_plan.md` を参照してください。
