using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // UT-08: FR-013 / AS-13, AS-14
    public class RestartTests
    {
        static int[] Layout(MemoryGame g)
        {
            var a = new int[MemoryGame.CardCount];
            for (int i = 0; i < a.Length; i++) a[i] = g.SymbolAt(i);
            return a;
        }

        static void AssertInitial(MemoryGame g)
        {
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
        }

        [Test]
        public void Restart_MidGame_ResetsEverythingAndKeepsTheMode()
        {
            var g = TestHelpers.NewGame(GameMode.VsCpu);
            int a, b;
            TestHelpers.FindPair(g, 0, out a, out b);
            g.TryFlip(a); g.TryFlip(b);
            TestHelpers.FindMismatch(g, out a, out b);
            g.TryFlip(a); g.TryFlip(b);       // pending mismatch

            g.Restart();

            AssertInitial(g);
            Assert.AreEqual(GameMode.VsCpu, g.Mode);
        }

        [Test]
        public void Restart_AfterTheGameEnded_StartsANewGame()
        {
            var g = TestHelpers.NewGame();
            for (int p = 0; p < MemoryGame.PairCount; p++)
            {
                int a, b;
                TestHelpers.FindPair(g, p, out a, out b);
                g.TryFlip(a); g.TryFlip(b);
            }
            Assert.AreNotEqual(GameResult.InProgress, g.Result);

            g.Restart();

            AssertInitial(g);
            Assert.IsTrue(g.TryFlip(0));
        }

        [Test]
        public void Restart_ReshufflesTheLayout_Reproducibly()
        {
            var g1 = TestHelpers.NewGame(GameMode.TwoPlayer, 5);
            var g2 = TestHelpers.NewGame(GameMode.TwoPlayer, 5);
            var before = Layout(g1);
            g1.Restart();
            g2.Restart();
            Assert.AreNotEqual(before, Layout(g1));
            Assert.AreEqual(Layout(g1), Layout(g2));
        }

        [Test]
        public void Restart_BeforeAnyModeIsChosen_DoesNothing()
        {
            var g = new MemoryGame();
            g.Restart();
            Assert.AreEqual(GameMode.None, g.Mode);
            Assert.IsFalse(g.TryFlip(0));
        }

        [Test]
        public void StartWithAnotherMode_MidGame_BeginsANewGameInThatMode()
        {
            var g = TestHelpers.NewGame(GameMode.TwoPlayer);
            int a, b;
            TestHelpers.FindPair(g, 0, out a, out b);
            g.TryFlip(a); g.TryFlip(b);

            g.Start(GameMode.VsCpu, 9);

            AssertInitial(g);
            Assert.AreEqual(GameMode.VsCpu, g.Mode);
        }
    }
}
