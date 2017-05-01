﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Representation in world space
/// </summary>
[System.Serializable]
public class Node : IHeapItem<Node>
{
    public int gridX; // Positions on the x y grid
    public int gridY;

    public Vector3 WorldPosition; // Where this node is in world space

    public enum NodeState { Walkable, UnWalkable };
    public NodeState State;
     
    public UnitScript myOccupant;
    public GameObject Visualization;

    //An enum containing the actor colors that could "win" the node
    public enum NodeAffector { None, Black, Yellow, Blue, White}
    public NodeAffector nodeWinner = NodeAffector.None; //default to none, changed when the influence map is affected
    public int affectorStrength = 0; //default

    // For A*
    public int gCost;
    public int hCost;
    public Node parent;
    int heapIndex; // Where this node is on the heap

    public int HeapIndex
    {
        get
        { return heapIndex; }

        set
        { heapIndex = value; }
    }

    public bool Occupied
    {
        get { return myOccupant != null; }
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public Node(int _gridX, int _gridY)
    {
        gridX = _gridX;
        gridY = _gridY;
    }

    public int CompareTo(Node _otherNode)
    {
        int compare = fCost.CompareTo(_otherNode.fCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(_otherNode.hCost);
        }
        return -compare;
    }
}
