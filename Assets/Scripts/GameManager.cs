using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Shapes of spawned blocks as in tetris
public enum Shapes{
        Long,
        Block,
        T,
        S
};

// Shapes will be generated for the Game Manager to spawn the blocks
interface ShapeGenerator
{
    Shapes nextShape();
}

// This picks the next shape randomly
class RandomGenerator : ShapeGenerator
{
    Shapes[] shapes;
    public RandomGenerator()
    {
        shapes = (Shapes[])System.Enum.GetValues(typeof(Shapes));
    }
    public Shapes nextShape()
    { 
        int randIndex = shapes.Length;
        while(randIndex == shapes.Length)   // Random.value may return 1
        {
            randIndex = (int)(Random.value * shapes.Length);
        }
        return shapes[randIndex];
    }
}

public class GameManager : MonoBehaviour
{
    
    private enum Directions
    {
        Left,
        Right,
        Down
    };

    [SerializeField]
    private GameObject[] blockPrefabs;         // Block to copy each time blocks are spawned

    [SerializeField]
    private int ROWCOUNT = 10;              // Number of blocks that can fit horizontally 

    [SerializeField]
    private float waitTime = 1f;            // Number of seconds to wait before moving active blocks down a step
    
    private int COLUMNCOUNT = 30;           // Number of blocks to allowed vertically
    
    private Block[,] occupied;         // Stores landed blocks and used to determine when blocks should land
                                            // Coordinate (0, 0) corresponds to a block at the bottom left corner
                                            // of the playable area
    private int highestUnoccupied; // Highest unoccupied row (just to optimize a bit)
    private int[] blocksInRow;       // Stores block-counts for each row

    private Block[] activeBlocks;         
    
    

    private Vector2 bottomLeft, topRight;   // Coordinates of corners of screen in World coordinates
    
    private float blockWidth;       // and height
    
    private Vector3 spawnPos, HorizontalShift, VerticalShift;

    private Vector2 spawnCoords, HorizontalCoordShift = new Vector2(1, 0), VerticalCoordShift = new Vector2(0, 1);

    private bool spawnNew = true;

    // Event system to be triggered each time blocks land
    private delegate void OnBlockLand();
    private event OnBlockLand BlockLanded;

    private delegate void OnBlockMove();
    private event OnBlockMove BlockMove;    

    private bool gamePaused = false;

    private ShapeGenerator generator;
   
    IEnumerator activeCoroutine;    // The active coroutine that moves blocks downsssss
    Shapes currentShape;

    // Start is called before the first frame update
    void Awake()
    {   
        // Determine key constants for the rest of the game and Initialize data structures to keep track of game status
        
        blockWidth = blockPrefabs[0].GetComponent<SpriteRenderer>().bounds.size.x;

        activeBlocks = new Block[4];

        Camera cam = Camera.main;
        bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        spawnPos = new Vector3(-blockWidth/ 2 * (1 - ROWCOUNT % 2), bottomLeft.y + (blockWidth * (COLUMNCOUNT + 0.5f + 1)), 0);
        spawnCoords = new Vector2(ROWCOUNT / 2 - 1 + (ROWCOUNT % 2), COLUMNCOUNT + 1);

        HorizontalShift = new Vector3(blockWidth, 0, 0);
        VerticalShift = new Vector3(0, blockWidth, 0);

        occupied = new Block[ROWCOUNT, COLUMNCOUNT + 3];
        highestUnoccupied = 0;
        blocksInRow = new int[COLUMNCOUNT + 3];

        generator = new RandomGenerator();
    }

    void OnEnable()
    {
        // Subscribe to events

        BlockLanded += blockLandHandler1;

        BlockLanded += blockLandHandler2;

        BlockMove += checkLand;

        PauseManager.TogglePause += OnTogglePause;
    }

    void OnDisable()
    {
        // Unsubscribe to events

        BlockLanded -= blockLandHandler1;

        BlockLanded -= blockLandHandler2;

        BlockMove -= checkLand;

        PauseManager.TogglePause -= OnTogglePause;
    }

    // Update is called once per frame
    void Update()
    {
        if (gamePaused)
        {
            return;
        }
        
        if(spawnNew){
            currentShape = generator.nextShape();
            spawnBlocks(currentShape);
           
            
            spawnNew = false;
            activeCoroutine = moveActive();
            StartCoroutine(activeCoroutine);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && canMoveLeft())
        {
            moveBlocks(Directions.Left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && canMoveRight())
        {
            moveBlocks(Directions.Right);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            rotateBlocks(currentShape, false);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            rotateBlocks(currentShape, true);
        }
    }

    void spawnBlocks(Shapes shape)
    {
        Vector3[] positions = new Vector3[4];
        
        Vector2[] coords = new Vector2[4];

        switch (shape)
        {
            case Shapes.Long:

                positions[0] = spawnPos - HorizontalShift;
                coords[0] = spawnCoords - HorizontalCoordShift;
        
                positions[1] = spawnPos;
                coords[1] = spawnCoords;

                positions[2] = spawnPos + HorizontalShift;
                coords[2] = spawnCoords + HorizontalCoordShift;

                positions[3] = spawnPos + (HorizontalShift * 2);
                coords[3] = spawnCoords + (HorizontalCoordShift * 2);
                break;
            
             case Shapes.Block:

                positions[0] = spawnPos;
                coords[0] = spawnCoords;
        
                positions[1] = spawnPos + HorizontalShift;
                coords[1] = spawnCoords + HorizontalCoordShift;

                positions[2] = spawnPos + HorizontalShift + VerticalShift;
                coords[2] = spawnCoords + HorizontalCoordShift + VerticalCoordShift;

                positions[3] = spawnPos + VerticalShift;
                coords[3] = spawnCoords + VerticalCoordShift;
                break;
             
             case Shapes.S:

                positions[0] = spawnPos - HorizontalShift;
                coords[0] = spawnCoords - HorizontalCoordShift;
        
                positions[1] = spawnPos;
                coords[1] = spawnCoords;

                positions[2] = spawnPos + VerticalShift;
                coords[2] = spawnCoords + VerticalCoordShift;

                positions[3] = spawnPos + VerticalShift + HorizontalShift;
                coords[3] = spawnCoords + VerticalCoordShift + HorizontalCoordShift;
                break;
             
             case Shapes.T:

                positions[0] = spawnPos - HorizontalShift;
                coords[0] = spawnCoords - HorizontalCoordShift;
        
                positions[1] = spawnPos;
                coords[1] = spawnCoords;

                positions[2] = spawnPos + VerticalShift;
                coords[2] = spawnCoords + VerticalCoordShift;

                positions[3] = spawnPos + HorizontalShift;
                coords[3] = spawnCoords + HorizontalCoordShift;
                break;

        }

        for (int i = 0; i < 4; i++)
        {
            activeBlocks[i] = Instantiate(blockPrefabs[(int) currentShape]).GetComponent<Block>();
            activeBlocks[i].transform.position = positions[i];
            activeBlocks[i].coords = coords[i];
        }

        
        
    }

    IEnumerator moveActive()
    {
        // This moves blocks down continuously until they land
        while (true)
        {
            yield return new WaitForSeconds(waitTime);

            moveBlocks(Directions.Down);

        }
    }

    bool checkOccupied(Vector2 coords)
    {
        return occupied[(int)coords.x, (int)coords.y] != null;
    }

    void rotateBlocks(Shapes shape, bool anticlockwise)
    {
        Vector2[] coords = new Vector2[activeBlocks.Length];
        int pivotIndex = 1;

        for(int i = 0; i < activeBlocks.Length; i++)
        {
            coords[i] = activeBlocks[i].coords;
        }

        Vector2[] shifts = rotation_aux(coords, pivotIndex, anticlockwise);

        for(int i = 0; i < activeBlocks.Length; i++)    // Check if rotation is possible
        {
        
            if (checkOccupied(coords[pivotIndex] + shifts[i]))
            {
                return; 
            }
        }

        for(int i = 0; i < activeBlocks.Length; i++)
        {
            activeBlocks[i].coords = coords[pivotIndex] + shifts[i];
            activeBlocks[i].transform.position = activeBlocks[pivotIndex].transform.position + (Vector3)(shifts[i] * blockWidth);
        }
        checkLand();

    }

    // The following returns shifts relative to the pivot of the coordinates resulting from rotation

    Vector2[] rotation_aux(Vector2[] coords, int pivotIndex, bool anticlockwise)
    {
        Vector2[] shifts = new Vector2[coords.Length];
        Vector2 diff;

        int multiplier = anticlockwise?1 : -1;

        for (int i = 0; i < coords.Length; i++)
        {
            diff = coords[i] - coords[pivotIndex];
            shifts[i] = multiplier * new Vector2(-diff.y, diff.x);
        }


        return shifts;
    }

    void moveBlocks(Directions direction)
    {
        // move blocks in specified direction and check for landing
        switch (direction)
        {
            case Directions.Left:
                
                foreach(Block activeBlock in activeBlocks)
                    activeBlock.move(-1 * HorizontalShift, -1 * HorizontalCoordShift);
                
                break;
           
            case Directions.Right:

                foreach(Block activeBlock in activeBlocks)
                    activeBlock.move(HorizontalShift, HorizontalCoordShift);

                break;

            case Directions.Down:

                foreach(Block activeBlock in activeBlocks)
                    activeBlock.move(-1 * VerticalShift, -1 * VerticalCoordShift);

                break;
        }

        BlockMove();
    }

    void blockLandHandler1(){
        
        // Land The blocks

        StopCoroutine(activeCoroutine);
        
        foreach(Block activeBlock in activeBlocks)
        {
            occupied[(int)activeBlock.coords.x, (int)activeBlock.coords.y] = activeBlock;
            
            if(activeBlock.coords.y >= highestUnoccupied)
                highestUnoccupied = (int)activeBlock.coords.y + 1;
            
            blocksInRow[(int)activeBlock.coords.y] += 1;
        }
          
    }

    void blockLandHandler2()
    {
        // Check if rows have been filled, destroy filled rows, and move upper rows down by appropiate amounts

        int lowestDestroyed = highestUnoccupied;    // this gives us the lowest unoccupied row after destroying full rows
        
        foreach(Block activeBlock in activeBlocks)
        {
            if (activeBlock != null && blocksInRow[(int)activeBlock.coords.y] == ROWCOUNT)
            { 
                for(int i = 0; i < ROWCOUNT; i++)   // destroy row
                {
                    Destroy(occupied[i, (int)activeBlock.coords.y].gameObject);
                    occupied[i, (int)activeBlock.coords.y] = null;
                }
                blocksInRow[(int)activeBlock.coords.y] = 0;     
                
                if(activeBlock.coords.y < lowestDestroyed) 
                    lowestDestroyed = (int)activeBlock.coords.y;
            }
        }

        int shift = 0;  // keeps track of hom much to shift occupied rows 

        for(int j = lowestDestroyed; j < highestUnoccupied; j++)
        {
            if (blocksInRow[j] == 0)
            {
                shift++;    // each time a destroyed row is encountered, shift is incremented
                continue;
            }

            for(int i = 0; i < ROWCOUNT; i++)   // shift down occupied rows
            {
                occupied[i, j - shift] = occupied[i, j];
                if (occupied[i, j] != null)
                {
                    occupied[i, j].move(-shift * VerticalShift, -shift * VerticalCoordShift);
                    occupied[i, j] = null;
                }
            }
            blocksInRow[j - shift] = blocksInRow[j];
            blocksInRow[j] = 0;
        }
        highestUnoccupied -= shift;

        spawnNew = true;
    

    }
    void checkLand()
    {
        // Blocks land when one reaches the bottom or is right above a landed block
        
        foreach(Block activeBlock in activeBlocks)
        {
            if (activeBlock.coords.y == 0 || checkOccupied(new Vector2(activeBlock.coords.x, activeBlock.coords.y - 1)))
            {
                BlockLanded();
                return;
            }

        }
        
    }

    bool canMoveRight()
    {
        // Just check if any block is on the right extreme
        foreach(Block activeBlock in activeBlocks)
        {
            if(activeBlock.coords.x == ROWCOUNT - 1 || checkOccupied(new Vector2(activeBlock.coords.x + 1, activeBlock.coords.y)))
            {
                return false;
            }
        }
        return true;
    }

    bool canMoveLeft()
    {
        // Just check if any block is on the left extreme
        foreach(Block activeBlock in activeBlocks)
        {
            if(activeBlock.coords.x == 0 || checkOccupied(new Vector2(activeBlock.coords.x - 1, activeBlock.coords.y)))
            {
                return false;
            }
        }
        return true; 
    }

    void OnTogglePause()
    {
        gamePaused ^= true;
    }

}