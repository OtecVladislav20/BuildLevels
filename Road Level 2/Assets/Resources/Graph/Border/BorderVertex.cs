using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderVertex
{
    public List<BorderEdge> Edges { get; private set;}
    public readonly Vector3 Position;

    public BorderVertex(Vector3 pos, List<BorderEdge> edges = null)
    {
        Edges = edges ?? new List<BorderEdge>();
        Position = pos;

        // var t = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // t.transform.position = pos;
        // t.transform.localScale = Vector3.one * 0.1f;
    }
}
