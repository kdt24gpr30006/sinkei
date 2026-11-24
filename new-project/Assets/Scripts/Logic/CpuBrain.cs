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

        public int ChooseNext(MemoryGame game) { throw new NotImplementedException(); }
    }
}
