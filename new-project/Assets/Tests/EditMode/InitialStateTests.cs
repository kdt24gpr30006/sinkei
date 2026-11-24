using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // UT-02: FR-001, FR-002, FR-004 / AS-01, AS-12
    public class InitialStateTests
    {
        [Test]
        public void BeforeStart_ModeIsNone()
        {
            Assert.AreEqual(GameMode.None, new MemoryGame().Mode);
        }

        [TestCase(GameMode.TwoPlayer)]
        [TestCase(GameMode.VsCpu)]
        public void AfterStart_BoardIsInInitialState(GameMode mode)
        {
            var g = new MemoryGame();
            g.Start(mode, 7);

            Assert.AreEqual(mode, g.Mode);
            for (int i = 0; i < MemoryGame.CardCount; i++)
            {
                Assert.IsFalse(g.IsFaceUp(i), "card " + i + " face up");
                Assert.IsFalse(g.IsTaken(i), "card " + i + " taken");
            }
            Assert.AreEqual(0, g.Pairs(0));
            Assert.AreEqual(0, g.Pairs(1));
            Assert.AreEqual(0, g.Score(0));
            Assert.AreEqual(0, g.Score(1));
            Assert.AreEqual(0, g.CurrentPlayer);
            Assert.AreEqual(GameResult.InProgress, g.Result);
            Assert.AreEqual(MemoryGame.PairCount, g.RemainingPairs);
            Assert.AreEqual(0, g.FlippedCount);
            Assert.IsFalse(g.IsPending);
            Assert.IsFalse(g.IsCpuTurn);
        }
    }
}
