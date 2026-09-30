using System.Collections.Generic;
using UnityEngine;

namespace ZKTris.Core
{
    // Same order as the original Form.Forms enum so serialized data and the
    // preview sprites keep lining up.
    public enum PieceType
    {
        L,
        Z,
        T,
        O,
        J,
        S,
        I,
    }

    // A falling piece: a type, an origin on the board and the four cell
    // offsets of its current rotation.
    public class Tetromino
    {
        // Wall kicks tried in order when a rotation is blocked.
        static readonly Vector2Int[] Kicks =
        {
            new Vector2Int(0, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-2, 0),
            new Vector2Int(2, 0),
        };

        public PieceType Type { get; }
        public Vector2Int Origin { get; private set; }

        readonly Vector2Int[] offsets = new Vector2Int[4];
        readonly Vector2Int[] cells = new Vector2Int[4];

        public Tetromino(PieceType type, Vector2Int origin)
        {
            Type = type;
            Origin = origin;
            SpawnOffsets(type).CopyTo(offsets, 0);
            Refresh();
        }

        // Board positions of the four cells.
        public IReadOnlyList<Vector2Int> Cells => cells;

        // I and O rotate around the center of a 2x2 block rather than
        // around a cell, which keeps the O still and makes the I rotate in
        // place instead of wandering.
        bool CenteredBetweenCells => Type == PieceType.I || Type == PieceType.O;

        public static Vector2Int[] SpawnOffsets(PieceType type)
        {
            switch (type)
            {
                case PieceType.L: return new[] { V(-1, 0), V(0, 0), V(1, 0), V(1, 1) };
                case PieceType.J: return new[] { V(-1, 1), V(-1, 0), V(0, 0), V(1, 0) };
                case PieceType.T: return new[] { V(-1, 0), V(0, 0), V(1, 0), V(0, 1) };
                case PieceType.S: return new[] { V(-1, 0), V(0, 0), V(0, 1), V(1, 1) };
                case PieceType.Z: return new[] { V(-1, 1), V(0, 1), V(0, 0), V(1, 0) };
                case PieceType.O: return new[] { V(0, 0), V(1, 0), V(0, 1), V(1, 1) };
                default: return new[] { V(-1, 0), V(0, 0), V(1, 0), V(2, 0) };
            }
        }

        // Spawn position: horizontally centered, top cell on the top row.
        public static Vector2Int SpawnOrigin(PieceType type, int boardWidth, int boardHeight)
        {
            int maxY = 0;
            foreach (var o in SpawnOffsets(type))
                maxY = Mathf.Max(maxY, o.y);
            return new Vector2Int((boardWidth - 1) / 2, boardHeight - 1 - maxY);
        }

        public bool TryMove(Board board, Vector2Int delta)
        {
            if (!board.Fits(CellsAt(Origin + delta, offsets))) return false;
            Origin += delta;
            Refresh();
            return true;
        }

        // direction: +1 clockwise, -1 counter-clockwise.
        public bool TryRotate(Board board, int direction)
        {
            if (Type == PieceType.O) return false;

            var rotated = new Vector2Int[4];
            for (int i = 0; i < 4; i++)
                rotated[i] = Rotate(offsets[i], direction);

            foreach (var kick in Kicks)
            {
                if (board.Fits(CellsAt(Origin + kick, rotated)))
                {
                    Origin += kick;
                    rotated.CopyTo(offsets, 0);
                    Refresh();
                    return true;
                }
            }
            return false;
        }

        // How far the piece can fall from where it is.
        public int DropDistance(Board board)
        {
            int d = 0;
            while (board.Fits(CellsAt(Origin + new Vector2Int(0, -(d + 1)), offsets)))
                d++;
            return d;
        }

        public bool IsResting(Board board) => DropDistance(board) == 0;

        Vector2Int Rotate(Vector2Int p, int direction)
        {
            // Clockwise around (0,0): (x, y) -> (y, -x).
            // Around (0.5, 0.5) that becomes (y, 1 - x), still integers.
            int c = CenteredBetweenCells ? 1 : 0;
            return direction > 0 ? new Vector2Int(p.y, c - p.x) : new Vector2Int(c - p.y, p.x);
        }

        void Refresh()
        {
            for (int i = 0; i < 4; i++)
                cells[i] = Origin + offsets[i];
        }

        static IEnumerable<Vector2Int> CellsAt(Vector2Int origin, Vector2Int[] offs)
        {
            foreach (var o in offs)
                yield return origin + o;
        }

        static Vector2Int V(int x, int y) => new Vector2Int(x, y);
    }
}
