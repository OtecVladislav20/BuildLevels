using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

using Vector3 = UnityEngine.Vector3;

public class BorderController : MonoBehaviour
{
    private static readonly float BORDER_SMOOTH = 0.2f;
    
    public static HashSet<BorderEdge> drawedEdges;
    public static HashSet<BorderEdge> reachedEdges;

    private static GameObject availableRendererPrefab;
    private static GameObject obstacleRendererPrefab;
    private static Dictionary<LineRenderer, Vector3[]> contours;

    private static int X_SIGN = 0;
    private static int Z_SIGN = 0;
    private static float SQUARE_SIZE = 0;
    
    [SerializeField] private GameObject availableRenderer;
    [SerializeField] private GameObject obstacleRenderer;

    private void Awake()
    { 
        availableRendererPrefab = availableRenderer;
        obstacleRendererPrefab = obstacleRenderer;
        drawedEdges = new HashSet<BorderEdge>();
        reachedEdges = new HashSet<BorderEdge>();
        contours = new Dictionary<LineRenderer, Vector3[]>();
    }

    public void MakeBorderStructure(FightNode[,] graph, float squareSize)
    {
        var rows = graph.GetLength(0);
        var columns = graph.GetLength(1);

        if (X_SIGN == 0 || Z_SIGN == 0)
        {
            var directions = GetDirections(rows, columns, graph);
            X_SIGN = directions.Item1;
            Z_SIGN = directions.Item2;
        }

        if (SQUARE_SIZE == 0)
            SQUARE_SIZE = squareSize;

        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < columns; j++)
            {
                var currentNode = graph[i, j];
                if (currentNode == null)
                    continue;
                
                var currentPos = currentNode.transform.position;


                //Правый нижний угол
                var rightEdge = new BorderEdge(currentNode);
                currentNode.AllEdges.Add(rightEdge);
                if (j < columns - 1 && graph[i, j + 1] != null)
                {
                    var rightNode = graph[i, j + 1];
                    rightEdge.SecondFightNode = rightNode;
                    currentNode.NeighboursEdges[rightNode] = rightEdge;
                    rightNode.NeighboursEdges[currentNode] = rightEdge;
                }
                
                var bottomEdge = new BorderEdge(currentNode);
                currentNode.AllEdges.Add(bottomEdge);
                if (i < rows - 1 && graph[i + 1, j] != null)
                {
                    var bottomNode = graph[i + 1, j];
                    bottomEdge.SecondFightNode = bottomNode;
                    currentNode.NeighboursEdges[bottomNode] = bottomEdge;
                    bottomNode.NeighboursEdges[currentNode] = bottomEdge;
                }
                
                var bottomRightV = new BorderVertex(currentPos + 
                                                    new Vector3(.5f * squareSize * X_SIGN, 0,
                                                        .5f * squareSize * Z_SIGN));
                rightEdge.AddVertex(bottomRightV);
                bottomEdge.AddVertex(bottomRightV);
                bottomRightV.Edges.Add(rightEdge);
                bottomRightV.Edges.Add(bottomEdge);

                //Нахождение левой верхней вершины
                BorderVertex leftUpV = null;
                var possibleLeftUpVPos = currentPos +
                                         new Vector3(.5f * squareSize * -X_SIGN, 0,
                                             .5f * squareSize * -Z_SIGN);
                if (i != 0 && j != 0 && graph[i - 1, j - 1] != null)
                {
                    var leftUpNode = graph[i - 1, j - 1];
                    leftUpV = FindNearestVertexByNode(leftUpNode, possibleLeftUpVPos);
                }
                else if (i != 0 && graph[i - 1, j] != null)
                {
                    var upNode = graph[i - 1, j];
                    leftUpV = FindNearestVertexByNode(upNode, possibleLeftUpVPos);
                }
                else if (j != 0 && graph[i, j - 1] != null)
                {
                    var leftNode = graph[i, j - 1];
                    leftUpV = FindNearestVertexByNode(leftNode, possibleLeftUpVPos);
                }
                else
                {
                    leftUpV = new BorderVertex(possibleLeftUpVPos);
                }
                
                //Правый верхний угол
                if(i != 0 && graph[i - 1, j] != null)
                {
                    var upNode = graph[i - 1, j];
                    //currentNode.NeighboursEdges[upNode] = upNode.NeighboursEdges[currentNode];
                    var upEdge = currentNode.NeighboursEdges[upNode];
                    var rightUpV = FindNearestVertex(upEdge, bottomRightV.Position);
                    
                    currentNode.AllEdges.Add(upEdge);
                    rightEdge.AddVertex(rightUpV);
                    rightUpV.Edges.Add(rightEdge);
                }
                else
                {
                    var upEdge = new BorderEdge(currentNode);
                    if (i != 0 && j < columns - 1 && graph[i - 1, j + 1] != null)
                    {
                        var rightUpNode = graph[i - 1, j + 1];
                        var possibleVPos = currentPos + 
                                           new Vector3(.5f * squareSize * X_SIGN, 0,
                                               .5f * squareSize * -Z_SIGN);
                        var rightUpV = FindNearestVertexByNode(rightUpNode, possibleVPos);
                        
                        upEdge.AddVertex(leftUpV);
                        upEdge.AddVertex(rightUpV);
                        rightEdge.AddVertex(rightUpV);
                        
                        leftUpV.Edges.Add(upEdge);
                        rightUpV.Edges.Add(upEdge);
                        rightUpV.Edges.Add(rightEdge);
                    }
                    else
                    {
                        var rightUpV = new BorderVertex(currentPos + 
                                                        new Vector3(.5f * squareSize * X_SIGN, 0,
                                                            .5f * squareSize * -Z_SIGN));
                        
                        upEdge.AddVertex(leftUpV);
                        upEdge.AddVertex(rightUpV);
                        rightEdge.AddVertex(rightUpV);
                        
                        leftUpV.Edges.Add(upEdge);
                        rightUpV.Edges.Add(upEdge);
                        rightUpV.Edges.Add(rightEdge);
                    }
                    currentNode.AllEdges.Add(upEdge);
                }

                //Левый нижний угол
                if (j != 0 && graph[i, j - 1] != null)
                {
                    var leftNode = graph[i, j - 1];
                    //currentNode.NeighboursEdges[leftNode] = leftNode.NeighboursEdges[currentNode];
                    var leftEdge = currentNode.NeighboursEdges[leftNode];
                    var leftBottomV = FindNearestVertex(leftEdge, bottomRightV.Position);
                    
                    currentNode.AllEdges.Add(leftEdge);
                    bottomEdge.AddVertex(leftBottomV);
                    leftBottomV.Edges.Add(bottomEdge);
                }
                else
                {
                    var leftEdge = new BorderEdge(currentNode);
                    var leftBottomV = new BorderVertex(currentPos + 
                                                       new Vector3(.5f * squareSize * -X_SIGN, 0,
                                                           .5f * squareSize * Z_SIGN));
                    
                    leftEdge.AddVertex(leftUpV);
                    bottomEdge.AddVertex(leftBottomV);
                    leftEdge.AddVertex(leftBottomV);
                    
                    leftUpV.Edges.Add(leftEdge);
                    leftBottomV.Edges.Add(bottomEdge);
                    leftBottomV.Edges.Add(leftEdge);
                    
                    currentNode.AllEdges.Add(leftEdge);
                }
            }
        }
    }

    public static void DrawBorders()
    {
        foreach (var item in contours)
        {
            var renderer = item.Key;
            var points = item.Value;
            renderer.positionCount = points.Length; 
            renderer.SetPositions(points);
        }
    }

    public static void HideBorders()
    {
        foreach (var renderer in contours.Keys)
        {
            renderer.positionCount = 0;
        }
    }

    public static void CalculateAvailableBorder()
    {
        while (drawedEdges.Count > 0)
        {
            var segment = drawedEdges.First();
            var renderer = Instantiate(availableRendererPrefab).GetComponent<LineRenderer>();
            RegistrateContour(GetContour(segment), renderer);
        }
    }

    public static void CalculateObstacleBorder(NodesNavAgent agent)
    {
        var borderPoints = new List<Vector3>();
        var rootPos = agent.RootNode.transform.position +
                      new Vector3(-X_SIGN, 0, -Z_SIGN) * (SQUARE_SIZE * .45f);
        var xOffset = new Vector3(X_SIGN, 0, 0) * (SQUARE_SIZE * agent.Size.x * .9f);
        var zOffset = new Vector3(0, 0, Z_SIGN) * (SQUARE_SIZE * agent.Size.y * .9f);
        
        //Порядок важен
        borderPoints.Add(rootPos);
        borderPoints.Add(rootPos + xOffset);
        borderPoints.Add(rootPos + xOffset + zOffset);
        borderPoints.Add(rootPos + zOffset);
        
        var renderer = Instantiate(obstacleRendererPrefab).GetComponent<LineRenderer>();
        RegistrateContour(borderPoints, renderer);
    }

    public static void EraseBorder()
    {
        foreach (var edge in reachedEdges)
        {
            edge.Reset();
        }
       
        reachedEdges.Clear();
        drawedEdges.Clear();
        foreach(var renderer in contours.Keys)
            Destroy(renderer.gameObject);
        contours.Clear();
    }

    private static List<Vector3> GetContour(BorderEdge startSegment)
    {
        var contour = new List<Vector3>();

        BorderVertex previousVertex = null;
        var currentSegment = startSegment;
        while (currentSegment != null)
        {
            var newV = currentSegment.Vertexes
                .Where(v => v != previousVertex)
                .First();
            
            previousVertex = newV;
            contour.Add(newV.Position);
            drawedEdges.Remove(currentSegment);
            currentSegment = newV.Edges
                .Where(e => drawedEdges.Contains(e))
                .FirstOrDefault();
        }

        return contour;
    }

    private static void RegistrateContour(List<Vector3> contour, LineRenderer contourRenderer)
    {

        var smoothedContour = new List<Vector3>();
        for (var i = 1; i <= contour.Count; i++)
        {
            var currentDot = contour[i % contour.Count];
            var previousDot = contour[(i - 1) % contour.Count];
            var nextDot = contour[(i + 1) % contour.Count];
            smoothedContour.Add(currentDot + (previousDot - currentDot) * (BORDER_SMOOTH * SQUARE_SIZE));
            smoothedContour.Add(currentDot + (nextDot - currentDot) * (BORDER_SMOOTH * SQUARE_SIZE));
        }

        contours[contourRenderer] = smoothedContour.ToArray();
        //contourRenderer.positionCount = smoothedContour.Count;
        //contourRenderer.SetPositions(smoothedContour.ToArray());
    }

    private Tuple<int, int> GetDirections(int rows, int columns, FightNode[,] graph)
    { FightNode firstNotNullNode = null;
        FightNode secondNotNullNode = null;
        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < columns; j++)
            {
                var node = graph[i, j];
                if (node != null)
                {
                    if (firstNotNullNode == null)
                    {
                        firstNotNullNode = node;
                        break;
                    }
                    
                    if(firstNotNullNode.Index.x < node.Index.x && firstNotNullNode.Index.y < node.Index.y)
                    {
                        secondNotNullNode = node;
                        break;
                    }
                }
            }
            
            if(secondNotNullNode != null)
                break;
        }

        var xSign = Math.Sign(secondNotNullNode.transform.position.x - firstNotNullNode.transform.position.x);
        var zSign = Math.Sign(secondNotNullNode.transform.position.z - firstNotNullNode.transform.position.z);

        return new Tuple<int, int>(xSign, zSign);
    }

    private BorderVertex FindNearestVertex(BorderEdge edge, Vector3 point)
    {
        BorderVertex nearestV = null;
        var minDistance = float.MaxValue;
        foreach (var vertex in edge.Vertexes)
        {
            var distance = Vector3.Distance(point, vertex.Position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestV = vertex;
            }
        }

        return nearestV;
    }

    private BorderVertex FindNearestVertexByNode(FightNode node, Vector3 point)
    {
        BorderVertex nearestV = null;
        var minDistance = float.MaxValue;   
        foreach (var edge in node.NeighboursEdges.Values)
        {
            foreach (var vertex in edge.Vertexes)
            {
                var distance = Vector3.Distance(point, vertex.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestV = vertex;
                }
            }
        }

        return nearestV;
    }
}
