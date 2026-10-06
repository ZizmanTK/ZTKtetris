using System.Linq;
using NUnit.Framework;
using UnityEngine;
using ZKTris.Core;

public class BoardTests
{
    const int W = 21, H = 39;

    static Vector2Int V(int x, int y) => new Vector2Int(x, y);

    static void FillRow(Board b, int y, int exceptX = -1)
    {
        for (int x = 0; x < b.Width; x++)
            if (x != exceptX) b.Set(x, y, CellState.Block, 1);
    }

    [Test]
    public void Lock_MarksGapsUnderThePieceAsHoles()
    {
        var b = new Board(W, H);
        // A horizontal I floating 3 rows up leaves 3 holes under each cell.
        var r = b.Lock(new[] { V(0, 3), V(1, 3), V(2, 3), V(3, 3) }, 1);
        Assert.AreEqual(12, r.newHoles.Count);
        Assert.AreEqual(CellState.Hole, b[0, 0]);
        Assert.AreEqual(CellState.Hole, b[3, 2]);
        Assert.AreEqual(CellState.Empty, b[4, 0]);
    }

    [Test]
    public void Lock_VerticalPieceDoesNotMarkItsOwnCells()
    {
        var b = new Board(W, H);
        var r = b.Lock(new[] { V(5, 0), V(5, 1), V(5, 2), V(5, 3) }, 1);
        Assert.AreEqual(0, r.newHoles.Count);
    }

    [Test]
    public void RowWithHole_CanNeverBeCleared()
    {
        var b = new Board(W, H);
        FillRow(b, 0, exceptX: 7);
        b.Set(7, 0, CellState.Hole);
        var r = b.Lock(new[] { V(0, 1), V(1, 1), V(2, 1), V(3, 1) }, 1);
        Assert.IsEmpty(r.clearedRows);
        Assert.AreEqual(CellState.Hole, b[7, 0]);
    }

    [Test]
    public void Lock_ClearsMultipleRowsAtOnce()
    {
        // Regression: the original code shifted rows while other blocks
        // still pointed at the old indices, so double clears could miss.
        var b = new Board(W, H);
        for (int y = 0; y < 4; y++) FillRow(b, y, exceptX: 20);
        b.Set(3, 4, CellState.Block, 2); // marker above the stack

        var r = b.Lock(new[] { V(20, 0), V(20, 1), V(20, 2), V(20, 3) }, 1);

        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, r.clearedRows);
        Assert.AreEqual(CellState.Block, b[3, 0], "marker should drop 4 rows");
        Assert.AreEqual(2, b.ColorAt(3, 0));
        Assert.AreEqual(CellState.Empty, b[3, 4]);
    }

    [Test]
    public void Lock_ClearsNonAdjacentRowsAndKeepsTheOneBetween()
    {
        var b = new Board(W, H);
        FillRow(b, 0, exceptX: 20);
        FillRow(b, 1, exceptX: 19); // row 1 stays incomplete
        b.Set(19, 1, CellState.Hole);
        FillRow(b, 2, exceptX: 20);
        b.Set(20, 1, CellState.Block, 1);

        var r = b.Lock(new[] { V(20, 0), V(20, 2), V(20, 3), V(20, 4) }, 1);

        CollectionAssert.AreEqual(new[] { 0, 2 }, r.clearedRows);
        Assert.AreEqual(CellState.Hole, b[19, 0], "row with the hole drops to the bottom");
        Assert.AreEqual(CellState.Block, b[20, 1]);
    }

    [Test]
    public void Lock_AboveTheTopIsALockOut()
    {
        var b = new Board(W, H);
        var r = b.Lock(new[] { V(0, H - 1), V(0, H) }, 1);
        Assert.IsTrue(r.lockedOut);
    }
}

public class TetrominoTests
{
    const int W = 21, H = 39;

    static Vector2Int V(int x, int y) => new Vector2Int(x, y);

    [Test]
    public void O_DoesNotRotate()
    {
        var b = new Board(W, H);
        var o = new Tetromino(PieceType.O, V(10, 10));
        var before = o.Cells.ToArray();
        Assert.IsFalse(o.TryRotate(b, 1));
        CollectionAssert.AreEqual(before, o.Cells.ToArray());
    }

    [Test]
    public void FourRotations_ReturnToStart([Values] PieceType type)
    {
        var b = new Board(W, H);
        var p = new Tetromino(type, V(10, 10));
        var before = p.Cells.OrderBy(c => c.x).ThenBy(c => c.y).ToArray();
        for (int i = 0; i < 4; i++) p.TryRotate(b, 1);
        CollectionAssert.AreEqual(before, p.Cells.OrderBy(c => c.x).ThenBy(c => c.y).ToArray());
    }

    [Test]
    public void Rotation_IsBlockedByPlacedBlocks()
    {
        // Regression: the original only checked the board edges, so a
        // piece could rotate into the stack.
        var b = new Board(W, H);
        var i = new Tetromino(PieceType.I, V(10, 1)); // cells (9..12, 1)
        // Wall the piece in so no rotation or kick fits.
        for (int x = 6; x <= 15; x++)
        {
            b.Set(x, 0, CellState.Block, 1);
            b.Set(x, 2, CellState.Block, 1);
            b.Set(x, 3, CellState.Block, 1);
        }
        var before = i.Cells.ToArray();
        Assert.IsFalse(i.TryRotate(b, 1));
        CollectionAssert.AreEqual(before, i.Cells.ToArray());
    }

    [Test]
    public void Rotation_KicksOffTheWall()
    {
        var b = new Board(W, H);
        var t = new Tetromino(PieceType.T, V(1, 10));
        Assert.IsTrue(t.TryRotate(b, 1));          // vertical, stem pointing right
        Assert.IsTrue(t.TryMove(b, V(-1, 0)));     // flush against the left wall
        Assert.IsTrue(t.TryRotate(b, 1));          // would poke out at x = -1 without a kick
        Assert.IsTrue(t.Cells.All(c => c.x >= 0));
    }

    [Test]
    public void Move_StopsAtWallsAndBlocks()
    {
        var b = new Board(W, H);
        var t = new Tetromino(PieceType.T, V(1, 5));
        Assert.IsFalse(t.TryMove(b, V(-1, 0)));
        b.Set(3, 5, CellState.Block, 1);
        Assert.IsFalse(t.TryMove(b, V(1, 0)));
        Assert.IsTrue(t.TryMove(b, V(0, -1)));
    }

    [Test]
    public void DropDistance_LandsOnTheStack()
    {
        var b = new Board(W, H);
        for (int x = 0; x < W; x++) b.Set(x, 0, CellState.Block, 1);
        var t = new Tetromino(PieceType.T, V(10, 10));
        Assert.AreEqual(9, t.DropDistance(b));
    }

    [Test]
    public void Spawn_FitsOnEmptyBoard([Values] PieceType type)
    {
        var b = new Board(W, H);
        var p = new Tetromino(type, Tetromino.SpawnOrigin(type, W, H));
        Assert.IsTrue(b.Fits(p.Cells));
        Assert.AreEqual(H - 1, p.Cells.Max(c => c.y));
    }

    [Test]
    public void I_CanRotateOnTheSpawnRow()
    {
        var b = new Board(W, H);
        var p = new Tetromino(PieceType.I, Tetromino.SpawnOrigin(PieceType.I, W, H));
        Assert.IsTrue(p.TryRotate(b, 1));
    }
}

public class ScoreAndBagTests
{
    [Test]
    public void Scoring_UsesLevelMultiplier()
    {
        var s = new ScoreKeeper();
        Assert.AreEqual(800, s.AddLines(4));
        for (int i = 0; i < 6; i++) s.AddLines(1); // 10 lines -> level 2
        Assert.AreEqual(2, s.Level);
        Assert.AreEqual(600, s.AddLines(2));
    }

    [Test]
    public void FallInterval_StartsAtOriginalSpeedAndSpeedsUp()
    {
        Assert.AreEqual(0.4f, ScoreKeeper.FallInterval(1), 1e-5f);
        Assert.Less(ScoreKeeper.FallInterval(5), ScoreKeeper.FallInterval(4));
        Assert.GreaterOrEqual(ScoreKeeper.FallInterval(100), 0.03f);
    }

    [Test]
    public void Bag_DealsEachPieceOncePerSeven()
    {
        var bag = new PieceBag(seed: 42);
        for (int round = 0; round < 5; round++)
        {
            var seven = Enumerable.Range(0, 7).Select(_ => bag.Next()).Distinct().Count();
            Assert.AreEqual(7, seven);
        }
    }
}

public class PieceQueueTests
{
    [Test]
    public void Take_ReturnsTheFirstUpcomingPieceAndKeepsTheSize()
    {
        var queue = new PieceQueue(new PieceBag(seed: 7), 3);
        var first = queue.Upcoming[0];
        var second = queue.Upcoming[1];
        Assert.AreEqual(first, queue.Take());
        Assert.AreEqual(second, queue.Upcoming[0]);
        Assert.AreEqual(3, queue.Upcoming.Count);
    }
}
