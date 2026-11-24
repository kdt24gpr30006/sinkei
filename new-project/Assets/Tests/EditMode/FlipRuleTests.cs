using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // UT-03: FR-005, FR-012 / AS-02, AS-05
    public class FlipRuleTests
    {
        [Test]
        public void FirstFlip_TurnsCardFaceUp()
        {
            var g = TestHelpers.NewGame();
            Assert.IsTrue(g.TryFlip(3));
            Assert.IsTrue(g.IsFaceUp(3));
            Assert.AreEqual(1, g.FlippedCount);
        }

        [Test]
        public void SameCard_CannotBeChosenAsSecondCard()
        {
            var g = TestHelpers.NewGame();
            g.TryFlip(3);
            Assert.IsFalse(g.TryFlip(3));
            Assert.AreEqual(1, g.FlippedCount);
        }

        [Test]
        public void ThirdCard_CannotBeFlipped_AfterTwoMismatchedCards()
        {
            var g = TestHelpers.NewGame();
            int a, b;
            TestHelpers.FindMismatch(g, out a, out b);
            Assert.IsTrue(g.TryFlip(a));
            Assert.IsTrue(g.TryFlip(b));
            int c = 0;
            while (c == a || c == b) c++;
            Assert.IsFalse(g.TryFlip(c));
            Assert.IsFalse(g.IsFaceUp(c));
        }

        [TestCase(-1)]
        [TestCase(MemoryGame.CardCount)]
        public void OutOfRangeIndex_IsRejected(int index)
        {
            Assert.IsFalse(TestHelpers.NewGame().TryFlip(index));
        }

        [Test]
        public void BeforeStart_NothingCanBeFlipped()
        {
            Assert.IsFalse(new MemoryGame().TryFlip(0));
        }
    }
}
