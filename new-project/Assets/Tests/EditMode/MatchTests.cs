using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // UT-04: FR-007, FR-008 / AS-03, AS-05, AS-11
    public class MatchTests
    {
        [Test]
        public void MatchingPair_IsTakenAndStaysFaceUp()
        {
            var g = TestHelpers.NewGame();
            int a, b;
            TestHelpers.FindPair(g, 0, out a, out b);
            Assert.IsTrue(g.TryFlip(a));
            Assert.IsTrue(g.TryFlip(b));

            Assert.IsTrue(g.IsTaken(a));
            Assert.IsTrue(g.IsTaken(b));
            Assert.IsTrue(g.IsFaceUp(a));
            Assert.IsTrue(g.IsFaceUp(b));
            Assert.AreEqual(1, g.Pairs(0));
            Assert.AreEqual(0, g.Pairs(1));
            Assert.AreEqual(MemoryGame.PairCount - 1, g.RemainingPairs);
            Assert.AreEqual(0, g.FlippedCount);
        }

        [Test]
        public void FirstPair_Scores100()
        {
            var g = TestHelpers.NewGame();
            int a, b;
            TestHelpers.FindPair(g, 0, out a, out b);
            g.TryFlip(a); g.TryFlip(b);
            Assert.AreEqual(100, g.Score(0));
            Assert.AreEqual(0, g.Score(1));
        }

        [Test]
        public void SamePlayer_ContinuesAfterMatch()
        {
            var g = TestHelpers.NewGame();
            int a, b;
            TestHelpers.FindPair(g, 0, out a, out b);
            g.TryFlip(a); g.TryFlip(b);
            Assert.AreEqual(0, g.CurrentPlayer);
            int c, d;
            TestHelpers.FindPair(g, 1, out c, out d);
            Assert.IsTrue(g.TryFlip(c));
            Assert.IsTrue(g.TryFlip(d));
            Assert.AreEqual(2, g.Pairs(0));
            Assert.AreEqual(100 + 90, g.Score(0));
        }

        [Test]
        public void TakenCard_CannotBeChosenAgain()
        {
            var g = TestHelpers.NewGame();
            int a, b;
            TestHelpers.FindPair(g, 0, out a, out b);
            g.TryFlip(a); g.TryFlip(b);
            Assert.IsFalse(g.TryFlip(a));
            Assert.IsFalse(g.TryFlip(b));
            Assert.AreEqual(0, g.FlippedCount);
        }

        [Test]
        public void PointsFollowRemainingPairs_100DownTo10()
        {
            var g = TestHelpers.NewGame();
            int expectedScore = 0;
            for (int p = 0; p < MemoryGame.PairCount; p++)
            {
                int a, b;
                TestHelpers.FindPair(g, p, out a, out b);
                g.TryFlip(a);
                g.TryFlip(b);
                expectedScore += (MemoryGame.PairCount - p) * 10;
                Assert.AreEqual(expectedScore, g.Score(0), "after pair " + p);
            }
            Assert.AreEqual(550, g.Score(0));
        }
    }
}
