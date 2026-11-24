using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // UT-05: FR-006, FR-007 / AS-04
    public class MismatchTests
    {
        static void Mismatch(MemoryGame g, out int a, out int b)
        {
            TestHelpers.FindMismatch(g, out a, out b);
            g.TryFlip(a);
            g.TryFlip(b);
        }

        [Test]
        public void Mismatch_BecomesPending_CardsStayFaceUp_ScoreUnchanged()
        {
            var g = TestHelpers.NewGame();
            int a, b;
            Mismatch(g, out a, out b);

            Assert.IsTrue(g.IsPending);
            Assert.IsTrue(g.IsFaceUp(a));
            Assert.IsTrue(g.IsFaceUp(b));
            Assert.AreEqual(0, g.Score(0));
            Assert.AreEqual(0, g.Pairs(0));
            Assert.AreEqual(0, g.CurrentPlayer);
        }

        [Test]
        public void WhilePending_NoCardCanBeFlipped()
        {
            var g = TestHelpers.NewGame();
            int a, b;
            Mismatch(g, out a, out b);
            for (int i = 0; i < MemoryGame.CardCount; i++)
                Assert.IsFalse(g.TryFlip(i), "card " + i);
        }

        [Test]
        public void ResolvePending_FlipsBackAndPassesTheTurn()
        {
            var g = TestHelpers.NewGame();
            int a, b;
            Mismatch(g, out a, out b);

            Assert.IsTrue(g.ResolvePending());

            Assert.IsFalse(g.IsPending);
            Assert.IsFalse(g.IsFaceUp(a));
            Assert.IsFalse(g.IsFaceUp(b));
            Assert.AreEqual(0, g.FlippedCount);
            Assert.AreEqual(1, g.CurrentPlayer);
            Assert.AreEqual(0, g.Score(0));
            Assert.AreEqual(0, g.Score(1));
            Assert.IsTrue(g.TryFlip(a));
        }

        [Test]
        public void SecondPlayerMismatch_PassesTurnBackToFirstPlayer()
        {
            var g = TestHelpers.NewGame();
            int a, b;
            Mismatch(g, out a, out b);
            g.ResolvePending();
            Assert.AreEqual(1, g.CurrentPlayer);
            Mismatch(g, out a, out b);
            g.ResolvePending();
            Assert.AreEqual(0, g.CurrentPlayer);
        }

        [Test]
        public void ResolvePending_WithoutPending_DoesNothing()
        {
            var g = TestHelpers.NewGame();
            Assert.IsFalse(g.ResolvePending());
            Assert.AreEqual(0, g.CurrentPlayer);
        }

        [Test]
        public void SecondPlayer_ScoresOnMatch_AndKeepsTheTurn()
        {
            var g = TestHelpers.NewGame();
            int a, b;
            Mismatch(g, out a, out b);
            g.ResolvePending();
            TestHelpers.FindPair(g, 0, out a, out b);
            g.TryFlip(a); g.TryFlip(b);
            Assert.AreEqual(1, g.CurrentPlayer);
            Assert.AreEqual(100, g.Score(1));
            Assert.AreEqual(0, g.Score(0));
        }
    }
}
