using GF2DarkZoneMapEditor;
using UnityEngine;

public class Demo1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        PrintPolygons();
    }

    void PrintPolygons()
    {
        var boxColliders = GetComponentsInChildren<BoxCollider2D>();
        RectanglesMerger.GetPolygonsAfterMerge(boxColliders);
    }

}
