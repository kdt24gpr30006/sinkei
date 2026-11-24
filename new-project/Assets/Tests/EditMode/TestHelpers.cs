namespace MemoryMatch.Tests
{
    static class TestHelpers
    {
        public static MemoryGame NewGame(GameMode mode = GameMode.TwoPlayer, int seed = 1)
        {
            var g = new MemoryGame();
            g.Start(mode, seed);
            return g;
        }

        // Two card indices that hold the same symbol. 'skip' selects the n-th pair (0-based).
        public static void FindPair(MemoryGame g, int skip, out int a, out int b)
        {
            int sym = skip;
            a = b = -1;
            for (int i = 0; i < MemoryGame.CardCount; i++)
            {
                if (g.SymbolAt(i) != sym) continue;
                if (a < 0) a = i; else b = i;
            }
        }

        // Two card indices holding different symbols (neither is in 'avoid').
        public static void FindMismatch(MemoryGame g, out int a, out int b)
        {
            a = 0;
            b = 1;
            while (g.SymbolAt(b) == g.SymbolAt(a)) b++;
        }
    }
}
