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
    //ʵ�ʾ���
    public int G
    {
        get { return g; }
        set { g = value; f = g + h; }
    }
    //�������
    public int H
    { 
        get { return h; } 
        set { h = value; f = g + h; } 
    }
    //������¾���
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
    static int FACTOR = 10;//ˮƽ��ֱ���ڸ��ӵľ���
    static int FACTOR_DIAGONAL = 14;//�Խ������ڸ��ӵľ���
    PriorityQueue<Node> open;
    List<Node> close;
    
}
