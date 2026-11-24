using NUnit.Framework;

namespace MemoryMatch.Tests
{
    // FR-005: arrow keys move the cursor over the 5 x 4 grid and never leave it.
    public class BoardCursorTests
    {
        [Test]
        public void Right_MovesOneColumn() { Assert.AreEqual(1, BoardCursor.Move(0, 1, 0)); }

        [Test]
        public void Down_MovesOneRow() { Assert.AreEqual(5, BoardCursor.Move(0, 0, 1)); }

        [Test]
        public void Left_And_Up_MoveBack() { Assert.AreEqual(6, BoardCursor.Move(7, -1, 0)); Assert.AreEqual(2, BoardCursor.Move(7, 0, -1)); }

        [Test]
        public void Edges_AreClamped_NoWrapAround()
        {
            Assert.AreEqual(0, BoardCursor.Move(0, -1, 0));
            Assert.AreEqual(0, BoardCursor.Move(0, 0, -1));
            Assert.AreEqual(4, BoardCursor.Move(4, 1, 0));      // right edge of the first row
            Assert.AreEqual(19, BoardCursor.Move(19, 1, 0));
            Assert.AreEqual(19, BoardCursor.Move(19, 0, 1));
            Assert.AreEqual(15, BoardCursor.Move(15, -1, 0));   // left edge of the last row
        }
    }
}
