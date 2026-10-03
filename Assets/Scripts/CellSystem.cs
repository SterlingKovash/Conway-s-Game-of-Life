using Unity.Collections;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine.Rendering;
using Unity.VisualScripting;
using System;
using UnityEngine.InputSystem;

public class CellSystem : MonoBehaviour
{
    [SerializeField] private GameObject cell;
    [SerializeField] private float tickInterval = 0.25f;
    private float timer = 0f;
    private NativeArray<byte> cellStates;
    private NativeArray<byte> nextCellStates;
    private SpriteRenderer[] cellRenders;
    GridOutline gridOutline;
    private int width;
    private int gridSize;
    private bool isPaused = false;
    private bool isDrawing = false;
    private bool isDrawingRenders = false;
    [SerializeField] Camera mainCam;
    private Vector3 mousePos;
    private byte paintVal;


    void Awake()
    {
        gridOutline = GetComponent<GridOutline>();
    }
    void Start()
    {
        Debug.Log("Pause - Space\nR - Speed Up\nE - Slow Down\nLeft Click to Draw");

        width = gridOutline.boundsHalfExtent * 2 / gridOutline.cellSize;
        gridSize = (width + 2) * (width + 2);

        cellStates = new NativeArray<byte>(gridSize, Allocator.Persistent);
        nextCellStates = new NativeArray<byte>(gridSize, Allocator.Persistent);

        cellRenders = new SpriteRenderer[gridSize];

        CreateCellStates();

        /*
        CreateBlinker(4, 3);
        CreateCube(10, 10);
        CreateGlider(1, 15, "up right");
        CreateGlider(1, 9);
        CreateGlider(34, 35);
        */
    }


    void Update()
    {
        mousePos = new Vector3(Mathf.FloorToInt(mainCam.ScreenToWorldPoint(Input.mousePosition).x) + gridOutline.boundsHalfExtent, Mathf.FloorToInt(mainCam.ScreenToWorldPoint(Input.mousePosition).y) + gridOutline.boundsHalfExtent, 0);
        
        if (Input.GetKeyDown("space"))
        {
            isPaused = !isPaused;
        } else if (Input.GetKeyDown("r"))
        {
            //tickInterval *= 0.5f;
            tickInterval = Mathf.Clamp(tickInterval * 0.5f, 0.1f, 2f);
        } else if (Input.GetKeyDown("e"))
        {
            //tickInterval *= 1.5f;
            tickInterval = Mathf.Clamp(tickInterval * 1.5f, 0.1f, 2f);
        } else if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("painting");
            isDrawing = true;
            paintVal = (byte)(1 - cellStates[(int)((mousePos.x + 1) * (width + 2) + mousePos.y + 1)]);
            isDrawingRenders = !cellRenders[(int)((mousePos.x + 1) * (width + 2) + mousePos.y + 1)].enabled;
        }

        if (isDrawing && 0 <= mousePos.x && mousePos.x <= gridOutline.boundsHalfExtent * 2 && 0 <= mousePos.y && mousePos.y <= gridOutline.boundsHalfExtent * 2)
        {
            //Debug.Log(mousePos);
            
            int currentCellIdx = (int)((mousePos.x + 1) * (width + 2) + mousePos.y + 1);
            int currentCellVal = cellStates[currentCellIdx];

            cellRenders[currentCellIdx].enabled = isDrawingRenders;
            cellStates[currentCellIdx] = paintVal;
               
        }
        if (Input.GetMouseButtonUp(0))
            {
                isDrawing = false;
            } 

        if (timer >= tickInterval)
        {
            var updateCells = new UpdateCells
            {
                gridState = cellStates,
                nextGridState = nextCellStates,
                boundsHalfExtent = gridOutline.boundsHalfExtent,
                cellSize = gridOutline.cellSize,
                width = width,
                stride = width + 2,
            };
            updateCells.ScheduleParallel(width-1, 1, default).Complete();

            DrawCells();

            cellStates.CopyFrom(nextCellStates);

            timer -= tickInterval;
        }
        else if (!isPaused)
        {
            timer += Time.deltaTime;
        }
    }



    //Could maybe combine with DrawCells()
    void CreateCellStates()
    {
        int stride = width + 2;
        float cellSize = gridOutline.cellSize;
        int b = gridOutline.boundsHalfExtent;
        
        for (int i = 0; i < gridSize; i++)
        {            
            //check for padding 
            if (i % stride == 0 || i <= 12 || stride % i == 1 || gridSize - i == stride)
            {
                cellStates[i] = 0;
            }
            else
            {  
                cellStates[i] = 0;
            }
        }

        int cellCount = 0;
        for (int i = 0; i < width+2; i++)
        {
            for (int j = 0; j < width+2; j++)
            {
                GameObject newCell = Instantiate(cell, new Vector3(i-b-1 + (cellSize/2), j-b-1 + (cellSize/2), 0), Quaternion.identity);
                SpriteRenderer sr = newCell.GetComponent<SpriteRenderer>();
                sr.material.color = Color.yellow;
                cellRenders[cellCount] = sr;
                cellRenders[cellCount].enabled = false;
                cellCount++;
            }
        }
    }


    //Instantiates gameobject cell at Start() and adds component spriterenderer to list
    void DrawCells()
    {
        int stride = width + 2;

        for (int row = 0; row < stride; row++)
        {
            for (int col = 0; col < stride; col++)
            {
                cellRenders[row*stride + col].enabled = (cellStates[row*stride + col] == 1);
            } 
        }
    }
    int ConvertBufferIndex(int x, int y)
    {
        int stride = width + 2;
        return (x + 1) * stride + y + 1;
    }

    //Input handler for mouse
    void MouseDraw()
    {
        Vector3 mousePos = new Vector3(Mathf.FloorToInt(mainCam.ScreenToWorldPoint(Input.mousePosition).x), Mathf.FloorToInt(mainCam.ScreenToWorldPoint(Input.mousePosition).y), 0);
        int currentCellVal = cellStates[(int)((mousePos.x + 1) * (width + 2) + mousePos.y + 1)];
        int paintVal = 1 - currentCellVal;

    }


    //PATTERN CREATION FUNCTIONS
    void CreateBlinker(int x, int y)
    {
        cellStates[ConvertBufferIndex(x-1, y)] = 1;
        cellStates[ConvertBufferIndex(x, y)] = 1;
        cellStates[ConvertBufferIndex(x+1, y)] = 1;
    }
    void CreateCube(int x, int y)
    {
        cellStates[ConvertBufferIndex(x, y)] = 1;
        cellStates[ConvertBufferIndex(x+1, y)] = 1;
        cellStates[ConvertBufferIndex(x+1, y+1)] = 1;
        cellStates[ConvertBufferIndex(x, y+1)] = 1;
    }
    void CreateGlider(int x, int y)
    {
        cellStates[ConvertBufferIndex(x, y)] = 1;
        cellStates[ConvertBufferIndex(x+1, y)] = 1;
        cellStates[ConvertBufferIndex(x+2, y)] = 1;
        cellStates[ConvertBufferIndex(x+2, y+1)] = 1;
        cellStates[ConvertBufferIndex(x+1, y+2)] = 1;
    }
    void CreateGlider(int x, int y, string dir)
    {
        if (dir == "right")
        {
            cellStates[ConvertBufferIndex(x, y)] = 1;
            cellStates[ConvertBufferIndex(x+1, y)] = 1;
            cellStates[ConvertBufferIndex(x+2, y)] = 1;
            cellStates[ConvertBufferIndex(x+2, y+1)] = 1;
            cellStates[ConvertBufferIndex(x+1, y+2)] = 1;
        }
        else if (dir == "left")
        {
            cellStates[ConvertBufferIndex(x, y)] = 1;
            cellStates[ConvertBufferIndex(x-1, y)] = 1;
            cellStates[ConvertBufferIndex(x-2, y)] = 1;
            cellStates[ConvertBufferIndex(x-2, y+1)] = 1;
            cellStates[ConvertBufferIndex(x-1, y+2)] = 1;
        }
        else if (dir == "up left")
        {
            cellStates[ConvertBufferIndex(x, y)] = 1;
            cellStates[ConvertBufferIndex(x-1, y)] = 1;
            cellStates[ConvertBufferIndex(x-2, y)] = 1;
            cellStates[ConvertBufferIndex(x-2, y-1)] = 1;
            cellStates[ConvertBufferIndex(x-1, y-2)] = 1;
        }
        else
        {
            cellStates[ConvertBufferIndex(x, y)] = 1;
            cellStates[ConvertBufferIndex(x+1, y)] = 1;
            cellStates[ConvertBufferIndex(x+2, y)] = 1;
            cellStates[ConvertBufferIndex(x+2, y-1)] = 1;
            cellStates[ConvertBufferIndex(x+1, y-2)] = 1;
        }
        
    }


    void OnDestroy()
    {
        if(cellStates.IsCreated) cellStates.Dispose();
        if(nextCellStates.IsCreated) nextCellStates.Dispose();
    }


    [BurstCompile]

    private struct UpdateCells : IJobFor
    {
        //FORMULA -- Row * Stride + Column
        [ReadOnly] public NativeArray<byte> gridState;
        [WriteOnly][NativeDisableParallelForRestriction] public NativeArray<byte> nextGridState;
        public int boundsHalfExtent;
        public int cellSize;
        public int width;
        public int stride;
        

        public void Execute(int row)
        {
            //add 1 to row to account for padding
            row += 1;
            for (int col = 1; col < width+1; col++)
            {
                //add neighbors starting from top left cell
                //I think the line below acts for unpadded grid, when the grid is padded
                //int neighborSum = gridState[(i+2)*stride+col+1] + gridState[(i+2)*stride+col+2] + gridState[(i+2)*stride+col+3] + gridState[(i+1)*stride+col+1] + gridState[(i+1)*stride+col+3] + gridState[i*stride+col+1] + gridState[i*stride+col+2] + gridState[i*stride+col+3];
                int neighborSum = gridState[(row+1)*stride+col-1] + gridState[(row+1)*stride+col] + gridState[(row+1)*stride+col+1] + gridState[row*stride+col-1] + gridState[row*stride+col+1] + gridState[(row-1)*stride+col-1] + gridState[(row-1)*stride+col] + gridState[(row-1)*stride+col+1];

                if (neighborSum == 3 || neighborSum + gridState[row*stride + col] == 3)
                {
                    nextGridState[row*stride + col] = 1;
                } else
                {
                    nextGridState[row*stride + col] = 0;
                }
            }
        }
    }
}
