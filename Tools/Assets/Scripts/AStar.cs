using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum TileNodeState : int
{
    Available = 0,
    OccupyWalkable = 1,
    Occupy = 2,
}
public class Node : IComparable<Node>
{
    public Node parent;
    public Vector2Int coordinate;
 
    public TileNodeState state;
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
    public Node(Vector2Int pos, Node parent, int g, int h,TileNodeState state = TileNodeState.Available)
    {
        coordinate = pos;
        this.parent = parent;
        this.g = g;
        this.h = h;
        f = this.g +this.h;
        this.state = state; // ȷ��״̬����ֵ
    }
    // һ�����򵥵Ĺ��캯�����ڳ��δ�����ͼʱ������
    public Node(Vector2Int pos, TileNodeState state = TileNodeState.Available)
    {
        this.coordinate = pos;
        this.state = state;
        this.parent = null;
        this.g = 0;
        this.h = 0;
        this.f = 0;
    }
    /// <summary>
    /// ʵ�ֱȽϷ����������ȶ���֪���������
    /// </summary>
    /// <param name="other">Ҫ�Ƚϵ���һ���ڵ�</param>
    /// <returns>
    ///   < 0: ��ǰʵ������ other ֮ǰ (���ȼ�����)
    ///   = 0: ˳����ͬ
    ///   > 0: ��ǰʵ������ other ֮�� (���ȼ�����)
    /// </returns>
    public int CompareTo(Node other)
    {
        // ���ȱȽ� F ֵ��F ֵС�����ȼ��ߡ�
        int compare = F.CompareTo(other.F);

        // ��� F ֵ��ͬ����Ƚ� H ֵ����Ϊ��ʤ�֣���H ֵС�ĸ��ӽ��յ㣬���ȴ���
        if (compare == 0)
        {
            compare = H.CompareTo(other.H);
        }
        return compare;
    }

}
/// <summary>
/// ��������ͼ
/// </summary>
[System.Serializable]
public class NodeMap
{
    public Node[,] nodeMap;
    public int row;
    public int column;

    public Node this[int x, int y]
    {
        get
        {
            if (x < 0 || y < 0 || x >= nodeMap.GetLength(0) || y >= nodeMap.GetLength(1))
                return null;

            return nodeMap[x, y];
        }
        set
        {
            if (x < 0 || y < 0 || x >= nodeMap.GetLength(0) || y >= nodeMap.GetLength(1))
                return;

            nodeMap[x, y] = value;
        }
    }

    public Node this[Vector2Int coordinate]
    {
        get
        {
            if (coordinate.x < 0 || coordinate.y < 0
                || coordinate.x >= nodeMap.GetLength(0) || coordinate.y >= nodeMap.GetLength(1))
                return null;

            return nodeMap[coordinate.x, coordinate.y];
        }
        set
        {

            if (coordinate.x < 0 || coordinate.y < 0
                || coordinate.x >= nodeMap.GetLength(0) || coordinate.y >= nodeMap.GetLength(1))
                return;

            nodeMap[coordinate.x, coordinate.y] = value;
        }
    }

    public NodeMap(Node[,] nodeMap)
    {
        this.nodeMap = nodeMap;
        row = nodeMap.GetLength(0);
        column = nodeMap.GetLength(1);
    }
    // ���ߴ紴����ͼ�Ĺ��캯��
    public NodeMap(int width, int height)
    {
        row = width;
        column = height;
        nodeMap = new Node[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                nodeMap[i, j] = new Node(new Vector2Int(i, j));
            }
        }
    }
    /// <summary>
    /// ���õ�ͼ�����нڵ��Ѱ·״̬ (parent, G, H, F)��
    /// </summary>
    public void ResetNodeStates()
    {
        for (int x = 0; x < row; x++)
        {
            for (int y = 0; y < column; y++)
            {
                nodeMap[x, y].parent = null;
                nodeMap[x, y].G = 0;
                nodeMap[x, y].H = 0;
                // F ֵ���Զ��� G �� H ���£������ֶ�����
            }
        }
    }
}

public class AStar : MonoBehaviour
{
    static int FACTOR = 10;//ˮƽ��ֱ���ڸ��ӵľ���
    static int FACTOR_DIAGONAL = 14;//�Խ������ڸ��ӵľ���
    PriorityQueue<Node> openQueue;//׼������
    HashSet<Node> closedSet;//�������
    Vector2Int start, target;
    NodeMap map;
    [Header("A* �㷨����")]
    [Tooltip("����ʽ������Ȩ�ء�>1 ����쵫���ܷ�����·����1 Ϊ��׼A*��")]
    [Range(1f, 5f)]
    public float heuristicWeight = 1f; // Ĭ�ϸ�һ��1��Ȩ��

    public List<Node> FindPath(NodeMap mapData, Vector2Int startPos, Vector2Int targetPos)
    {
        // 1. ��ʼ��
        this.map = mapData;
        this.start = startPos;
        this.target = targetPos;

        // ��ʼ�����ȶ���
        openQueue = new PriorityQueue<Node>((a, b) => a.CompareTo(b));
        closedSet = new HashSet<Node>();

        Node startNode = map[start];
        Node targetNode = map[target];

        if (startNode == null || targetNode == null || startNode.state == TileNodeState.Occupy)
        {
            Debug.LogWarning("AStar: �����յ���Ч����������ϰ��");
            return null;
        }

      
        openQueue.Enqueue(startNode);

       
        // ��ʼ����������б�
        while (openQueue.Count > 0)
        {
            // Dequeue() ���Զ����ز��Ƴ� F ֵ��С�Ľڵ㡣
            Node currentNode = openQueue.Dequeue();
            if (closedSet.Contains(currentNode))
            {
                continue;
            }

            closedSet.Add(currentNode);


            // 3. �ҵ�Ŀ��
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            // 4. �����ھӽڵ�
            ProcessNeighbors(currentNode);
        }

        Debug.LogWarning("AStar: δ�ҵ�·����");
        return null;
    }
    /// <summary>
    /// ����Χ�ļӽ�ȥ
    /// </summary>
    /// <param name="currentNode"></param>
    private void ProcessNeighbors(Node currentNode)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                Vector2Int neighborPos = new Vector2Int(currentNode.coordinate.x + x, currentNode.coordinate.y + y);
                Node neighborNode = map[neighborPos];

                if (!IsNeighborValid(neighborNode))
                    continue;

                // --- �����ġ������ǡ��߼� ---
                // �ж��Ƿ���б���ƶ�
                if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                {
                    // �����б���ƶ����ͼ�������ڵ�������ֱ/ˮƽ����ĸ���
                    // ���磬���Ҫ�ƶ��� (x+1, y+1)������Ҫ��� (x+1, y) �� (x, y+1)
                    Node adjacentNode1 = map[currentNode.coordinate.x + x, currentNode.coordinate.y];
                    Node adjacentNode2 = map[currentNode.coordinate.x, currentNode.coordinate.y + y];

                    // ������������ڵĸ��Ӷ����ϰ����ô�����Խ���·�����ǲ�ͨ��
                    if (adjacentNode1 != null && adjacentNode1.state == TileNodeState.Occupy &&
                        adjacentNode2 != null && adjacentNode2.state == TileNodeState.Occupy)
                    {
                        continue; // ��������Խ����ھӣ���Ϊ�������нǡ���ס��
                    }
                }
                // --- �������߼����� ---
                int newMovementCostToNeighbor = currentNode.G + GetDistance(currentNode.coordinate, neighborNode.coordinate);


                // ʹ�����ȶ��к����ǲ�����Ҫ `openList.Contains()` ��飬��Ϊ��Ч�ʵ��¡�
                //neighborNode.G ��洢����֮ǰ����·���������ĳɱ���
                //newMovementCostToNeighbor ����������������·���������ĳɱ���
                // ��� `newMovementCostToNeighbor < neighborNode.G` �Ѿ��㹻��
                // ����ҵ�һ�����ŵ�·�������Ǿ͸��½ڵ���Ϣ�����������С�
                // ��ʹ�ɵġ����۸��ߵĽڵ����ڶ����У���Ҳ����ΪFֵ�ϸ߶���������
                // �������ձ�ȡ��ʱ�����ǻᷢ�������� closedSet �У��Ӷ���������
                if (newMovementCostToNeighbor < neighborNode.G || neighborNode.parent == null) // parent==null ��ζ������δ�����ʹ�
                {
                    neighborNode.G = newMovementCostToNeighbor;
                    int weightedH = (int)(GetDiagonalDistance(neighborNode.coordinate) * heuristicWeight);
                    neighborNode.H = weightedH;
                    neighborNode.parent = currentNode;

                    // �����º���ھӼ�����С�
                    openQueue.Enqueue(neighborNode);
                }
            }
        }
    }

    private bool IsNeighborValid(Node neighbor)
    {
        if (neighbor == null || closedSet.Contains(neighbor) || neighbor.state == TileNodeState.Occupy)
        {
            return false;
        }
        return true;
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    int GetDiagonalDistance(Vector2Int position)
    {
        int x = Mathf.Abs(target.x - position.x);
        int y = Mathf.Abs(target.y - position.y);
        int min = Mathf.Min(x, y);
        return min * FACTOR_DIAGONAL + Mathf.Abs(x - y) * FACTOR;
    }

    /// <summary>
    /// �������ڸ��ӵľ���
    /// </summary>
    /// <param name="posA"></param>
    /// <param name="posB"></param>
    /// <returns></returns>
    int GetDistance(Vector2Int posA, Vector2Int posB)
    {
        int distX = Mathf.Abs(posA.x - posB.x);
        int distY = Mathf.Abs(posA.y - posB.y);

        if (distX > 0 && distY > 0)
        {
            return FACTOR_DIAGONAL;
        }
        return FACTOR;
    }

    /// <summary>
    /// Ѱ��·���������ⷵ�ؼ���·�����ڿ��ӻ����ԡ�
    /// </summary>
    /// 
   
    public (List<Node> path, HashSet<Node> closedSet) FindPathAndReturnClosedSet(NodeMap mapData, Vector2Int startPos, Vector2Int targetPos)
    {
        // ����������߼�����ԭ�е� FindPath ����������ȫһ��
        this.map = mapData;
        this.start = startPos;
        this.target = targetPos;

        // ��ʼ�����ȶ���
        openQueue = new PriorityQueue<Node>((a, b) => a.CompareTo(b));
      
        closedSet = new HashSet<Node>();

        Node startNode = map[start];
        if (startNode != null)
        {
            startNode.H = (int)(GetDiagonalDistance(startNode.coordinate) * heuristicWeight);
            openQueue.Enqueue(startNode);
        }

        Node targetNode = map[target];

        if (startNode == null || targetNode == null || startNode.state == TileNodeState.Occupy)
        {
            return (null, closedSet);
        }

        while (openQueue.Count > 0)
        {
            Node currentNode = openQueue.Dequeue();

            if (closedSet.Contains(currentNode))
            {
                continue;
            }
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                List<Node> path = RetracePath(startNode, targetNode);
                return (path, closedSet); // �ɹ�
            }

            ProcessNeighbors(currentNode);
        }

        return (null, closedSet); // ʧ��
    }
}