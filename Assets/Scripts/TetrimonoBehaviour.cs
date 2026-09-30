using System.Collections.Generic;
using UnityEngine;
using ZKTris.Core;

// Runs a game: gravity, lock delay, hold, next piece, scoring and game
// over. Input arrives through MoveTetrimonos; the board lives in GameGrid.
public class TetrimonoBehaviour : MonoBehaviour
{
    const string BestScoreKey = "ZKTris.BestScore";

    public GameGrid instance;
    public Sounds sounds;
    public Form form;
    public Hud hud;
    public GameFeel feel;
    // The four blocks of the falling piece, and of its ghost.
    public PositionOnGrid[] monos;
    public PositionOnGrid[] ghosts;

    [Header("Timing")]
    public float softDropInterval = 0.03f;
    public float lockDelay = 0.5f;
    // Moves/rotations that can restart the lock delay, so a piece can't
    // be stalled on the stack forever.
    public int maxLockResets = 15;

    [HideInInspector]
    public bool gamePaused;
    public bool IsGameOver { get; private set; }
    public bool SoftDropping { get; set; }

    readonly ScoreKeeper score = new ScoreKeeper();
    PieceBag bag;
    Tetromino current;
    PieceType next;
    PieceType? held;
    bool holdUsed;
    int color;
    float fallTimer;
    float lockTimer;
    int lockResets;
    int best;

    Board Board => instance.Board;

    void Start()
    {
        best = PlayerPrefs.GetInt(BestScoreKey, 0);
        NewGame();
    }

    public void NewGame()
    {
        instance.ResetBoard();
        score.Reset();
        bag = new PieceBag();
        held = null;
        next = bag.Next();
        IsGameOver = false;
        hud.HideGameOver();
        Spawn(bag.Next());
        UpdateHud();
    }

    void Update()
    {
        if (gamePaused || IsGameOver) return;

        float gravity = ScoreKeeper.FallInterval(score.Level);
        float interval = SoftDropping ? Mathf.Min(softDropInterval, gravity) : gravity;

        if (current.IsResting(Board))
        {
            fallTimer = 0f;
            lockTimer += Time.deltaTime;
            if (lockTimer >= lockDelay)
            {
                LockPiece();
                return;
            }
        }
        else
        {
            lockTimer = 0f;
            fallTimer += Time.deltaTime;
            while (fallTimer >= interval && current.TryMove(Board, Vector2Int.down))
            {
                fallTimer -= interval;
                if (SoftDropping) score.AddSoftDrop(1);
            }
        }
        Render();
    }

    public bool Move(int direction)
    {
        if (!CanAct()) return false;
        if (!current.TryMove(Board, new Vector2Int(direction, 0))) return false;
        sounds.PlayMove();
        OnPieceAdjusted();
        return true;
    }

    public void Rotate(int direction)
    {
        if (!CanAct()) return;
        if (!current.TryRotate(Board, direction)) return;
        sounds.PlayMove();
        OnPieceAdjusted();
    }

    public void HardDrop()
    {
        if (!CanAct()) return;
        int distance = current.DropDistance(Board);
        current.TryMove(Board, new Vector2Int(0, -distance));
        score.AddHardDrop(distance);
        sounds.PlayHardDrop();
        feel.HardDrop(distance);
        LockPiece();
    }

    public void Hold()
    {
        if (!CanAct() || holdUsed) return;
        var type = current.Type;
        if (held.HasValue) Spawn(held.Value);
        else Spawn(TakeNext());
        held = type;
        holdUsed = true;
        form.DisplayHold(held, false);
        sounds.PlayMove();
        Render();
    }

    bool CanAct() => !gamePaused && !IsGameOver && current != null;

    void OnPieceAdjusted()
    {
        // Moving or rotating a resting piece gives it more time, a limited
        // number of times.
        if (lockTimer > 0f && lockResets < maxLockResets)
        {
            lockTimer = 0f;
            lockResets++;
        }
        Render();
    }

    PieceType TakeNext()
    {
        var type = next;
        next = bag.Next();
        return type;
    }

    void Spawn(PieceType type)
    {
        current = new Tetromino(type, Tetromino.SpawnOrigin(type, Board.Width, Board.Height));
        color = instance.RandomColor();
        holdUsed = false;
        fallTimer = 0f;
        lockTimer = 0f;
        lockResets = 0;
        form.DisplayNextForm(next);
        form.DisplayHold(held, true);

        // Block out: no room to spawn.
        if (!Board.Fits(current.Cells)) EndGame();
        Render();
    }

    void LockPiece()
    {
        var cells = new List<Vector2Int>(current.Cells);
        int levelBefore = score.Level;
        LockResult result = Board.Lock(cells, color);
        instance.Redraw();
        sounds.PlayPlaced();

        if (result.newHoles.Count > 0)
        {
            score.AddHoles(result.newHoles.Count);
            sounds.PlayError();
            var positions = new List<Vector3>(result.newHoles.Count);
            foreach (var hole in result.newHoles)
                positions.Add(instance.CellToWorld(hole));
            feel.HolesCreated(positions);
            if (result.newHoles.Count >= 3) hud.ShowBanner("UNFORGIVABLE", Hud.HoleColor);
        }

        int cleared = result.clearedRows.Count;
        if (cleared > 0)
        {
            score.AddLines(cleared);
            sounds.PlayRowFill();
            var rows = new List<float>(cleared);
            foreach (int row in result.clearedRows)
                rows.Add(instance.CellToWorld(new Vector2Int(0, row)).y);
            feel.LinesCleared(rows, instance.CellToWorld(new Vector2Int(Board.Width / 2, 0)).x);

            if (score.Level > levelBefore) hud.ShowBanner("LEVEL " + score.Level, Hud.AccentColor);
            else if (cleared == 4) hud.ShowBanner("ZKTRIS!", Hud.AccentColor);
            else if (cleared > 1) hud.ShowBanner(cleared == 2 ? "DOUBLE" : "TRIPLE", Color.white);
        }

        UpdateHud();

        if (result.lockedOut)
        {
            EndGame();
            return;
        }
        Spawn(TakeNext());
    }

    void EndGame()
    {
        IsGameOver = true;
        SoftDropping = false;
        bool newBest = score.Score > best;
        if (newBest)
        {
            best = score.Score;
            PlayerPrefs.SetInt(BestScoreKey, best);
            PlayerPrefs.Save();
        }
        sounds.PlayError();
        feel.GameOver();
        foreach (var ghost in ghosts) ghost.Hide();
        UpdateHud();
        hud.ShowGameOver(score.Score, best, newBest);
    }

    void UpdateHud()
    {
        hud.SetStats(score.Score, Mathf.Max(best, score.Score), score.Level, score.Lines, score.Holes);
    }

    void Render()
    {
        if (current == null) return;
        var sprite = instance.SpriteForColor(color);
        int drop = IsGameOver ? 0 : current.DropDistance(Board);
        for (int i = 0; i < monos.Length; i++)
        {
            var cell = current.Cells[i];
            monos[i].Show(instance.CellToWorld(cell), sprite);
            if (drop > 0) ghosts[i].Show(instance.CellToWorld(cell + new Vector2Int(0, -drop)), sprite);
            else ghosts[i].Hide();
        }
    }
}
