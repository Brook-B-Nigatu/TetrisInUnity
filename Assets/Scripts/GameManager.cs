using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject blockPrefab;

    [SerializeField]
    private int ROWCOUNT = 10;
    
    private int COLUMNCOUNT = 30;
    
    private GameObject[,] occupied;

    private GameObject activeBlock;
    

    private Vector2 bottomLeft, topRight;   // Coordinates of corners of screen
    private float blockWidth;
    
    private Vector3 spawnPos;

    private bool spawnNew = true;

    private enum Shapes{
        Long,
        Block,
        T,
        S
    };

    private GameObject[] blocks;

    // Start is called before the first frame update
    void Awake()
    {
        
        blockWidth = blockPrefab.GetComponent<SpriteRenderer>().bounds.size.x;

        Camera cam = Camera.main;
        bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
        topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, 0));

        spawnPos = new Vector3(-blockWidth/ 2, bottomLeft.y + (blockWidth * (COLUMNCOUNT + 0.5f)), 0);

        
        occupied = new GameObject[ROWCOUNT, COLUMNCOUNT];
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnNew)
        {
            activeBlock = Instantiate(blockPrefab);
            activeBlock.transform.position = spawnPos;
            spawnNew = false;
            return;
        }

        // activeBlock.transform.position -= new Vector3(0, blockwidth, 0);
    }
}
