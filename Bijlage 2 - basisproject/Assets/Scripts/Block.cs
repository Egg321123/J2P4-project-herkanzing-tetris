using UnityEngine;
using UnityEngine.UIElements;

public class Block : MonoBehaviour
{
    // Reference to the board this block is on
    public Board board { get; private set; }

    // Shape and wall kick data of this tetromino
    public TetrominoData data { get; private set; }

    // The four tiles that make up a tetromino
    public Vector3Int[] cells { get; private set; }

    // The current grid position of the block (used as the pivot)
    public Vector3Int position { get; private set; }

    // The current rotation index (0–3)
    public int rotationIndex { get; private set; }

    // Time between each automatic downward step
    public float stepDelay = 1f;

    // Time before the piece locks in place after hitting something
    public float lockDelay = 0.5f;

    private float stepTime; // Time to perform the next automatic step
    private float lockTime; // Timer to track how long the block has been still

    // Called when the block is created
    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        lockTime = 0f;

        // Initialize the cells array if not already done
        if (cells == null)
        {
            cells = new Vector3Int[data.cells.Length];
        }

        // Copy the base cell positions from the data (no rotation applied yet)
        for (int i = 0; i < data.cells.Length; i++)
        {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    public void Update()
    {
        // Remove the block from the board temporarily to test moves
        board.Clear(this);

        // Track how long we've been sitting still
        lockTime += Time.deltaTime;

        // Handle rotation input
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Rotate(-1); // Rotate counterclockwise
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            Rotate(1); // Rotate clockwise
        }

        // Handle horizontal/vertical movement input
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
            // Hard drop: move the block down until it can't move anymore
            while (Move(Vector2Int.down)) continue;
        }

        // Handle automatic downward step
        if (Time.time >= stepTime)
        {
            Step();
        }

        // Re-draw the block in its new position
        board.Set(this);
    }

    // Attempt to move the block in a direction (returns true if successful)
    private bool Move(Vector2Int movement)
    {
        Vector3Int newPosition = position;
        newPosition.x += movement.x;
        newPosition.y += movement.y;

        // Check if the new position is valid
        bool valid = board.IsValidPosition(this, newPosition);
        if (valid)
        {
            position = newPosition;
            lockTime = 0f; // Reset lock timer if we moved
        }

        return valid;
    }

    // Called when it's time to automatically move the block down
    private void Step()
    {
        stepTime = Time.time + stepDelay;

        Move(Vector2Int.down);

        // Lock the block if it's been still for long enough
        if (lockTime >= lockDelay)
        {
            Lock();
        }
    }

    // Finalizes the block's position and spawns a new one
    private void Lock()
    {
        board.Set(this);      // Lock in the block
        board.ClearLines();   // Check and clear any full lines
        board.SpawnPiece();   // Spawn the next block
    }

    // Rotates the block clockwise or counterclockwise
    private void Rotate(int direction)
    {
        int fromRotation = rotationIndex;
        int toRotation = Wrap(rotationIndex + direction, 0, 4);
        rotationIndex = toRotation;

        ApplyRotation(direction);

        // Try wall kicks to make the rotation fit
        if (!TestWallKicks(fromRotation, toRotation))
        {
            // If all wall kicks failed, revert the rotation
            rotationIndex = fromRotation;
            ApplyRotation(-direction);
        }
    }

    // Actually rotate the cells using a rotation matrix
    private void ApplyRotation(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];
            int x, y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    // These shapes rotate around a half-cell pivot
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    // Most pieces rotate around their center
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    // Try each wall kick position for a rotation
    private bool TestWallKicks(int from, int to)
    {
        int wallKickIndex = GetWallKickIndex(from, to);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];
            if (Move(translation))
            {
                return true; // Found a valid position after wall kick
            }
        }

        return false;
    }

    // Determine which wall kick data to use based on rotation change
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

    // Keeps rotationIndex between 0 and 3
    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (max - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
}
