# Memory Match (神経衰弱)

Unity で作った神経衰弱ゲームです。2人対戦と、プレイヤー対CPUの対戦ができます。
GitHub Spec Kit による SDD(仕様駆動開発)と、TDD(テスト駆動開発)で作りました。実装とテストの実行には unity-cli を使っています。

## 遊び方

1. Unity 6000.6.7f1 で `new-project` を開きます。
2. `Assets/Scenes/MemoryMatch.unity` を開いて Play を押します。
3. タイトル画面でモードを選びます。

| 操作 | 内容 |
|---|---|
| クリック | タイトルのボタン選択、カードをめくる、Restart / Title ボタン |
| ↑ ↓ + Space / Enter | タイトルでモードを選んで決定 |
| 1 / 2 | 2人対戦 / CPU戦で開始 |
| 矢印キー + Space | カードを選んでめくる |
| R | 同じモードでやり直し |
| Esc | タイトルに戻る |

## ルール

- 盤面は 5×4 の20枚(10ペア)です。
- 2枚めくって揃ったら、そのペアを取って、同じ人が続けます。
- 揃わなかったら、約1秒見せてから裏に戻し、手番が交代します(その間は操作できません)。
- 取ったときの点は「取る前の残りペア数 × 10」です。序盤ほど高得点です。
- 全部取ったら、点が多い方の勝ちです。
- CPU は、見たカードを 70% の確率で覚えていて、わかっているペアを狙います。

## 構成

- `new-project/Assets/Scripts/Logic/` : ルールと CPU の思考(Unity に依存しない純 C#)
- `new-project/Assets/Scripts/View/` : 入力・描画・タイマー
- `new-project/Assets/Tests/EditMode/` : EditMode テスト(57件)
- `specs/001-memory-match-versus/` : Spec Kit の成果物
  `interview.md` / `spec.md` / `plan.md` / `tasks.md` / `analysis.md` / `verification.md`(AIプレイの検証記録、`evidence/` に画像とログ)
- `.specify/` : Spec Kit の設定と constitution
- `unity-cli/` : unity-cli(git submodule)

## テストの実行

```
git clone --recurse-submodules https://github.com/kdt24gpr30006/sinkei.git
cd sinkei/new-project
unity-cli test --mode editmode --filter MemoryMatch
```

Unity エディタで `Window > General > Test Runner` から実行してもかまいません。

## 既知の点

- 10ペアの合計点は必ず550点になるため、実際のゲームで引き分けは起きません(判定はテスト済み)。詳細は `specs/001-memory-match-versus/analysis.md` を参照してください。
