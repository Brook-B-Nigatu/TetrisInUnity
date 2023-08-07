using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Shapes of spawned blocks as in tetris
    private enum Shapes{
        Long,
        Block,
        T,
        S
    };

    private enum Directions
    {
        Left,
        Right,
        Down
    };

    [SerializeField]
    private GameObject blockPrefab;         // Block to copy each time blocks are spawned

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
   
    IEnumerator activeCoroutine;    // The active coroutine that moves blocks downsssss

    // Start is called before the first frame update
    void Awake()
    {   
        // Determine key constants for the rest of the game and Initialize data structures to keep track of game status
        
        blockWidth = blockPrefab.GetComponent<SpriteRenderer>().bounds.size.x;

        activeBlocks = new Block[4];

        Camera cam = Camera.main;
        bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        spawnPos = new Vector3(-blockWidth/ 2 * (1 - ROWCOUNT % 2), bottomLeft.y + (blockWidth * (COLUMNCOUNT + 0.5f + 1)), 0);
        spawnCoords = new Vector2(ROWCOUNT / 2 - 1 + (ROWCOUNT % 2), COLUMNCOUNT + 1);

        HorizontalShift = new Vector3(blockWidth, 0, 0);
        VerticalShift = new Vector3(0, blockWidth, 0);

        occupied = new Block[ROWCOUNT, COLUMNCOUNT + 2];
        highestUnoccupied = 0;
        blocksInRow = new int[COLUMNCOUNT + 2];
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
            
            spawnBlocks();
            
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
    }

    void spawnBlocks()
    {
        for (int i = 0; i < 4; i++)
        {
            activeBlocks[i] = Instantiate(blockPrefab).GetComponent<Block>();
        }
        activeBlocks[0].transform.position = spawnPos - HorizontalShift;
        activeBlocks[0].coords = spawnCoords - HorizontalCoordShift;
        
        activeBlocks[1].transform.position = spawnPos;
        activeBlocks[1].coords = spawnCoords;

        activeBlocks[2].transform.position = spawnPos + HorizontalShift;
        activeBlocks[2].coords = spawnCoords + HorizontalCoordShift;

        activeBlocks[3].transform.position = spawnPos + (HorizontalShift * 2);
        activeBlocks[3].coords = spawnCoords + (HorizontalCoordShift * 2);
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
            if (activeBlock.coords.y == 0 || occupied[(int)activeBlock.coords.x, (int)activeBlock.coords.y - 1])
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
            if(activeBlock.coords.x == ROWCOUNT - 1 || occupied[(int)activeBlock.coords.x + 1, (int)activeBlock.coords.y])
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
            if(activeBlock.coords.x == 0 || occupied[(int)activeBlock.coords.x - 1, (int)activeBlock.coords.y])
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