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
    public Node(Vector2Int pos, Node parent, int g, int h,TileNodeState state = TileNodeState.Available)
    {
        coordinate = pos;
        this.parent = parent;
        this.g = g;
        this.h = h;
        f = this.g +this.h;
        this.state = state; // 确保状态被赋值
    }
    // 一个更简单的构造函数，在初次创建地图时很有用
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
    /// 实现比较方法，让优先队列知道如何排序
    /// </summary>
    /// <param name="other">要比较的另一个节点</param>
    /// <returns>
    ///   < 0: 当前实例排在 other 之前 (优先级更高)
    ///   = 0: 顺序相同
    ///   > 0: 当前实例排在 other 之后 (优先级更低)
    /// </returns>
    public int CompareTo(Node other)
    {
        // 首先比较 F 值。F 值小的优先级高。
        int compare = F.CompareTo(other.F);

        // 如果 F 值相同，则比较 H 值（作为决胜局）。H 值小的更接近终点，优先处理。
        if (compare == 0)
        {
            compare = H.CompareTo(other.H);
        }
        return compare;
    }

}
/// <summary>
/// 索引器地图
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
    // 按尺寸创建地图的构造函数
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
    /// 重置地图中所有节点的寻路状态 (parent, G, H, F)。
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
                // F 值会自动被 G 和 H 更新，无需手动重置
            }
        }
    }
}

public class AStar : MonoBehaviour
{
    static int FACTOR = 10;//水平竖直相邻格子的距离
    static int FACTOR_DIAGONAL = 14;//对角线相邻格子的距离
    PriorityQueue<Node> openQueue;//准备处理
    HashSet<Node> closedSet;//处理完成
    Vector2Int start, target;
    NodeMap map;
    [Header("A* 算法调整")]
    [Tooltip("启发式函数的权重。>1 会更快但可能非最优路径。1 为标准A*。")]
    [Range(1f, 5f)]
    public float heuristicWeight = 1f; // 默认给一个1的权重

    public List<Node> FindPath(NodeMap mapData, Vector2Int startPos, Vector2Int targetPos)
    {
        // 1. 初始化
        this.map = mapData;
        this.start = startPos;
        this.target = targetPos;

        // 初始化优先队列
        openQueue = new PriorityQueue<Node>((a, b) => a.CompareTo(b));
        closedSet = new HashSet<Node>();

        Node startNode = map[start];
        Node targetNode = map[target];

        if (startNode == null || targetNode == null || startNode.state == TileNodeState.Occupy)
        {
            Debug.LogWarning("AStar: 起点或终点无效，或起点是障碍物。");
            return null;
        }

      
        openQueue.Enqueue(startNode);

       
        // 开始处理待处理列表
        while (openQueue.Count > 0)
        {
            // Dequeue() 会自动返回并移除 F 值最小的节点。
            Node currentNode = openQueue.Dequeue();
            if (closedSet.Contains(currentNode))
            {
                continue;
            }

            closedSet.Add(currentNode);


            // 3. 找到目标
            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            // 4. 处理邻居节点
            ProcessNeighbors(currentNode);
        }

        Debug.LogWarning("AStar: 未找到路径。");
        return null;
    }
    /// <summary>
    /// 把周围的加进去
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

                // --- 新增的“防穿角”逻辑 ---
                // 判断是否是斜向移动
                if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
                {
                    // 如果是斜向移动，就检查它相邻的两个垂直/水平方向的格子
                    // 例如，如果要移动到 (x+1, y+1)，就需要检查 (x+1, y) 和 (x, y+1)
                    Node adjacentNode1 = map[currentNode.coordinate.x + x, currentNode.coordinate.y];
                    Node adjacentNode2 = map[currentNode.coordinate.x, currentNode.coordinate.y + y];

                    // 如果这两个相邻的格子都是障碍物，那么这条对角线路径就是不通的
                    if (adjacentNode1 != null && adjacentNode1.state == TileNodeState.Occupy &&
                        adjacentNode2 != null && adjacentNode2.state == TileNodeState.Occupy)
                    {
                        continue; // 跳过这个对角线邻居，因为它被“夹角”挡住了
                    }
                }
                // --- 防穿角逻辑结束 ---
                int newMovementCostToNeighbor = currentNode.G + GetDistance(currentNode.coordinate, neighborNode.coordinate);


                // 使用优先队列后，我们不再需要 `openList.Contains()` 检查，因为它效率低下。
                //neighborNode.G 里存储的是之前那条路径到达它的成本。
                //newMovementCostToNeighbor 是我们现在这条新路径到达它的成本。
                // 检查 `newMovementCostToNeighbor < neighborNode.G` 已经足够。
                // 如果找到一条更优的路径，我们就更新节点信息并将其加入队列。
                // 即使旧的、代价更高的节点仍在队列中，它也会因为F值较高而较晚被处理。
                // 当它最终被取出时，我们会发现它已在 closedSet 中，从而跳过它。
                if (newMovementCostToNeighbor < neighborNode.G || neighborNode.parent == null) // parent==null 意味着它从未被访问过
                {
                    neighborNode.G = newMovementCostToNeighbor;
                    int weightedH = (int)(GetDiagonalDistance(neighborNode.coordinate) * heuristicWeight);
                    neighborNode.H = weightedH;
                    neighborNode.parent = currentNode;

                    // 将更新后的邻居加入队列。
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
    /// 计算相邻格子的距离
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
    /// 寻找路径，并额外返回计算路径用于可视化调试。
    /// </summary>
    /// 
   
    public (List<Node> path, HashSet<Node> closedSet) FindPathAndReturnClosedSet(NodeMap mapData, Vector2Int startPos, Vector2Int targetPos)
    {
        // 这个方法的逻辑与你原有的 FindPath 方法几乎完全一样
        this.map = mapData;
        this.start = startPos;
        this.target = targetPos;

        // 初始化优先队列
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
                return (path, closedSet); // 成功
            }

            ProcessNeighbors(currentNode);
        }

        return (null, closedSet); // 失败
    }
}