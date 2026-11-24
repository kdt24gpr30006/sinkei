using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // UT-01: FR-003, FR-014 / AS-08
    public class BoardTests
    {
        static int[] Layout(MemoryGame g)
        {
            var a = new int[MemoryGame.CardCount];
            for (int i = 0; i < a.Length; i++) a[i] = g.SymbolAt(i);
            return a;
        }

        [Test]
        public void Board_Has20Cards_AsFiveColumnsByFourRows()
        {
            Assert.AreEqual(20, MemoryGame.CardCount);
            Assert.AreEqual(5, MemoryGame.Columns);
            Assert.AreEqual(4, MemoryGame.Rows);
            Assert.AreEqual(10, MemoryGame.PairCount);
        }

        [Test]
        public void EverySymbol_AppearsExactlyTwice()
        {
            var g = new MemoryGame();
            g.Start(GameMode.TwoPlayer, 123);
            var count = new int[MemoryGame.PairCount];
            foreach (var s in Layout(g))
            {
                Assert.That(s, Is.InRange(0, MemoryGame.PairCount - 1));
                count[s]++;
            }
            foreach (var c in count) Assert.AreEqual(2, c);
        }

        [Test]
        public void SameSeed_GivesSameLayout()
        {
            var a = new MemoryGame(); a.Start(GameMode.TwoPlayer, 42);
            var b = new MemoryGame(); b.Start(GameMode.VsCpu, 42);
            Assert.AreEqual(Layout(a), Layout(b));
        }

        [Test]
        public void DifferentSeed_GivesDifferentLayout()
        {
            var a = new MemoryGame(); a.Start(GameMode.TwoPlayer, 1);
            var b = new MemoryGame(); b.Start(GameMode.TwoPlayer, 2);
            Assert.AreNotEqual(Layout(a), Layout(b));
        }
    }
}
