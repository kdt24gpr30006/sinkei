using System;
using System.Collections.Generic;

namespace MemoryMatch
{
    // The CPU only learns about a card when it is shown face up (Observe),
    // and each observation is kept with probability rememberChance.
    public class CpuBrain
    {
        public const double DefaultRememberChance = 0.7;

        readonly Random rng;
        readonly double rememberChance;
        readonly Dictionary<int, int> memory = new Dictionary<int, int>();

        public CpuBrain(int seed, double rememberChance = DefaultRememberChance)
        {
            rng = new Random(seed);
            this.rememberChance = rememberChance;
        }

        public void Observe(int index, int symbol)
        {
            if (rng.NextDouble() < rememberChance) memory[index] = symbol;
        }

        public bool Knows(int index) { return memory.ContainsKey(index); }

        // Picks the next card to flip using only what the CPU remembers and what is publicly visible.
        public int ChooseNext(MemoryGame game)
        {
            var available = new List<int>();
            int firstFaceUp = -1;
            for (int i = 0; i < MemoryGame.CardCount; i++)
            {
                if (game.IsTaken(i)) continue;
                if (game.IsFaceUp(i)) { firstFaceUp = i; continue; }
                available.Add(i);
            }
            if (available.Count == 0) return -1;

            if (firstFaceUp < 0)
            {
                // First card of the turn: go for a remembered pair if there is one.
                foreach (int i in available)
                {
                    int sym;
                    if (!memory.TryGetValue(i, out sym)) continue;
                    foreach (int j in available)
                    {
                        int other;
                        if (j != i && memory.TryGetValue(j, out other) && other == sym) return i;
                    }
                }
            }
            else
            {
                // Second card: go for the partner of the visible card if it is remembered.
                int sym = game.VisibleSymbol(firstFaceUp);
                foreach (int i in available)
                {
                    int s;
                    if (memory.TryGetValue(i, out s) && s == sym) return i;
                }
            }

            var unknown = available.FindAll(i => !memory.ContainsKey(i));
            var pool = unknown.Count > 0 ? unknown : available;
            return pool[rng.Next(pool.Count)];
        }
    }
}
