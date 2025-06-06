using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Node
{
    public Node parent;
    public Vector3 position;
    public GameObject gameObject;
    
    private int g;
    private int h;
    private int f;
    //实际距离
    public int G
    {
        get { return g; }
        set { g = value; f = g + h; }
    }
    //估算距离
    public int H
    { 
        get { return h; } 
        set { h = value; f = g + h; } 
    }
    //计算大致距离
    public int F => f;
    public Node(Vector3 pos, Node parent, int g, int h)
    {
        position = pos;
        this.parent = parent;
        this.g = g;
        this.h = h;
        f = this.g +this.h;
    }

}
public class Map
{

}

public class AStar : MonoBehaviour
{
    static int FACTOR = 10;//水平竖直相邻格子的距离
    static int FACTOR_DIAGONAL = 14;//对角线相邻格子的距离
    PriorityQueue<Node> open;
    List<Node> close;
    
}
