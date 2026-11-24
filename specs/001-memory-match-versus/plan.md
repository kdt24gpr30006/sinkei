# Implementation Plan: 神経衰弱 対戦ゲーム

**Branch**: `001-memory-match-versus` | **Date**: 2026-11-24 | **Spec**: [spec.md](spec.md)

## Summary

20枚(10ペア)の神経衰弱で、2人対戦とCPU戦を提供する。ルールは Unity に依存しない純粋なC#のロジック層に集め、EditModeテストで TDD する。Unity層(シーン・カーソル・表示・入力)は薄くし、unity-cli で状態を取得して受け入れシナリオを実証する。

## Technical Context

**Language/Version**: C# (Unity 6000.6.7f1)
**Primary Dependencies**: 既存のもののみ(Input System 1.20.0、Test Framework 1.8.0、unity-cli-bridge 0.19.1)。追加なし
**Storage**: なし
**Testing**: Unity Test Framework の EditMode。unity-cli で実行する
**Target Platform**: Unity Editor(Windows)。ビルドはしない
**Project Type**: ゲーム(Unityプロジェクト)
**Constraints**: 外部アセットなし。絵柄は色と記号で区別する

## Constitution Check

| 原則 | 判定 |
|------|------|
| I. SPEC First | SPECの範囲だけを実装する。範囲外(効果音・オンライン・ランキングなど)は作らない |
| II. Test-First | 全Taskは「失敗テスト → 最小実装」の順 |
| III. 分離 | ロジックは `MemoryMatch.Logic`(UnityEngine参照なし)、Unity層は別アセンブリ |
| IV. 実証 | Task T-13 で AS-01〜14 を実プレイで実証し `verification.md` に記録 |
| V. 正直な報告 | 結果は確認済み/未成立/未確認に分けて記録 |

違反はない。

## 設計の決定

### 層の分離

- **ロジック層**(純粋なC#): 配置、めくる規則、判定、得点、手番、勝敗、CPUの思考。
- **Unity層**: カードの表示、カーソル、キー入力、1秒待ちのタイマー、CPUを動かす駆動、HUD表示。

### ロジック層の主な型

| 型 | 責務 |
|----|------|
| `GameMode` | `None` / `TwoPlayer` / `VsCpu` |
| `GameResult` | `InProgress` / `Player1Wins` / `Player2Wins` / `Draw` |
| `MemoryGame` | 盤面と状態。`Start(mode, seed)`、`TryFlip(index)`、`ResolvePending()`、`Restart()` |
| `CpuBrain` | 見たカードの記憶(70%)と、次にめくるカードの選択 |

- `MemoryGame` は外れたとき `IsPending = true` にし、判定は即確定する(得点・手番の更新は `ResolvePending()` で行う)。保留中の `TryFlip` は常に失敗する。時間の1秒待ちはUnity層が管理し、ロジックは時間を持たない(テストで待ち時間0にできる)。
- 手番がCPUのとき、人間用の `TryFlipHuman(index)` は失敗する。CPUは `CpuBrain` が選んだカードを `TryFlip` で操作する。
- 乱数は `System.Random(seed)` を使う。配置用とCPU用で別のインスタンスにする(CPUの記憶判定が配置を変えないため)。
- 点数は「取る直前の残りペア数 × 10」。

### Unity層

- シーン: `Assets/Scenes/MemoryMatch.unity`。
- `Game` オブジェクト(コンポーネント `MemoryGameController`)が状態を公開フィールドで持つ。状態名: `mode`、`currentPlayer`、`score1`、`score2`、`pairs1`、`pairs2`、`result`、`pending`、`cursorIndex`、`faceUp`(カードごと)。CLIの `get_component_values` で読める名前にする。
- カードは `Card_00`〜`Card_19`(5列×4行)。絵柄は色と記号(Unity標準のUIのみ)。
- HUD: モード、手番、両者の取得ペア数と得点、結果。
- 入力: Input System の Keyboard を直接読む(矢印、Space、1、2、R)。

### ファイル構成

```text
new-project/Assets/
├── Scenes/MemoryMatch.unity
├── Scripts/
│   ├── Logic/            # MemoryMatch.Logic.asmdef (noEngineReferences)
│   │   ├── GameMode.cs
│   │   ├── GameResult.cs
│   │   ├── MemoryGame.cs
│   │   └── CpuBrain.cs
│   └── View/             # MemoryMatch.View.asmdef
│       └── MemoryGameController.cs
└── Tests/EditMode/       # MemoryMatch.Tests.asmdef
    ├── BoardTests.cs
    ├── FlipRuleTests.cs
    ├── MatchTests.cs
    ├── CpuBrainTests.cs
    └── ResultAndRestartTests.cs
```

既存の `Assets/Scripts/GameLogic.cs` と `PlayerController.cs` は別ゲームのテンプレート残りなので、T-01 で削除する。

### 追跡表(SPEC → テスト → Task)

| FR | AS | テスト | Task |
|----|----|--------|------|
| FR-003, FR-014 | AS-08 | UT-01 配置(各絵柄2枚、シード再現) | T-02 |
| FR-001, FR-002, FR-004 | AS-01, AS-12 | UT-02 初期状態、モード開始 | T-03 |
| FR-005, FR-012 | AS-02, AS-05 | UT-03 めくる規則と二重防止 | T-04 |
| FR-007, FR-008 | AS-03, AS-11 | UT-04 成立・得点・続行 | T-05 |
| FR-006 | AS-04 | UT-05 外れ・保留・交代 | T-06 |
| FR-011 | AS-06, AS-07 | UT-06 CPUの記憶と選択、人間入力の拒否 | T-07, T-08 |
| FR-010 | AS-09, AS-10 | UT-07 勝敗と終了後の入力停止 | T-09 |
| FR-013 | AS-13, AS-14 | UT-08 再開・モード切替 | T-10 |
| FR-009 | AS-01〜AS-11 | シーンのHierarchy・Game画像 | T-11, T-12 |
| 全FR | AS-01〜AS-14 | AIプレイ実証 | T-13 |
