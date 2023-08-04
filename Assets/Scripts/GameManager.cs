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

    private Block activeBlock;         
    
    

    private Vector2 bottomLeft, topRight;   // Coordinates of corners of screen in World coordinates
    private float blockWidth;       // and height
    
    private Vector3 spawnPos, HorizontalShift, VerticalShift;

    private Vector2 spawnCoords, HorizontalCoordShift = new Vector2(1, 0), VerticalCoordShift = new Vector2(0, 1);

    private bool spawnNew = true;

    // Event system to be triggered each time blocks land
    private delegate void OnBlockLand();
    private event OnBlockLand BlockLanded;

    private delegate void OnBlockMove(ref Block block);
    private event OnBlockMove BlockMove;    

    private Block[] blocks;

    private bool gamePaused = false;
   
    IEnumerator activeCoroutine;    // The active coroutine that moves blocks downsssss

    // Start is called before the first frame update
    void Awake()
    {   
        // Determine key constants for the rest of the game
        
        blockWidth = blockPrefab.GetComponent<SpriteRenderer>().bounds.size.x;

        Camera cam = Camera.main;
        bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        spawnPos = new Vector3(-blockWidth/ 2 * (1 - ROWCOUNT % 2), bottomLeft.y + (blockWidth * (COLUMNCOUNT + 0.5f + 1)), 0);
        spawnCoords = new Vector2(ROWCOUNT / 2 - 1 + (ROWCOUNT % 2), COLUMNCOUNT + 1);

        HorizontalShift = new Vector3(blockWidth, 0, 0);
        VerticalShift = new Vector3(0, blockWidth, 0);

        occupied = new Block[ROWCOUNT, COLUMNCOUNT + 2];
        
    }

    void OnEnable()
    {
        // Subscribe to events

        BlockLanded += BlockLandHandler;

        BlockMove += checkLand;

        PauseManager.TogglePause += OnTogglePause;
    }

    void OnDisable()
    {
        // Unsubscribe to events

        BlockLanded -= BlockLandHandler;

        BlockMove -= checkLand;

        PauseManager.TogglePause -= OnTogglePause;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gamePaused)
        {
            return;
        }
       
        if(spawnNew){
            
            activeBlock = Instantiate(blockPrefab).GetComponent<Block>();
            activeBlock.transform.position = spawnPos;
            activeBlock.coords = spawnCoords;
            
            spawnNew = false;
            activeCoroutine = moveActive();
            StartCoroutine(activeCoroutine);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) && canMoveLeft(ref activeBlock))
        {
            moveBlock(ref activeBlock, Directions.Left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) && canMoveRight(ref activeBlock))
        {
            moveBlock(ref activeBlock, Directions.Right);
        }
    }

    IEnumerator moveActive()
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);

            moveBlock(ref activeBlock, Directions.Down);

        }
    }

    void moveBlock(ref Block block, Directions direction)
    {
        switch (direction)
        {
            case Directions.Left:
                
                //activeBlock.coords -= HorizontalCoordShift;
                //activeBlock.transform.position -= HorizontalShift;

                block.move(-1 * HorizontalShift, -1 * HorizontalCoordShift);
                
                break;
           
            case Directions.Right:

                //activeBlock.coords += HorizontalCoordShift;
                //activeBlock.transform.position += HorizontalShift;
                
                block.move(HorizontalShift, HorizontalCoordShift);

                break;
            case Directions.Down:

                //activeBlock.transform.position -= VerticalShift;
                //activeBlock.coords -= VerticalCoordShift;

                block.move(-1 * VerticalShift, -1 * VerticalCoordShift);

                break;
        }

        BlockMove(ref block);
    }

    void BlockLandHandler(){
        StopCoroutine(activeCoroutine);
        spawnNew = true;
    }

    void checkLand(ref Block block)
    {
        // Blocks land when it one reaches the bottom or is right above a landed block
        if (block.coords.y == 0 || occupied[(int)block.coords.x, (int)block.coords.y - 1])
        {
            occupied[(int)block.coords.x, (int)block.coords.y] = block;
            BlockLanded();
        }
    }
    bool canMoveRight(ref Block block)
    {
        return block.coords.x < ROWCOUNT - 1
            && !occupied[(int)block.coords.x + 1, (int)block.coords.y];
    }

    bool canMoveLeft(ref Block block)
    {
        return block.coords.x > 0
            && !occupied[(int)block.coords.x - 1, (int)block.coords.y];
    }

    void OnTogglePause()
    {
        gamePaused ^= true;
    }

}