# 神経衰弱 対戦ゲーム Constitution

## Core Principles

### I. SPEC First
ゲームのルールは `specs/*/spec.md` だけが決める。SPECにないルールを Plan や実装で追加しない。必要なら Interview と SPEC に戻る。

### II. Test-First (NON-NEGOTIABLE)
実装の前に失敗するEditModeテストを書き、unity-cli で実行して落ちることを確認する。その後に最小実装でGreenにし、全テストを再実行してからRefactorする。

### III. ロジックとUnityの分離
配置・めくる規則・判定・得点・手番・CPUの思考は、Unityに依存しない純粋なC#にする。Unity層は表示・カーソル・入力に絞る。

### IV. 実証で完了とする
「実行した」では完了にしない。テスト名・件数・合否、取得した状態、Game画像を `verification.md` に記録する。状態の直接書き換えや、裏の絵柄データを覗いたプレイで合格にしない。

### V. 正直な報告
確認済み・未成立・未確認を分けて報告する。失敗は成功として扱わない。

## 追加の制約

- 外部アセット、パッケージの追加、Unityの版の更新はしない。
- 1回の作業は1つのTaskだけにする。
- 認証情報・個人設定はリポジトリに入れない。

## Governance

この constitution が他の運用より優先する。変更するときは、理由をコミットメッセージに残す。

**Version**: 1.0.0 | **Ratified**: 2026-11-24 | **Last Amended**: 2026-11-24
