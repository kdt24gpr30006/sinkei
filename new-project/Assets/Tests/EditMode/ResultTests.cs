using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // UT-07: FR-010 / AS-09, AS-10, AS-11
    public class ResultTests
    {
        static void TakeAll(MemoryGame g)
        {
            for (int p = 0; p < MemoryGame.PairCount; p++)
            {
                int a, b;
                TestHelpers.FindPair(g, p, out a, out b);
                g.TryFlip(a);
                g.TryFlip(b);
            }
        }

        static void PassTurn(MemoryGame g)
        {
            int a, b;
            TestHelpers.FindMismatch(g, out a, out b);
            g.TryFlip(a); g.TryFlip(b);
            g.ResolvePending();
        }

        [Test]
        public void Decide_HigherScoreWins_EqualScoreIsDraw()
        {
            Assert.AreEqual(GameResult.Player1Wins, MemoryGame.Decide(300, 250));
            Assert.AreEqual(GameResult.Player2Wins, MemoryGame.Decide(250, 300));
            Assert.AreEqual(GameResult.Draw, MemoryGame.Decide(275, 275));
        }

        [Test]
        public void GameIsInProgress_UntilLastPairIsTaken()
        {
            var g = TestHelpers.NewGame();
            for (int p = 0; p < MemoryGame.PairCount - 1; p++)
            {
                int a, b;
                TestHelpers.FindPair(g, p, out a, out b);
                g.TryFlip(a); g.TryFlip(b);
                Assert.AreEqual(GameResult.InProgress, g.Result, "after pair " + p);
            }
        }

        [Test]
        public void Player1TakesEverything_Player1Wins_With550()
        {
            var g = TestHelpers.NewGame();
            TakeAll(g);
            Assert.AreEqual(GameResult.Player1Wins, g.Result);
            Assert.AreEqual(550, g.Score(0));
            Assert.AreEqual(0, g.RemainingPairs);
        }

        [Test]
        public void Player2TakesEverything_Player2Wins()
        {
            var g = TestHelpers.NewGame();
            PassTurn(g);
            TakeAll(g);
            Assert.AreEqual(GameResult.Player2Wins, g.Result);
            Assert.AreEqual(550, g.Score(1));
        }

        [Test]
        public void SplitGame_HigherScoreWins()
        {
            var g = TestHelpers.NewGame();
            int a, b;
            TestHelpers.FindPair(g, 0, out a, out b);   // P1 takes the 100-point pair
            g.TryFlip(a); g.TryFlip(b);
            PassTurn(g);                                 // P2 to move
            for (int p = 1; p < MemoryGame.PairCount; p++)
            {
                TestHelpers.FindPair(g, p, out a, out b);
                g.TryFlip(a); g.TryFlip(b);
            }
            Assert.AreEqual(100, g.Score(0));
            Assert.AreEqual(450, g.Score(1));
            Assert.AreEqual(GameResult.Player2Wins, g.Result);
        }

        [Test]
        public void AfterTheGameEnds_NoInputIsAccepted()
        {
            var g = TestHelpers.NewGame();
            TakeAll(g);
            for (int i = 0; i < MemoryGame.CardCount; i++)
            {
                Assert.IsFalse(g.TryFlip(i));
                Assert.IsFalse(g.TryFlipHuman(i));
            }
            Assert.IsFalse(g.ResolvePending());
            Assert.AreEqual(GameResult.Player1Wins, g.Result);
        }
    }
}
