using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // UT-06a: FR-011 / AS-07, AS-08
    public class CpuMemoryTests
    {
        [Test]
        public void DefaultRememberChance_Is70Percent()
        {
            Assert.AreEqual(0.7, CpuBrain.DefaultRememberChance, 1e-9);
        }

        [Test]
        public void ObservedCard_IsRememberedAboutSeventyPercentOfTheTime()
        {
            const int trials = 2000;
            int remembered = 0;
            for (int seed = 0; seed < trials; seed++)
            {
                var brain = new CpuBrain(seed);
                brain.Observe(0, 3);
                if (brain.Knows(0)) remembered++;
            }
            double ratio = remembered / (double)trials;
            Assert.That(ratio, Is.InRange(0.66, 0.74), "ratio " + ratio);
        }

        [Test]
        public void ChanceOne_AlwaysRemembers_ChanceZero_NeverRemembers()
        {
            for (int seed = 0; seed < 50; seed++)
            {
                var always = new CpuBrain(seed, 1.0);
                always.Observe(5, 2);
                Assert.IsTrue(always.Knows(5));

                var never = new CpuBrain(seed, 0.0);
                never.Observe(5, 2);
                Assert.IsFalse(never.Knows(5));
            }
        }

        [Test]
        public void SameSeed_GivesSameMemory()
        {
            var a = new CpuBrain(99);
            var b = new CpuBrain(99);
            for (int i = 0; i < MemoryGame.CardCount; i++)
            {
                a.Observe(i, i / 2);
                b.Observe(i, i / 2);
            }
            for (int i = 0; i < MemoryGame.CardCount; i++)
                Assert.AreEqual(a.Knows(i), b.Knows(i), "card " + i);
        }

        [Test]
        public void UnobservedCard_IsNotKnown()
        {
            var brain = new CpuBrain(1, 1.0);
            Assert.IsFalse(brain.Knows(7));
        }
    }
}
