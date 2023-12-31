﻿using GF2DarkZoneMapEditor;
using UnityEngine;

public class Demo : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        DrawResult();
    }

    private void DrawResult()
    {
        var boxColliders = GetComponentsInChildren<BoxCollider2D>();
        var edges = RectanglesMerger.GetEdgesAfterMerge(boxColliders);
        foreach (var edge in edges)
        {
            Debug.DrawLine(edge.startPoint, edge.endPoint, Color.red, 1);
        }
    }

}
