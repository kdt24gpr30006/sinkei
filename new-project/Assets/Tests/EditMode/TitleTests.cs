using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // UT-09: FR-016 / AS-17 (return to the title screen)
    public class TitleTests
    {
        [Test]
        public void ReturnToTitle_ClearsTheGame_AndModeBecomesNone()
        {
            var g = TestHelpers.NewGame(GameMode.VsCpu);
            int a, b;
            TestHelpers.FindPair(g, 0, out a, out b);
            g.TryFlip(a); g.TryFlip(b);          // one pair taken, 100 points
            TestHelpers.FindMismatch(g, out a, out b);
            g.TryFlip(a);                        // one card face up

            g.ReturnToTitle();

            Assert.AreEqual(GameMode.None, g.Mode);
            Assert.AreEqual(GameResult.InProgress, g.Result);
            Assert.AreEqual(0, g.Score(0));
            Assert.AreEqual(0, g.Pairs(0));
            Assert.AreEqual(0, g.FlippedCount);
            Assert.IsFalse(g.IsPending);
            for (int i = 0; i < MemoryGame.CardCount; i++)
            {
                Assert.IsFalse(g.IsFaceUp(i), "card " + i);
                Assert.IsFalse(g.IsTaken(i), "card " + i);
            }
        }

        [Test]
        public void OnTheTitle_NothingCanBeFlipped_AndRestartDoesNothing()
        {
            var g = TestHelpers.NewGame();
            g.ReturnToTitle();

            Assert.IsFalse(g.TryFlip(0));
            Assert.IsFalse(g.TryFlipHuman(0));
            g.Restart();
            Assert.AreEqual(GameMode.None, g.Mode);
        }

        [Test]
        public void ReturnToTitle_AlsoWorksWhileAMismatchIsPending()
        {
            var g = TestHelpers.NewGame();
            int a, b;
            TestHelpers.FindMismatch(g, out a, out b);
            g.TryFlip(a); g.TryFlip(b);
            Assert.IsTrue(g.IsPending);

            g.ReturnToTitle();

            Assert.IsFalse(g.IsPending);
            Assert.AreEqual(GameMode.None, g.Mode);
            Assert.IsFalse(g.ResolvePending());
        }

        [Test]
        public void AfterTheTitle_ANewGameCanBeStartedInAnyMode()
        {
            var g = TestHelpers.NewGame(GameMode.TwoPlayer);
            g.ReturnToTitle();
            g.Start(GameMode.VsCpu, 3);

            Assert.AreEqual(GameMode.VsCpu, g.Mode);
            Assert.IsTrue(g.TryFlip(0));
        }
    }
}
