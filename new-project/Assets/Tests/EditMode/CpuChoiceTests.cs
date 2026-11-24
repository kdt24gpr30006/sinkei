using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // UT-06b: FR-011 / AS-06, AS-07
    public class CpuChoiceTests
    {
        static void Show(CpuBrain brain, MemoryGame g, int index) { brain.Observe(index, g.SymbolAt(index)); }

        [Test]
        public void FirstCard_KnownPairIsChosen()
        {
            var g = TestHelpers.NewGame(GameMode.VsCpu);
            var brain = new CpuBrain(1, 1.0);
            int a, b;
            TestHelpers.FindPair(g, 4, out a, out b);
            Show(brain, g, a);
            Show(brain, g, b);
            int choice = brain.ChooseNext(g);
            Assert.That(choice == a || choice == b, "choice " + choice);
        }

        [Test]
        public void SecondCard_KnownPartnerIsChosen()
        {
            var g = TestHelpers.NewGame(GameMode.VsCpu);
            var brain = new CpuBrain(1, 1.0);
            int a, b;
            TestHelpers.FindPair(g, 2, out a, out b);
            Show(brain, g, b);          // CPU remembers the partner
            g.TryFlip(a);               // first card is now face up
            Assert.AreEqual(b, brain.ChooseNext(g));
        }

        [Test]
        public void FirstCard_PrefersUnknownCardOverKnownSingle()
        {
            var g = TestHelpers.NewGame(GameMode.VsCpu);
            for (int seed = 0; seed < 30; seed++)
            {
                var brain = new CpuBrain(seed, 1.0);
                Show(brain, g, 6);
                Assert.AreNotEqual(6, brain.ChooseNext(g), "seed " + seed);
            }
        }

        [Test]
        public void SecondCard_WithUnknownPartner_PicksAnotherUnknownCard()
        {
            var g = TestHelpers.NewGame(GameMode.VsCpu);
            int first = 0;
            int known = 1;
            while (g.SymbolAt(known) == g.SymbolAt(first)) known++;
            for (int seed = 0; seed < 30; seed++)
            {
                var gg = TestHelpers.NewGame(GameMode.VsCpu);
                var brain = new CpuBrain(seed, 1.0);
                Show(brain, gg, known);
                gg.TryFlip(first);
                int choice = brain.ChooseNext(gg);
                Assert.AreNotEqual(first, choice, "seed " + seed);
                Assert.AreNotEqual(known, choice, "seed " + seed);
            }
        }

        [Test]
        public void NeverChoosesTakenOrFaceUpCards()
        {
            var g = TestHelpers.NewGame(GameMode.VsCpu);
            var brain = new CpuBrain(3, 1.0);
            int a, b;
            TestHelpers.FindPair(g, 0, out a, out b);
            Show(brain, g, a);
            Show(brain, g, b);
            g.TryFlip(a); g.TryFlip(b);          // taken now; brain still remembers them
            for (int i = 0; i < 30; i++)
            {
                int choice = brain.ChooseNext(g);
                Assert.IsFalse(g.IsTaken(choice));
                Assert.IsFalse(g.IsFaceUp(choice));
            }
        }

        [Test]
        public void VisibleSymbol_IsHiddenForFaceDownCards()
        {
            var g = TestHelpers.NewGame();
            Assert.AreEqual(-1, g.VisibleSymbol(4));
            g.TryFlip(4);
            Assert.AreEqual(g.SymbolAt(4), g.VisibleSymbol(4));
        }

        [Test]
        public void HumanInput_IsRejectedOnCpuTurn_ButCpuCanFlip()
        {
            var g = TestHelpers.NewGame(GameMode.VsCpu);
            int a, b;
            TestHelpers.FindMismatch(g, out a, out b);
            Assert.IsTrue(g.TryFlipHuman(a));
            g.TryFlip(b);
            g.ResolvePending();

            Assert.IsTrue(g.IsCpuTurn);
            Assert.IsFalse(g.TryFlipHuman(a));
            Assert.AreEqual(0, g.FlippedCount);
            Assert.IsTrue(g.TryFlip(a));
        }

        [Test]
        public void TwoPlayerMode_BothPlayersAreHuman()
        {
            var g = TestHelpers.NewGame(GameMode.TwoPlayer);
            int a, b;
            TestHelpers.FindMismatch(g, out a, out b);
            g.TryFlipHuman(a); g.TryFlipHuman(b);
            g.ResolvePending();
            Assert.AreEqual(1, g.CurrentPlayer);
            Assert.IsTrue(g.TryFlipHuman(a));
        }
    }
}
