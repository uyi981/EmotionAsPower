using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class APathFinding
{
    HashSet<Vector2Int> closed = new HashSet<Vector2Int>();
    List<ANode> openList = new List<ANode>();
    ANode startNode;
    ANode endNode;
    
    public List<Vector2Int> GetPathResult(Vector2Int start, Vector2Int end, float[,] heightMap,int size)
    {

        closed = new HashSet<Vector2Int>();
        openList = new List<ANode>();
        startNode=null;
        endNode= null;
        if (CheckIsObstacle(start, heightMap) || CheckIsObstacle(end, heightMap))
        {
            //Debug.Log("Start or End is Obstacle");
            //Debug.Log("Start: "+ heightMap[start.x,start.y]);
            //Debug.Log("End: " + heightMap[end.x,end.y]);
            return null;
        }

        startNode = new ANode(null, start, Vector2Int.Distance(start, end), 0);
        endNode = new ANode(null, end, 0, Vector2Int.Distance(end, start));
        ANode currentNode = startNode;
        AddNeighboursToOpenList(GetNeighbours(currentNode), currentNode, heightMap);
        int checkWhile = 0;
        while (openList.Count > 0)
        {
            if(checkWhile<5)
            {
                //Debug.Log(currentNode.position);
                checkWhile++;
            }         
            currentNode = GetBestANode(openList);
            AddNeighboursToOpenList(GetNeighbours(currentNode), currentNode, heightMap);
            openList.Remove(currentNode);
            closed.Add(currentNode.position);
            if (currentNode.position == endNode.position)
            {
                //Debug.Log("Path Found!");
                return RevervePath(currentNode,heightMap,size);
            }
        }
        return null;
    }
    public ANode FindNode(Vector2Int postition)
    {
        return openList.Find(x => x.position == postition);
    }
    public ANode GetBestANode(List<ANode> openList)
    {
        return openList.OrderBy(x => x.fCost).First();
    }
    public List<Vector2Int> RevervePath(ANode endNode, float[,] heightMap,int size)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        while (endNode != null)
        {
            result.Add(endNode.position);
            if(endNode.parent==null)
            {
                break;
            }       
            endNode = endNode.parent;
        }
        return result;
    }
    public bool CheckIsObstacle(Vector2Int position, float[,] heightMap)
    {

      
        try
        {
            float value = heightMap[position.x, position.y];
            if (value == 1)
            {
                return true;
            }
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.LogError("Index out of range: " + e.Message+position);
            throw;
        }
        return false;
    }
    public void AddNeighboursToOpenList(List<Vector2Int> neighbours, ANode node, float[,] heightMap)
    {
        foreach (var neighbour in neighbours)
        {
            ANode nodeExist = FindNode(neighbour);
            if (CheckIsObstacle(neighbour, heightMap))
            {
                continue;
            }
            if (nodeExist != null)
            {
                CheckCost(node, nodeExist);
                continue;
            }
            else
            {
                if (!closed.Contains(neighbour))
                    openList.Add(new ANode(node, neighbour, Vector2Int.Distance(neighbour, endNode.position),1+ node.gCost));

            }
        }
    }
    public void CheckCost(ANode node, ANode neighbour)
    {
        float newGCost = node.gCost + 1;
        bool isBetter = newGCost < neighbour.gCost;
        if (isBetter)
        {
            neighbour.gCost = newGCost;
            neighbour.parent = node;
        }
    }
    public List<Vector2Int> GetNeighbours(ANode node)
    {
        List<Vector2Int> neighbours = new List<Vector2Int>();
        Vector2Int[] directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };
        foreach (var direction in directions)
        {
            Vector2Int neighbourPos = node.position + direction;
            if (neighbourPos.x < 0 || neighbourPos.y < 0)
            {
                continue; // Skip out of bounds positions
            }
            neighbours.Add(neighbourPos);
        }
        return neighbours;
    }
}
public class ANode
{
    public float hCost;
    public float gCost;
    public ANode parent;
    public float fCost { get { return hCost + gCost; } }
    public Vector2Int position;
    public ANode(ANode parent, Vector2Int position, float hCost, float gCost)
    {
        this.position = position;
        this.hCost = hCost;
        this.gCost = gCost;
        this.parent = parent;
    }

}