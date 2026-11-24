using System;

namespace MemoryMatch
{
    public class MemoryGame
    {
        public const int Columns = 5;
        public const int Rows = 4;
        public const int CardCount = Columns * Rows;
        public const int PairCount = CardCount / 2;

        readonly int[] symbols = new int[CardCount];

        public GameMode Mode { get; private set; }
        public int Seed { get; private set; }

        public void Start(GameMode mode, int seed)
        {
            Mode = mode;
            Seed = seed;
            Shuffle(seed);
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
