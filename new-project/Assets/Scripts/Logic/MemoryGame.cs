using System;

namespace MemoryMatch
{
    public class MemoryGame
    {
        public const int Columns = 5;
        public const int Rows = 4;
        public const int CardCount = Columns * Rows;
        public const int PairCount = CardCount / 2;
        public const int PointsPerRemainingPair = 10;

        readonly int[] symbols = new int[CardCount];
        readonly bool[] faceUp = new bool[CardCount];
        readonly bool[] taken = new bool[CardCount];
        readonly int[] scores = new int[2];
        readonly int[] pairs = new int[2];
        int firstIndex;

        public GameMode Mode { get; private set; }
        public int Seed { get; private set; }

        public int CurrentPlayer { get; private set; }
        public GameResult Result { get; private set; }
        public int RemainingPairs { get; private set; }
        public int FlippedCount { get; private set; }
        public bool IsPending { get; private set; }
        public bool IsCpuTurn { get { return Mode == GameMode.VsCpu && CurrentPlayer == 1; } }

        public bool IsFaceUp(int index) { return faceUp[index]; }
        public bool IsTaken(int index) { return taken[index]; }
        public int Score(int player) { return scores[player]; }
        public int Pairs(int player) { return pairs[player]; }

        public void Start(GameMode mode, int seed)
        {
            Mode = mode;
            Seed = seed;
            Shuffle(seed);
            Array.Clear(faceUp, 0, CardCount);
            Array.Clear(taken, 0, CardCount);
            Array.Clear(scores, 0, scores.Length);
            Array.Clear(pairs, 0, pairs.Length);
            CurrentPlayer = 0;
            Result = GameResult.InProgress;
            RemainingPairs = PairCount;
            FlippedCount = 0;
            IsPending = false;
        }

        public bool TryFlip(int index)
        {
            if (Mode == GameMode.None || Result != GameResult.InProgress || IsPending) return false;
            if (index < 0 || index >= CardCount) return false;
            if (taken[index] || faceUp[index] || FlippedCount >= 2) return false;
            faceUp[index] = true;
            FlippedCount++;
            if (FlippedCount == 1) { firstIndex = index; return true; }
            if (symbols[firstIndex] == symbols[index]) Take(firstIndex, index);
            else IsPending = true;
            return true;
        }

        // Input from a person: rejected while it is the CPU's turn.
        // Same mode, freshly shuffled layout (derived from the current seed, so it is reproducible).
        public void Restart()
        {
            if (Mode == GameMode.None) return;
            Start(Mode, new Random(Seed ^ 0x5bd1e995).Next());
        }

        public bool TryFlipHuman(int index) { return !IsCpuTurn && TryFlip(index); }

        // What a player can see: the symbol of a face-up card, otherwise -1.
        public int VisibleSymbol(int index) { return faceUp[index] ? symbols[index] : -1; }

        public static GameResult Decide(int score1, int score2)
        {
            if (score1 > score2) return GameResult.Player1Wins;
            if (score2 > score1) return GameResult.Player2Wins;
            return GameResult.Draw;
        }

        public bool ResolvePending()
        {
            if (!IsPending) return false;
            for (int i = 0; i < CardCount; i++) if (!taken[i]) faceUp[i] = false;
            FlippedCount = 0;
            IsPending = false;
            CurrentPlayer = 1 - CurrentPlayer;
            return true;
        }

        void Take(int a, int b)
        {
            taken[a] = true;
            taken[b] = true;
            scores[CurrentPlayer] += RemainingPairs * PointsPerRemainingPair;
            pairs[CurrentPlayer]++;
            RemainingPairs--;
            if (RemainingPairs == 0) Result = Decide(scores[0], scores[1]);
            FlippedCount = 0;
        }

        void Shuffle(int seed)
        {
            for (int i = 0; i < CardCount; i++) symbols[i] = i / 2;
            var rng = new Random(seed);
            for (int i = CardCount - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int t = symbols[i]; symbols[i] = symbols[j]; symbols[j] = t;
            }
        }

        // Symbol (0..PairCount-1) of a card. For tests and debugging only; the view must use VisibleSymbol.
        public int SymbolAt(int index) { return symbols[index]; }
    }
}
