using UnityEngine;
using UnityEngine.Tilemaps;
using ZKTris.Core;

// Owns the board model and draws it into the tilemap.
public class GameGrid : MonoBehaviour
{
    public const int Width = 21;
    public const int Height = 39;

    public Tilemap tilemap;
    // Background tile drawn in empty cells.
    public Tile emptyTile;
    public Tile errorTile;
    public Tile[] baseCubes;
    // World position of the centre of cell (0, 0).
    public Vector2 origin = new Vector2(-9.5f, -21.5f);

    public Board Board { get; private set; }

    void Awake()
    {
        Board = new Board(Width, Height);
    }

    public Vector3 CellToWorld(Vector2Int cell) => origin + (Vector2)cell;

    public int RandomColor() => Random.Range(0, baseCubes.Length);

    public Sprite SpriteForColor(int color) => baseCubes[color].sprite;

    public void ResetBoard()
    {
        Board.Clear();
        Redraw();
    }

    public void Redraw()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Tile tile;
                switch (Board[x, y])
                {
                    case CellState.Block: tile = baseCubes[Board.ColorAt(x, y)]; break;
                    case CellState.Hole: tile = errorTile; break;
                    default: tile = emptyTile; break;
                }
                var cell = tilemap.WorldToCell(CellToWorld(new Vector2Int(x, y)));
                if (tilemap.GetTile(cell) != tile)
                    tilemap.SetTile(cell, tile);
            }
        }
    }
}
