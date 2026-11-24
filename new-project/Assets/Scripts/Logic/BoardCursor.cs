namespace MemoryMatch
{
    public static class BoardCursor
    {
        // Moves the cursor on the grid; movement past an edge is ignored (no wrap-around).
        public static int Move(int index, int deltaColumn, int deltaRow)
        {
            int column = index % MemoryGame.Columns + deltaColumn;
            int row = index / MemoryGame.Columns + deltaRow;
            if (column < 0 || column >= MemoryGame.Columns || row < 0 || row >= MemoryGame.Rows) return index;
            return row * MemoryGame.Columns + column;
        }
    }
}
