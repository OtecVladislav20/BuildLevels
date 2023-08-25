using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderEdge
{
    private List<BorderVertex> vertexes;
    private bool firstFightNodeDrawed = false;
    private bool secondFightNodeDrawed = false;
    public FightNode FirstFightNode { get; private set; }
    public FightNode SecondFightNode { get; set; }

    public BorderEdgeStatus Status { get; private set; }

    public Transform mark;

    public IReadOnlyList<BorderVertex> Vertexes => vertexes;

    public BorderEdge(FightNode firstNode, FightNode secondNode = null)
    {
        vertexes = new List<BorderVertex>();
        Status = BorderEdgeStatus.Off;

        FirstFightNode = firstNode;
        SecondFightNode = secondNode;

        // mark = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        // mark.localScale = Vector3.one * 0.2f;
    }

    public void AddVertex(BorderVertex vertex)
    {
        if(vertexes.Count < 2)
            vertexes.Add(vertex);
        // if (vertexes.Count == 1)
        // {
        //     mark.position = vertexes[0].Position;
        // }
        // if (vertexes.Count == 2)
        // {
        //     mark.position = (vertexes[0].Position + Vertexes[1].Position) / 2;
        // }
    }

    public BorderEdgeStatus ChangeStatus(FightNode initialNode)
    {
        firstFightNodeDrawed |= initialNode == FirstFightNode;
        secondFightNodeDrawed |= initialNode == SecondFightNode;
        Status = (firstFightNodeDrawed ^ secondFightNodeDrawed) ? BorderEdgeStatus.On : BorderEdgeStatus.Off;
        // if (Status == BorderEdgeStatus.Unreachebled)
        // {
        //     mark.GetComponent<Renderer>().material.color = Color.green;
        //     Status = BorderEdgeStatus.On;
        // }
        // else if (Status == BorderEdgeStatus.On 
        //          && SecondFightNode != null 
        //          && SecondFightNode.Type == NodeType.Free
        //          && FirstFightNode.Type == NodeType.Free)
        // {
        //     mark.GetComponent<Renderer>().material.color = Color.red;
        //     Status = BorderEdgeStatus.Off;
        // }

        return Status;
    }

    public void Reset()
    {
        //mark.GetComponent<Renderer>().material.color = Color.white;
        Status = BorderEdgeStatus.Off;
        firstFightNodeDrawed = false;
        secondFightNodeDrawed = false;
    }
}
