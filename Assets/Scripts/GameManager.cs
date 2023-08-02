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

    [SerializeField]
    private GameObject blockPrefab;         // Block to copy each time blocks are spawned

    [SerializeField]
    private int ROWCOUNT = 10;              // Number of blocks that can fit horizontally 

    [SerializeField]
    private float waitTime = 1f;            // Number of seconds to wait before moving active blocks down a step
    
    private int COLUMNCOUNT = 30;           // Number of blocks to allowed vertically
    
    private GameObject[,] occupied;         // Stores landed blocks and used to determine when blocks should land
                                            // Coordinate (0, 0) corresponds to a block at the bottom left corner
                                            // of the playable area

    private GameObject activeBlock;         
    private Vector2 activeBlockCoords;      // Coordinates in the imaginary grid where (0, 0) corresponds to a block 
                                            // at the bottom left corner of the playable area
    

    private Vector2 bottomLeft, topRight;   // Coordinates of corners of screen in World coordinates
    private float blockWidth;       // and height
    
    private Vector3 spawnPos;
    
    private Vector2 spawnCoords;

    private bool spawnNew = true;

    // Event system to be triggered each time blocks land
    private delegate void OnBlockLand();
    private event OnBlockLand BlockLanded;

    private GameObject[] blocks;

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

        
        occupied = new GameObject[ROWCOUNT, COLUMNCOUNT + 1];
        BlockLanded += BlockLandHandler;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
        if(spawnNew){
            
            activeBlock = Instantiate(blockPrefab);
            activeBlock.transform.position = spawnPos;
            activeBlockCoords = spawnCoords;
            
            spawnNew = false;
            activeCoroutine = moveActive();
            StartCoroutine(activeCoroutine);
        }
         
    }

    IEnumerator moveActive(){
        while(true){
            yield return new WaitForSeconds(waitTime);
            
            activeBlock.transform.position -= new Vector3(0, blockWidth, 0);
            activeBlockCoords -= new Vector2(0, 1);

            // Blocks land when it one reaches the bottom or is right above a landed block
            if (activeBlockCoords.y == 0 || occupied[(int)activeBlockCoords.x, (int)activeBlockCoords.y - 1]){
                occupied[(int)activeBlockCoords.x, (int)activeBlockCoords.y] = activeBlock;
                BlockLanded();
            }
            
        }
    }

    void BlockLandHandler(){
        StopCoroutine(activeCoroutine);
        spawnNew = true;
    }
}
