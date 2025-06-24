using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UIElements;

public class Block : MonoBehaviour
{
    public Board board { get; private set; }

    // Gegevens over de vorm
    public TetrominoData data { get; private set; }
   
    //de vier blokjes waaruit een tetromino bestaat
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex { get; private set; }
    public float stepDelay = 1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float lockTime;

    // Initialiseer het blok met een bord, positie en tetromino-data
    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;
        this.stepTime = Time.time + stepDelay;
        this.lockTime = 0f;

        // Als de cellen-array nog niet bestaat, maak hem aan
        if (this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        // Kopieer de celposities uit de tetromino-data
        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }
    public void Update()
    {
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            Rotate(1);
        }


        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(Vector2Int.right);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(Vector2Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            //hard drop, moves down untill it cant anymore
            while (Move(Vector2Int.down))
            {
                continue;
            }
        }
            this.board.Set(this);
    }
    private bool Move(Vector2Int movement)
    {
        Vector3Int newPosition = this.position; 
        newPosition.x += movement.x;
        newPosition.y += movement.y;
        bool valid = this.board.IsValidPosition(this, newPosition);
        if (valid) 
        {
            this.position = newPosition;
            lockTime = 0f;
        }
        return valid;
    }
    private void Rotate(int direction)
    {
        int fromRotation = this.rotationIndex;
        int toRotation = Wrap(this.rotationIndex + direction, 0, 4);

        this.rotationIndex = toRotation;
        ApplyRotation(direction);

        if (!TestWallKicks(fromRotation, toRotation))
        {
            this.rotationIndex = fromRotation;
            ApplyRotation(-direction);
        }
    }
    private void ApplyRotation(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        // draai alle cells aan de hand van een rotation matrix
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    // "I" en "O" worden anders gedraaid vanwege dat ze geen center point hebben
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }
    private bool TestWallKicks(int from, int to)
    {
        int wallKickIndex = GetWallKickIndex(from, to);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];
            if (Move(translation))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int from, int to)
    {
        if (from == 0 && to == 1) return 0;
        if (from == 1 && to == 2) return 1;
        if (from == 2 && to == 3) return 2;
        if (from == 3 && to == 0) return 3;
        if (from == 1 && to == 0) return 4;
        if (from == 2 && to == 1) return 5;
        if (from == 3 && to == 2) return 6;
        if (from == 0 && to == 3) return 7;

        return 0; 
    }
    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (max - input) % (max - min);
        }
        else
        {
            return min - (input - min) % (max - min);
        }
    }
}
