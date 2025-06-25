using UnityEngine;
using UnityEngine.Tilemaps;

// The 7 possible tetromino types
public enum Tetromino
{
    T,
    O,
    J,
    L,
    I,
    S,
    Z
}

[System.Serializable]
public struct TetrominoData
{
    public Tetromino tetromino;     // Which tetromino this is (e.g. T, I, etc.)
    public Tile tile;               // The visual tile used on the Tilemap

    // The shape and wall kick data are set during initialization
    public Vector2Int[] cells { get; private set; }        // The cells that form the shape
    public Vector2Int[,] wallKicks { get; private set; }   // Offsets for rotation adjustments (wall kicks)

    // Initializes this data using static definitions (to keep things centralized and reusable)
    public void Initialize()
    {
        this.cells = Data.Cells[this.tetromino];
        this.wallKicks = Data.WallKicks[this.tetromino];
    }
}
