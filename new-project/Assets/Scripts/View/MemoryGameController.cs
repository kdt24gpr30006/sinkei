using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MemoryMatch
{
    // Unity layer: input, drawing and timers only. All rules live in MemoryGame / CpuBrain.
    // The public fields mirror the game state so that unity-cli can read them (get_component_values).
    public class MemoryGameController : MonoBehaviour
    {
        [Header("Settings")]
        public int seed = 12345;
        public float mismatchDelay = 1.0f;
        public float cpuDelay = 0.6f;

        [Header("State (read-only, for unity-cli)")]
        public string modeName = "None";
        public int currentPlayer;
        public int score1, score2, pairs1, pairs2;
        public string resultName = "InProgress";
        public bool pending;
        public int flippedCount;
        public int remainingPairs;
        public int cursorIndex;
        public string cardStates = "";     // per card: '.' face down, 'U' face up, 'T' taken
        public string visibleSymbols = ""; // per card: symbol letter if face up, '-' if face down

        const string Letters = "ABCDEFGHIJ";
        static readonly Color[] Palette =
        {
            new Color(0.90f, 0.30f, 0.30f), new Color(0.95f, 0.60f, 0.20f), new Color(0.90f, 0.85f, 0.25f),
            new Color(0.45f, 0.80f, 0.30f), new Color(0.20f, 0.70f, 0.55f), new Color(0.25f, 0.70f, 0.90f),
            new Color(0.40f, 0.45f, 0.95f), new Color(0.65f, 0.40f, 0.90f), new Color(0.90f, 0.40f, 0.75f),
            new Color(0.60f, 0.60f, 0.60f),
        };
        static readonly Color BackColor = new Color(0.15f, 0.27f, 0.50f);

        readonly MemoryGame game = new MemoryGame();
        CpuBrain brain = new CpuBrain(0);
        float pendingTimer, cpuTimer;

        readonly Image[] cardImages = new Image[MemoryGame.CardCount];
        readonly Text[] cardLabels = new Text[MemoryGame.CardCount];
        RectTransform cursorRect;
        Text modeText, turnText, p1Text, p2Text, resultText;

        void Awake()
        {
            for (int i = 0; i < MemoryGame.CardCount; i++)
            {
                var go = GameObject.Find("Card_" + i.ToString("00"));
                cardImages[i] = go.GetComponent<Image>();
                cardLabels[i] = go.transform.Find("Label").GetComponent<Text>();
            }
            cursorRect = GameObject.Find("Cursor").GetComponent<RectTransform>();
            modeText = GameObject.Find("ModeText").GetComponent<Text>();
            turnText = GameObject.Find("TurnText").GetComponent<Text>();
            p1Text = GameObject.Find("P1Text").GetComponent<Text>();
            p2Text = GameObject.Find("P2Text").GetComponent<Text>();
            resultText = GameObject.Find("ResultText").GetComponent<Text>();
            Refresh();
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb != null && !game.IsPending) HandleKeys(kb);

            if (game.IsPending)
            {
                pendingTimer += Time.deltaTime;
                if (pendingTimer >= mismatchDelay) { game.ResolvePending(); pendingTimer = 0f; cpuTimer = 0f; }
            }
            else if (game.Mode == GameMode.VsCpu && game.Result == GameResult.InProgress && game.IsCpuTurn)
            {
                cpuTimer += Time.deltaTime;
                if (cpuTimer >= cpuDelay)
                {
                    cpuTimer = 0f;
                    int choice = brain.ChooseNext(game);
                    if (choice >= 0) Flip(choice, false);
                }
            }
            Refresh();
        }

        void HandleKeys(Keyboard kb)
        {
            if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame) StartGame(GameMode.TwoPlayer);
            else if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame) StartGame(GameMode.VsCpu);
            else if (kb.rKey.wasPressedThisFrame) RestartGame();

            if (game.Mode == GameMode.None || game.Result != GameResult.InProgress || game.IsCpuTurn) return;

            if (kb.leftArrowKey.wasPressedThisFrame) cursorIndex = BoardCursor.Move(cursorIndex, -1, 0);
            if (kb.rightArrowKey.wasPressedThisFrame) cursorIndex = BoardCursor.Move(cursorIndex, 1, 0);
            if (kb.upArrowKey.wasPressedThisFrame) cursorIndex = BoardCursor.Move(cursorIndex, 0, -1);
            if (kb.downArrowKey.wasPressedThisFrame) cursorIndex = BoardCursor.Move(cursorIndex, 0, 1);
            if (kb.spaceKey.wasPressedThisFrame) Flip(cursorIndex, true);
        }

        void StartGame(GameMode mode)
        {
            game.Start(mode, seed);
            brain = new CpuBrain(seed + 1);
            cursorIndex = 0; pendingTimer = 0f; cpuTimer = 0f;
        }

        void RestartGame()
        {
            if (game.Mode == GameMode.None) return;
            game.Restart();
            brain = new CpuBrain(game.Seed + 1);
            cursorIndex = 0; pendingTimer = 0f; cpuTimer = 0f;
        }

        void Flip(int index, bool human)
        {
            bool ok = human ? game.TryFlipHuman(index) : game.TryFlip(index);
            if (ok) brain.Observe(index, game.VisibleSymbol(index)); // the CPU sees every card that is shown
        }

        void Refresh()
        {
            var states = new System.Text.StringBuilder();
            var symbols = new System.Text.StringBuilder();
            for (int i = 0; i < MemoryGame.CardCount; i++)
            {
                int s = game.VisibleSymbol(i);
                if (s < 0)
                {
                    cardImages[i].color = BackColor;
                    cardLabels[i].text = "?";
                    states.Append('.');
                    symbols.Append('-');
                }
                else
                {
                    var c = Palette[s];
                    if (game.IsTaken(i)) c = new Color(c.r * 0.6f, c.g * 0.6f, c.b * 0.6f, 1f);
                    cardImages[i].color = c;
                    cardLabels[i].text = Letters[s].ToString();
                    states.Append(game.IsTaken(i) ? 'T' : 'U');
                    symbols.Append(Letters[s]);
                }
            }
            cardStates = states.ToString();
            visibleSymbols = symbols.ToString();

            modeName = game.Mode.ToString();
            currentPlayer = game.CurrentPlayer;
            score1 = game.Score(0);
            score2 = game.Score(1);
            pairs1 = game.Pairs(0);
            pairs2 = game.Pairs(1);
            resultName = game.Result.ToString();
            pending = game.IsPending;
            flippedCount = game.FlippedCount;
            remainingPairs = game.RemainingPairs;

            cursorRect.gameObject.SetActive(game.Mode != GameMode.None && game.Result == GameResult.InProgress && !game.IsCpuTurn);
            cursorRect.anchoredPosition = ((RectTransform)cardImages[cursorIndex].transform).anchoredPosition;

            string n1 = "Player 1";
            string n2 = game.Mode == GameMode.VsCpu ? "CPU" : "Player 2";
            if (game.Mode == GameMode.None) modeText.text = "Press 1: 2 Players   2: vs CPU";
            else modeText.text = game.Mode == GameMode.TwoPlayer ? "2 Players" : "Player vs CPU";
            if (game.Mode == GameMode.None || game.Result != GameResult.InProgress) turnText.text = "";
            else turnText.text = (game.CurrentPlayer == 0 ? n1 : n2) + " turn";
            p1Text.text = n1 + "\nPairs " + pairs1 + "\nScore " + score1;
            p2Text.text = n2 + "\nPairs " + pairs2 + "\nScore " + score2;
            if (game.Result == GameResult.InProgress) resultText.text = "";
            else if (game.Result == GameResult.Draw) resultText.text = "Draw";
            else resultText.text = (game.Result == GameResult.Player1Wins ? n1 : n2) + " wins!";
        }
    }
}
