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
        public bool onTitle = true;
        public int titleSelection;         // 0 = 2 Players, 1 = vs CPU
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
        static readonly Color ButtonColor = new Color(0.20f, 0.35f, 0.60f);
        static readonly Color ButtonSelectedColor = new Color(0.90f, 0.70f, 0.15f);

        readonly MemoryGame game = new MemoryGame();
        CpuBrain brain = new CpuBrain(0);
        float pendingTimer, cpuTimer;

        readonly Image[] cardImages = new Image[MemoryGame.CardCount];
        readonly Text[] cardLabels = new Text[MemoryGame.CardCount];
        RectTransform cursorRect;
        Text modeText, turnText, p1Text, p2Text, resultText;
        GameObject titlePanel, restartButton, titleButton;
        Image btn2P, btnCpu;

        void Awake()
        {
            for (int i = 0; i < MemoryGame.CardCount; i++)
            {
                var go = GameObject.Find("Card_" + i.ToString("00"));
                cardImages[i] = go.GetComponent<Image>();
                cardLabels[i] = go.transform.Find("Label").GetComponent<Text>();
                int index = i;
                go.GetComponent<Button>().onClick.AddListener(() => OnCardClicked(index));
            }
            cursorRect = GameObject.Find("Cursor").GetComponent<RectTransform>();
            modeText = GameObject.Find("ModeText").GetComponent<Text>();
            turnText = GameObject.Find("TurnText").GetComponent<Text>();
            p1Text = GameObject.Find("P1Text").GetComponent<Text>();
            p2Text = GameObject.Find("P2Text").GetComponent<Text>();
            resultText = GameObject.Find("ResultText").GetComponent<Text>();

            titlePanel = GameObject.Find("TitlePanel");
            restartButton = GameObject.Find("RestartButton");
            titleButton = GameObject.Find("TitleButton");
            btn2P = GameObject.Find("Btn2P").GetComponent<Image>();
            btnCpu = GameObject.Find("BtnCpu").GetComponent<Image>();
            GameObject.Find("Btn2P").GetComponent<Button>().onClick.AddListener(() => OnTitleButton(GameMode.TwoPlayer));
            GameObject.Find("BtnCpu").GetComponent<Button>().onClick.AddListener(() => OnTitleButton(GameMode.VsCpu));
            restartButton.GetComponent<Button>().onClick.AddListener(OnRestartButton);
            titleButton.GetComponent<Button>().onClick.AddListener(OnTitleScreenButton);
            Refresh();
        }

        void Update()
        {
            if (!game.IsPending) HandleKeys();

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

        // True if any keyboard (the real one, or a virtual one) had the key pressed this frame.
        static bool Pressed(System.Func<Keyboard, bool> pick)
        {
            foreach (var device in InputSystem.devices)
            {
                var kb = device as Keyboard;
                if (kb != null && pick(kb)) return true;
            }
            return false;
        }

        void HandleKeys()
        {
            if (game.Mode == GameMode.None)
            {
                // title screen
                if (Pressed(k => k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame)) StartGame(GameMode.TwoPlayer);
                else if (Pressed(k => k.digit2Key.wasPressedThisFrame || k.numpad2Key.wasPressedThisFrame)) StartGame(GameMode.VsCpu);
                else if (Pressed(k => k.upArrowKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame)) titleSelection = 1 - titleSelection;
                else if (Pressed(k => k.spaceKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame))
                    StartGame(titleSelection == 0 ? GameMode.TwoPlayer : GameMode.VsCpu);
                return;
            }

            if (Pressed(k => k.digit1Key.wasPressedThisFrame || k.numpad1Key.wasPressedThisFrame)) StartGame(GameMode.TwoPlayer);
            else if (Pressed(k => k.digit2Key.wasPressedThisFrame || k.numpad2Key.wasPressedThisFrame)) StartGame(GameMode.VsCpu);
            else if (Pressed(k => k.rKey.wasPressedThisFrame)) RestartGame();
            else if (Pressed(k => k.escapeKey.wasPressedThisFrame)) GoToTitle();

            if (game.Mode == GameMode.None || game.Result != GameResult.InProgress || game.IsCpuTurn) return;

            if (Pressed(k => k.leftArrowKey.wasPressedThisFrame)) cursorIndex = BoardCursor.Move(cursorIndex, -1, 0);
            if (Pressed(k => k.rightArrowKey.wasPressedThisFrame)) cursorIndex = BoardCursor.Move(cursorIndex, 1, 0);
            if (Pressed(k => k.upArrowKey.wasPressedThisFrame)) cursorIndex = BoardCursor.Move(cursorIndex, 0, -1);
            if (Pressed(k => k.downArrowKey.wasPressedThisFrame)) cursorIndex = BoardCursor.Move(cursorIndex, 0, 1);
            if (Pressed(k => k.spaceKey.wasPressedThisFrame)) Flip(cursorIndex, true);
        }

        // ---- mouse / button handlers: ignored while a mismatch is being shown (FR-006) ----
        void OnTitleButton(GameMode mode)
        {
            if (game.IsPending || game.Mode != GameMode.None) return;
            StartGame(mode);
        }

        void OnCardClicked(int index)
        {
            if (game.IsPending || game.Mode == GameMode.None) return;
            cursorIndex = index;
            Flip(index, true);   // TryFlipHuman rejects CPU turns, taken/face-up cards and finished games
        }

        void OnRestartButton()
        {
            if (game.IsPending || game.Mode == GameMode.None) return;
            RestartGame();
        }

        void OnTitleScreenButton()
        {
            if (game.IsPending || game.Mode == GameMode.None) return;
            GoToTitle();
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

        void GoToTitle()
        {
            game.ReturnToTitle();
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

            onTitle = game.Mode == GameMode.None;
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

            titlePanel.SetActive(onTitle);
            restartButton.SetActive(!onTitle);
            titleButton.SetActive(!onTitle);
            btn2P.color = titleSelection == 0 ? ButtonSelectedColor : ButtonColor;
            btnCpu.color = titleSelection == 1 ? ButtonSelectedColor : ButtonColor;

            cursorRect.gameObject.SetActive(!onTitle && game.Result == GameResult.InProgress && !game.IsCpuTurn);
            cursorRect.anchoredPosition = ((RectTransform)cardImages[cursorIndex].transform).anchoredPosition;

            string n1 = "Player 1";
            string n2 = game.Mode == GameMode.VsCpu ? "CPU" : "Player 2";
            modeText.text = onTitle ? "" : (game.Mode == GameMode.TwoPlayer ? "2 Players" : "Player vs CPU");
            if (onTitle || game.Result != GameResult.InProgress) turnText.text = "";
            else turnText.text = (game.CurrentPlayer == 0 ? n1 : n2) + " turn";
            p1Text.text = n1 + "\nPairs " + pairs1 + "\nScore " + score1;
            p2Text.text = n2 + "\nPairs " + pairs2 + "\nScore " + score2;
            if (game.Result == GameResult.InProgress) resultText.text = "";
            else if (game.Result == GameResult.Draw) resultText.text = "Draw";
            else resultText.text = (game.Result == GameResult.Player1Wins ? n1 : n2) + " wins!";
        }
    }
}
