using System.Collections.Generic;
using UnityEngine;

namespace ZKTris.Core
{
    public enum CellState : byte
    {
        Empty,
        Block,
        // A gap left under a locked block. Unforgivable: it can never be
        // filled, and a row containing one can never be cleared.
        Hole,
    }

    public struct LockResult
    {
        // Rows (bottom-up, indices before the clear) that were cleared.
        public List<int> clearedRows;
        // Cells that became holes because of this lock.
        public List<Vector2Int> newHoles;
        // Part of the piece locked above the top row: game over.
        public bool lockedOut;
    }

    // Pure game-state model of the playfield. x = column (0 = left),
    // y = row (0 = bottom). No Unity scene dependencies, so it can be
    // unit tested headless.
    public class Board
    {
        public readonly int Width;
        public readonly int Height;

        readonly CellState[,] cells;
        readonly int[,] colors;

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new CellState[width, height];
            colors = new int[width, height];
        }

        public CellState this[int x, int y] => cells[x, y];
        public int ColorAt(int x, int y) => colors[x, y];

        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        // Rows above the top are an open buffer so pieces can spawn and
        // rotate there; locking into it is a lock out.
        public bool IsFree(int x, int y) =>
            x >= 0 && x < Width && y >= 0 && (y >= Height || cells[x, y] == CellState.Empty);

        public bool Fits(IEnumerable<Vector2Int> positions)
        {
            foreach (var p in positions)
                if (!IsFree(p.x, p.y)) return false;
            return true;
        }

        public void Set(int x, int y, CellState state, int color = 0)
        {
            cells[x, y] = state;
            colors[x, y] = color;
        }

        public void Clear()
        {
            System.Array.Clear(cells, 0, cells.Length);
            System.Array.Clear(colors, 0, colors.Length);
        }

        // Locks a piece into the board, marks the gaps it leaves as holes,
        // then clears every completed row.
        public LockResult Lock(IReadOnlyList<Vector2Int> positions, int color)
        {
            var result = new LockResult { clearedRows = new List<int>(), newHoles = new List<Vector2Int>() };

            foreach (var p in positions)
            {
                if (p.y >= Height) result.lockedOut = true;
                else Set(p.x, p.y, CellState.Block, color);
            }

            // Everything empty below a freshly placed block, down to the
            // stack, is a hole. Blocks of the same piece stop the scan, so
            // a vertical piece doesn't mark its own cells.
            foreach (var p in positions)
            {
                for (int y = Mathf.Min(p.y, Height) - 1; y >= 0 && cells[p.x, y] == CellState.Empty; y--)
                {
                    Set(p.x, y, CellState.Hole);
                    result.newHoles.Add(new Vector2Int(p.x, y));
                }
            }

            // Scan top-down so removing a row doesn't shift rows we still
            // have to check; record indices as they were before the clear.
            for (int y = Height - 1; y >= 0; y--)
            {
                if (IsRowComplete(y))
                {
                    result.clearedRows.Add(y);
                    RemoveRow(y);
                }
            }
            result.clearedRows.Reverse();
            return result;
        }

        public bool IsRowComplete(int y)
        {
            for (int x = 0; x < Width; x++)
                if (cells[x, y] != CellState.Block) return false;
            return true;
        }

        public int CountHoles()
        {
            int n = 0;
            foreach (var c in cells)
                if (c == CellState.Hole) n++;
            return n;
        }

        void RemoveRow(int row)
        {
            for (int y = row; y < Height - 1; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    cells[x, y] = cells[x, y + 1];
                    colors[x, y] = colors[x, y + 1];
                }
            }
            for (int x = 0; x < Width; x++)
            {
                cells[x, Height - 1] = CellState.Empty;
                colors[x, Height - 1] = 0;
            }
        }
    }
}
