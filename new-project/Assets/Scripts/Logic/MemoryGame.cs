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
            return true;
        }

        void Take(int a, int b)
        {
            taken[a] = true;
            taken[b] = true;
            scores[CurrentPlayer] += RemainingPairs * PointsPerRemainingPair;
            pairs[CurrentPlayer]++;
            RemainingPairs--;
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
