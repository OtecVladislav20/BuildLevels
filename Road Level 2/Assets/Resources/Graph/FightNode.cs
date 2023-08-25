using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FightNode : MonoBehaviour
{
    private static readonly Color OCCUPIED_COLOR = Color.red;
    private static readonly Color REACHEABLE_COLOR = Color.yellow;
    private static readonly Color AVAILABLE_COLOR = Color.green;
    
    [field:SerializeField] public List<FightNode> StraightNeighbours = new List<FightNode>();
    [field:SerializeField] public List<FightNode> DiagonalNeighbours = new List<FightNode>();
    [field:SerializeField] public Vector2Int Index { get; set; }
    [field:SerializeField] public NodeType Type { get; private set;}
    public readonly Dictionary<FightNode, BorderEdge> NeighboursEdges = new();
    public readonly HashSet<BorderEdge> AllEdges = new();

    private Renderer nodeRenderer;

    private void Awake()
    {
        nodeRenderer = transform.GetComponentInChildren<Renderer>();
    }

    public void SetNodeOccupied()
    {
        Type = NodeType.Occupied;
    }

    public void SetNodeAvailable()
    {
        //nodeRenderer.material.color = (nodeRenderer.material.color != AVAILABLE_COLOR) ? REACHEABLE_COLOR : AVAILABLE_COLOR;
        foreach (var edge in AllEdges)
        {
            var newStatus = edge.ChangeStatus(this);
            if (newStatus == BorderEdgeStatus.On)
            {
                BorderController.drawedEdges.Add(edge);
                BorderController.reachedEdges.Add(edge);
            }

            if (newStatus == BorderEdgeStatus.Off)
            {
                BorderController.drawedEdges.Remove(edge);
            }
        }
    }

    public void SetNodeReachable()
    {
        nodeRenderer.material.color = AVAILABLE_COLOR;
    }

    public void SetNodeDefault()
    {
        Type = NodeType.Free;
        foreach (var edge in AllEdges)
        {
            edge.Reset();
        }
        //nodeRenderer.enabled = false;
        //nodeRenderer.material.color = Color.white;
    }

    public FightNode SetNodeTarget()
    {
        nodeRenderer.material.color = Color.blue;
        return this;
    }

    public IEnumerable<FightNode> AllNeighbours()
    {
        //Сначала смежные соседи, потом соседи по диагонали
        foreach (var neighbour in StraightNeighbours) 
            yield return neighbour;
        foreach (var neighbour in DiagonalNeighbours)
            yield return neighbour;
    }
}
