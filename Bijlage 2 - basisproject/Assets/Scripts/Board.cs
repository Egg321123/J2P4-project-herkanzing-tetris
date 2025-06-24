using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Block activeBlock { get; private set; }
    // Array met alle mogelijke tetromino's
    public TetrominoData[] tetrominos;

    // De positie waar nieuwe blokken gespawned worden
    [SerializeField] private Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }
    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activeBlock = GetComponentInChildren<Block>();

        // Initialiseer alle tetromino's
        for (int i = 0; i < tetrominos.Length; i++)
        {
            this.tetrominos[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    // Spawnt een willekeurig blok op het bord
    public void SpawnPiece()
    {
        // Kies een willekeurige tetromino
        int random = Random.Range(0, this.tetrominos.Length);
        TetrominoData data = this.tetrominos[random];

        // Initialiseer het actieve blok met de tetromino en spawnpositie
        this.activeBlock.Initialize(this, this.spawnPosition, data);
        Set(this.activeBlock);
    }

    // Zet de cellen van een blok op de tilemap
    public void Set(Block block)
    {
        for (int i = 0; i < block.cells.Length; i++)
        {
            // Bereken de positie van de cel op de tilemap
            Vector3Int tilePosition = block.cells[i] + block.position;

            // Plaats de tegel op de tilemap
            this.tilemap.SetTile(tilePosition, block.data.tile);
        }
    }
    public void Clear(Block block)
    {
        for (int i = 0; i < block.cells.Length; i++)
        {
            // Bereken de positie van de cel op de tilemap
            Vector3Int tilePosition = block.cells[i] + block.position;

            // verweider de tegel op de tilemap
            this.tilemap.SetTile(tilePosition, null);
        }
    }
    public bool IsValidPosition(Block block, Vector3Int position)
    {
        RectInt bounds = this.Bounds;
        for (int i = 0; i < block.cells.Length; i++)
        {
            Vector3Int tilePosition = block.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }
            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }
}
