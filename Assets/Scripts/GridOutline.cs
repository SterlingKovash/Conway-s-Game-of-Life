using System;
using Unity.VisualScripting;
using UnityEngine;

public class GridOutline : MonoBehaviour
{
    [SerializeField] public int cellSize = 1;
    [SerializeField] public int boundsHalfExtent = 5;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Color gridColor = Color.white;
    private Material lineMaterial;
    LineRenderer boundsLine;
    void Start()
    {
        lineMaterial = new Material(Shader.Find("Sprites/Default"));
        boundsLine = gameObject.AddComponent<LineRenderer>();
        boundsLine.positionCount = 5;
        boundsLine.loop = false;
        boundsLine.useWorldSpace = true;
        boundsLine.startWidth = lineWidth;
        boundsLine.endWidth = lineWidth;
        boundsLine.material = lineMaterial;
        boundsLine.startColor = gridColor;
        boundsLine.endColor = gridColor;

        float b = boundsHalfExtent;
        boundsLine.SetPositions(new Vector3[]
        {
            new Vector3(-b, -b, 0),
            new Vector3(-b, b, 0),
            new Vector3(b, b, 0),
            new Vector3(b, -b, 0),
            new Vector3(-b - lineWidth/2, -b, 0), //close the loop
        });

        CreateGrid();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBoundsLine();       
    }

    void UpdateBoundsLine()
    {
        boundsLine.startWidth = lineWidth;
        boundsLine.endWidth = lineWidth;
        boundsLine.startColor = gridColor;
        boundsLine.endColor = gridColor;
    }

    void CreateGrid()
    {
        float b = boundsHalfExtent;
        
        
        //horizontal lines
        for (int i = 0; i < 2*b/cellSize; i++)
        {
            CreateLine(new Vector3(-b, i*cellSize - b, 0), new Vector3(b, i*cellSize - b, 0));
        }
        //vertical lines
        for (int i = 0; i < 2*b/cellSize; i++)
        {
            CreateLine(new Vector3(i*cellSize - b, -b, 0), new Vector3(i*cellSize - b, b, 0));
        }

    }

    void CreateLine(Vector3 a, Vector3 b)
    {
        var go = new GameObject("GridLine");
        go.transform.SetParent(transform, false); // false = keep local coords

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.material = lineMaterial;
        lr.startWidth = .07f;
        lr.endWidth = .07f;
        lr.startColor = Color.white;
        lr.endColor = Color.white;
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
    }
}
