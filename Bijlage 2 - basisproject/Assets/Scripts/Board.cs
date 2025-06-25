using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{

    //referece to the score display
    [SerializeField] private ScoreDisplayUI scoreDisplay;
    [SerializeField] private ScoreDisplayUI LineDisplay;
    
    // Reference to the tilemap that displays the blocks
    public Tilemap tilemap { get; private set; }

    // The currently active falling block
    public Block activeBlock { get; private set; }

    // All possible tetromino types (defined in inspector)
    public TetrominoData[] tetrominos;

    // Spawn location for new blocks
    [SerializeField] private Vector3Int spawnPosition;

    // Size of the playfield (width x height)
    public Vector2Int boardSize = new(10, 20);

    // Rectangle representing the playfield boundaries
    public RectInt Bounds
    {
        get
        {
            // Origin is centered; shift to get bottom-left corner
            Vector2Int position = new(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        // Cache references to Tilemap and Block components
        tilemap = GetComponentInChildren<Tilemap>();
        activeBlock = GetComponentInChildren<Block>();

        // Initialize tetromino data (e.g. rotation matrices)
        for (int i = 0; i < tetrominos.Length; i++)
        {
            tetrominos[i].Initialize();
        }
    }

    private void Start()
    {
        // Start game with a new piece
        SpawnPiece();
    }

    // Spawn a random tetromino on the board
    public void SpawnPiece()
    {
        int random = Random.Range(0, tetrominos.Length); // Pick random index
        TetrominoData data = tetrominos[random];         // Get tetromino data

        

        // Set up active block at spawn position
        activeBlock.Initialize(this, spawnPosition, data);

        // Check if the new block fits in spawn position
        if (!IsValidPosition(activeBlock, activeBlock.position))
        {
            OnGameOver();
            return;
        }

        // Place block tiles onto tilemap
        Set(activeBlock);
    }

    // Place block tiles on tilemap
    public void Set(Block block)
    {
        for (int i = 0; i < block.cells.Length; i++)
        {
            // Convert cell to world tilemap position
            Vector3Int tilePosition = block.cells[i] + block.position;

            // Draw tile at position
            tilemap.SetTile(tilePosition, block.data.tile);
        }
    }

    // Clear block tiles from tilemap
    public void Clear(Block block)
    {
        for (int i = 0; i < block.cells.Length; i++)
        {
            Vector3Int tilePosition = block.cells[i] + block.position;

            // Remove tile from tilemap
            tilemap.SetTile(tilePosition, null);
        }
    }

    // Check if a block fits at a given position
    public bool IsValidPosition(Block block, Vector3Int position)
    {
        RectInt bounds = Bounds;

        for (int i = 0; i < block.cells.Length; i++)
        {
            Vector3Int tilePosition = block.cells[i] + position;

            // If outside bounds or colliding with another tile, return false
            if (!bounds.Contains((Vector2Int)tilePosition) || tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    // Check for and clear full lines, also updates score
    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;
        int linesCleared = 0;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                linesCleared++;
                // Re-check same row because everything dropped
            }
            else
            {
                row++;
            }
        }

        // Scoring: based on lines cleared at once
        if (linesCleared > 0 && scoreDisplay != null)
        {
            int points = 0;

            switch (linesCleared)
            {
                case 1: 
                    points = 100; 
                    break;
                case 2: 
                    points = 300; 
                    break;
                case 3: 
                    points = 500; 
                    break;
                case 4: 
                    points = 800; 
                    break; 
                default:
                    Debug.LogWarning("Unusual number of lines cleared: " + linesCleared);
                    points = linesCleared * 200; 
                    break;
            }
            LineDisplay.AddScore(linesCleared);
            scoreDisplay.AddScore(points);
        }
    }


    // Returns true if every tile in a row is filled
    private bool IsLineFull(int row)
    {
        RectInt b = Bounds;

        for (int col = Bounds.xMin; col < b.xMax; col++)
        {
            // Use Vector3Int because Tilemap API requires it
            Vector3Int position = new(col, row, 0);

            if (!tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }

    // Clear a full line and drop above tiles down
    private void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // Clear tiles in the given row
        for (int col = Bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Move tiles above down by one row
        while (row < bounds.yMax)
        {
            for (int col = Bounds.xMin; col < bounds.xMax; col++)
            {
                // Get tile from above
                Vector3Int position = new(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                // Place it in the current row
                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }
    void OnGameOver()
    {
        Debug.Log("Game Over!");

        // Clear entire board
        tilemap.ClearAllTiles();

        // Reset scores
        if (scoreDisplay != null)
            scoreDisplay.ResetScore();

        if (LineDisplay != null)
            LineDisplay.ResetScore();
    }

}
