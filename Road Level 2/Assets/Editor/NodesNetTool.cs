using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace EditorNamespace
{
    //k - Create, i-NeedDraw, j-Delete, U-Redraw, P-Visible  После работы с графом сделать незначительное изменение в сцене и сохранить ее
    //(Изменения при работе с графом сразу не сохранаяются)
    // 0 - ConnectOrDisconnect
    [EditorTool ("Nodes Net Tool")]
    public class NodesNetTool : EditorTool
    {
        private Texture2D icon;
        private const float SQUARE_SIZE = 0.5f;
        private GameObject[,] nodes;
        private List<Tuple<Vector3, Vector3>> edges = new List<Tuple<Vector3, Vector3>>();
        private bool needDrawEdges = true;

        public override GUIContent toolbarIcon =>
            new GUIContent
            {
                image = icon,
                text = "Nodes Net Tool",
                tooltip = "Создает сеть вершин по двум вершинам"
            };

        public override void OnToolGUI(EditorWindow window)
        {

            if(Event.current.Equals(Event.KeyboardEvent("k")))
            {
                if (Selection.count == 2) 
                {
                    var firstObj = Selection.gameObjects[0];
                    var secondObj = Selection.gameObjects[1];
                    if ( firstObj.GetComponent<FightNode>() != null
                         && secondObj.GetComponent<FightNode>() != null)
                    {
                        if (Math.Abs(firstObj.transform.position.x - secondObj.transform.position.x) < SQUARE_SIZE
                            || Math.Abs(firstObj.transform.position.z - secondObj.transform.position.z) < SQUARE_SIZE)
                            Debug.Log("Слишком маленький участок");
                        else
                        {
                            Debug.Log("Create");
                            CreateNet(firstObj, secondObj);
                        }
                    }
                }
            }

            if (Event.current.Equals(Event.KeyboardEvent("i")))
                needDrawEdges = !needDrawEdges;

            if (Event.current.Equals(Event.KeyboardEvent("j")))
                DeleteNode();

            if (Event.current.Equals(Event.KeyboardEvent("u")))
                ReDrawEdges();
            
            if (Event.current.Equals(Event.KeyboardEvent("m")))
                DrawEdges();

            if (Event.current.Equals(Event.KeyboardEvent("o")))
                ConnectOrDisconnect();

            if (Event.current.Equals(Event.KeyboardEvent("z")))
                ChangeVisibility();
        }

        private void CreateNet(GameObject firstObj, GameObject secondObj)
        {
            var rows = (int)(Math.Abs(firstObj.transform.position.z - secondObj.transform.position.z) / SQUARE_SIZE) + 1;
            var columns = (int)(Math.Abs(firstObj.transform.position.x - secondObj.transform.position.x) / SQUARE_SIZE) + 1;

            // secondObj.transform.position = firstObj.transform.position 
            // + new Vector3( (columns - 1) * innerRadius * Math.Sign (secondObj.transform.position.x - firstObj.transform.position.x),
            // 0 , (rows - 1) * innerRadius * Math.Sign (secondObj.transform.position.z - firstObj.transform.position.z));

            nodes = new GameObject[rows, columns];
            nodes[0, 0] = firstObj;
            nodes[rows - 1, columns - 1] = secondObj;

            firstObj.GetComponent<FightNode>().StraightNeighbours.Clear();
            firstObj.GetComponent<FightNode>().DiagonalNeighbours.Clear();
            firstObj.GetComponent<FightNode>().Index = Vector2Int.zero;
            secondObj.GetComponent<FightNode>().StraightNeighbours.Clear();
            secondObj.GetComponent<FightNode>().DiagonalNeighbours.Clear();
            secondObj.GetComponent<FightNode>().Index = new Vector2Int(rows - 1, columns - 1);

            var firstObjPos = firstObj.transform.position;
            var secondObjPos = secondObj.transform.position;

            for (var i = 0; i < columns; i++)
            {
                for (var j = 0; j < rows; j++)
                {
                    if ((i != 0 && j != rows - 1) || (i != columns - 1 && j != 0))
                    {
                        var newNode = Instantiate(firstObj);
                        newNode.transform.parent = firstObj.transform.parent;
                    
                        newNode.transform.position = firstObjPos 
                                                     + new Vector3(i * SQUARE_SIZE * Math.Sign(secondObjPos.x - firstObjPos.x),
                                                         0, j * SQUARE_SIZE * Math.Sign(secondObjPos.z - firstObjPos.z)); 

                        nodes[j, i] = newNode;
                        nodes[j, i].GetComponent<FightNode>().Index = new Vector2Int(j ,i);
                    }
                }
            }

            MakeNeighbours(nodes, rows, columns);
        }

        private void MakeNeighbours(GameObject[,] nodes, int rows, int columns)
        {
            Debug.Log("Make Neighbours");
            for(var i = 0; i < columns; i++)
            {
                for (var j = 0; j < rows; j++)
                {
                    if (i != 0)
                    {
                        nodes[j, i].GetComponent<FightNode>().StraightNeighbours
                            .Add(nodes[j, i - 1].GetComponent<FightNode>());
                        edges.Add(new Tuple<Vector3, Vector3>(nodes[j, i].transform.position,
                            nodes[j, i - 1].transform.position));
                        if (j != 0)
                        {
                            nodes[j, i].GetComponent<FightNode>().DiagonalNeighbours
                                .Add(nodes[j - 1, i - 1].GetComponent<FightNode>());
                            edges.Add(new Tuple<Vector3, Vector3>(nodes[j, i].transform.position,
                                nodes[j - 1, i - 1].transform.position));
                        }
                        if (j != rows - 1)
                        {
                            nodes[j, i].GetComponent<FightNode>().DiagonalNeighbours
                                .Add(nodes[j + 1, i - 1].GetComponent<FightNode>());
                            edges.Add(new Tuple<Vector3, Vector3>(nodes[j, i].transform.position,
                                nodes[j + 1, i - 1].transform.position));
                        }
                    }
                    if (j != 0)
                    {
                        nodes[j, i].GetComponent<FightNode>().StraightNeighbours
                            .Add(nodes[j - 1, i].GetComponent<FightNode>());
                        edges.Add(new Tuple<Vector3, Vector3>(nodes[j, i].transform.position,
                            nodes[j - 1, i].transform.position));
                        
                        if (i != columns - 1)
                        {
                            nodes[j, i].GetComponent<FightNode>().DiagonalNeighbours
                                .Add(nodes[j - 1, i + 1].GetComponent<FightNode>());
                            edges.Add(new Tuple<Vector3, Vector3>(nodes[j, i].transform.position,
                                nodes[j - 1, i + 1].transform.position));
                        }
                    }
                    if (i != columns - 1)
                    {
                        nodes[j, i].GetComponent<FightNode>().StraightNeighbours.Add(nodes[j, i + 1].GetComponent<FightNode>());
                        edges.Add(new Tuple<Vector3, Vector3>(nodes[j, i].transform.position, nodes[j, i + 1].transform.position));
                    }
                    if (j != rows - 1)
                    {
                        nodes[j, i].GetComponent<FightNode>().StraightNeighbours
                            .Add(nodes[j + 1, i].GetComponent<FightNode>());
                        edges.Add(new Tuple<Vector3, Vector3>(nodes[j, i].transform.position,
                            nodes[j + 1, i].transform.position));
                        
                        if (i != columns - 1)
                        {
                            nodes[j, i].GetComponent<FightNode>().DiagonalNeighbours
                                .Add(nodes[j + 1, i + 1].GetComponent<FightNode>());
                            edges.Add(new Tuple<Vector3, Vector3>(nodes[j, i].transform.position,
                                nodes[j + 1, i + 1].transform.position));
                        }
                    }
                    // if (j % 2 == 0)
                    // {
                    //     if (i != 0)
                    //     {
                    //         if (j != 0)
                    //         {
                    //             nodes[j, i].GetComponent<FightNode>().Neighbours.Add(nodes[j - 1, i - 1].GetComponent<FightNode>());
                    //             Edges.Add(new Tuple<Vector3, Vector3>(nodes[j, i].transform.position, nodes[j - 1, i - 1].transform.position));
                    //         }
                    //         if (j != rows - 1)
                    //         {
                    //             nodes[j, i].GetComponent<FightNode>().Neighbours.Add(nodes[j + 1, i - 1].GetComponent<FightNode>());
                    //             Edges.Add(new Tuple<Vector3, Vector3>(nodes[j, i].transform.position, nodes[j + 1, i - 1].transform.position));
                    //         }
                    //     }
                    // }
                    // else
                    // {
                    //     if (i != columns - 1)
                    //     {
                    //         if (j != 0)
                    //         {
                    //             nodes[j, i].GetComponent<FightNode>().Neighbours.Add(nodes[j - 1, i + 1].GetComponent<FightNode>());
                    //             Edges.Add(new Tuple<Vector3, Vector3>(nodes[j, i].transform.position, nodes[j - 1, i + 1].transform.position));
                    //         }
                    //         if (j != rows - 1)
                    //         {
                    //             nodes[j, i].GetComponent<FightNode>().Neighbours.Add(nodes[j + 1, i + 1].GetComponent<FightNode>());
                    //             Edges.Add(new Tuple<Vector3, Vector3>(nodes[j, i].transform.position, nodes[j + 1, i + 1].transform.position));
                    //         }
                    //     }
                    // }
                }
            }
        }

        private void DeleteNode()
        {
            foreach(var nodeObj in Selection.gameObjects)
            {
                var node = nodeObj.GetComponent<FightNode>();
                if(node == null)
                    node = nodeObj.transform.parent.GetComponent<FightNode>();
                if (node != null)
                {
                    foreach (var neighbour in node.StraightNeighbours)
                    {
                        if(neighbour != null)
                            neighbour.StraightNeighbours.Remove(node);
                    }
                    
                    foreach (var neighbour in node.DiagonalNeighbours)
                    {
                        if(neighbour != null)
                            neighbour.DiagonalNeighbours.Remove(node);
                    }

                    DestroyImmediate(node.transform.gameObject);
                }
            }
            Debug.Log("Delete nodes");
        }

        private void DrawEdges()
        {
            Debug.Log("DRAW: " + edges.Count);
            foreach (var edge in edges)
            { 
                Debug.DrawLine(edge.Item1, edge.Item2);
            }
        }

        private void ReDrawEdges()
        {
            Debug.Log("REDRAW");
            edges.Clear();
            if (nodes.Length == 0)
                return;
            foreach(var nodeObj in nodes)
            {
                if (nodeObj != null)
                {
                    foreach (var neighbour in nodeObj.GetComponent<FightNode>().StraightNeighbours)
                    {
                        var neighbourObjPos = neighbour.GetComponentInParent<Transform>().position;
                        edges.Add(new Tuple<Vector3, Vector3>(neighbourObjPos, nodeObj.transform.position));
                    }
                    
                    foreach (var neighbour in nodeObj.GetComponent<FightNode>().DiagonalNeighbours)
                    {
                        var neighbourObjPos = neighbour.GetComponentInParent<Transform>().position;
                        edges.Add(new Tuple<Vector3, Vector3>(neighbourObjPos, nodeObj.transform.position));
                    }
                }
            }
        }

        private void ConnectOrDisconnect()
        {
            if (Selection.gameObjects.Length == 2)
            {
                var firstNode = Selection.gameObjects[0].transform.parent.GetComponent<FightNode>();
                var secondNode = Selection.gameObjects[1].transform.parent.GetComponent<FightNode>();

                if (firstNode.StraightNeighbours.Contains(secondNode))
                {
                    firstNode.StraightNeighbours.Remove(secondNode);
                    secondNode.StraightNeighbours.Remove(firstNode);
                }
                else if (firstNode.DiagonalNeighbours.Contains(secondNode))
                {
                    firstNode.DiagonalNeighbours.Remove(secondNode);
                    secondNode.DiagonalNeighbours.Remove(firstNode);
                }
                else
                {
                    if (firstNode.Index.x == secondNode.Index.x || firstNode.Index.y == secondNode.Index.y)
                    {
                        firstNode.StraightNeighbours.Add(secondNode);
                        secondNode.StraightNeighbours.Add(firstNode);
                    }
                    else
                    {
                        firstNode.DiagonalNeighbours.Add(secondNode);
                        secondNode.DiagonalNeighbours.Add(firstNode);
                    }
                }
            }
            else
            {
                Debug.Log("Выберите только две вершины");
            }
        }

        private void ChangeVisibility()
        {
            foreach (var node in Selection.gameObjects)
            {
                var meshRenderer = node.GetComponentInChildren<MeshRenderer>();
                if(meshRenderer)
                    meshRenderer.enabled = !meshRenderer.enabled;
            }
        }
    }
}
   