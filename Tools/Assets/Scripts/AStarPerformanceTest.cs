using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics; // 引入 Stopwatch
using System.Linq; // 用于计算平均值

// 确保场景中存在 AStar 组件
[RequireComponent(typeof(AStar))]
public class AStarPerformanceTest : MonoBehaviour
{
    [Header("地图设置")]
    [Tooltip("地图的宽度")]
    public int mapWidth = 100;
    [Tooltip("地图的高度")]
    public int mapHeight = 100;

    [Header("障碍物设置")]
    [Tooltip("地图中障碍物的百分比 (0 到 1)")]
    [Range(0f, 1f)]
    public float obstaclePercentage = 0.3f;

    [Header("测试参数")]
    [Tooltip("要运行的寻路测试次数")]
    public int numberOfRuns = 100;

    [Header("可视化设置")]
    [Tooltip("是否在场景视图中绘制Gizmos")]
    public bool drawGizmos = true;
    [Tooltip("格子的大小，用于在场景中绘制")]
    public float gizmoCellSize = 1.0f;

    // 私有变量
    private AStar aStar;
    private NodeMap nodeMap;

    // --- 用于可视化的变量 (存储最后一次运行的数据) ---
    private HashSet<Node> lastClosedSet;
    private List<Node> lastPath;
    private Vector2Int lastStartPos;
    private Vector2Int lastEndPos;

    void Start()
    {
        // 获取 AStar 组件的引用
        aStar = GetComponent<AStar>();

        // 运行测试
        RunCombinedTest();
    }

    // 按下空格键可以重新运行一次测试和可视化
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UnityEngine.Debug.Log("--- 按下空格，重新运行测试 ---");
            RunCombinedTest();
        }
    }

    /// <summary>
    /// 运行结合了性能测试和可视化的主函数
    /// </summary>
    public void RunCombinedTest()
    {
        UnityEngine.Debug.Log("--- A* 综合测试开始 ---");

        // 1. 生成测试地图 (只需要生成一次)
        GenerateTestMap();
        UnityEngine.Debug.Log($"地图已生成: {mapWidth}x{mapHeight}, 障碍物比例: {obstaclePercentage * 100}%");

        List<double> executionTimes = new List<double>();
        Stopwatch stopwatch = new Stopwatch();
        int pathsFound = 0;

        // 2. 循环运行测试
        for (int i = 0; i < numberOfRuns; i++)
        {
            // --- 在每次寻路前重置地图状态 ---
            nodeMap.ResetNodeStates();
            // 为每次测试随机选择可通行的起点和终点
            Vector2Int startPos = GetRandomWalkablePosition();
            Vector2Int endPos = GetRandomWalkablePosition(startPos);

            // 3. 运行并计时
            stopwatch.Restart(); // 重置并开始计时

            // 调用我们之前创建的、能返回所有数据的方法
            (List<Node> path, HashSet<Node> closedSet) result = aStar.FindPathAndReturnClosedSet(nodeMap, startPos, endPos);

            stopwatch.Stop(); // 停止计时

            // 4. 记录性能数据
            executionTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
            if (result.path != null)
            {
                pathsFound++;
            }

            // 5. 如果是最后一次运行，则保存其数据用于可视化
            if (i == numberOfRuns - 1)
            {
                lastPath = result.path;
                lastClosedSet = result.closedSet;
                lastStartPos = startPos;
                lastEndPos = endPos;
                UnityEngine.Debug.Log("最后一次寻路数据已保存用于可视化。");
            }
        }

        // 6. 报告最终的性能结果
        ReportResults(executionTimes, pathsFound);
    }

    /// <summary>
    /// 获取一个随机的可通行坐标
    /// </summary>
    private Vector2Int GetRandomWalkablePosition(Vector2Int? excludePos = null)
    {
        Vector2Int pos;
        do
        {
            pos = new Vector2Int(Random.Range(0, mapWidth), Random.Range(0, mapHeight));
        } while (nodeMap[pos].state == TileNodeState.Occupy || (excludePos.HasValue && pos == excludePos.Value));
        return pos;
    }

    /// <summary>
    /// 生成一个带有随机障碍物的测试地图
    /// </summary>
    void GenerateTestMap()
    {
        nodeMap = new NodeMap(mapWidth, mapHeight);
        int totalObstacles = (int)(mapWidth * mapHeight * obstaclePercentage);

        for (int i = 0; i < totalObstacles; i++)
        {
            Vector2Int pos = new Vector2Int(Random.Range(0, mapWidth), Random.Range(0, mapHeight));
            if (nodeMap[pos].state == TileNodeState.Occupy)
            {
                i--; // 本次循环无效，重新来一次
            }
            else
            {
                nodeMap[pos].state = TileNodeState.Occupy;
            }
        }
    }

    /// <summary>
    /// 在控制台中打印测试结果
    /// </summary>
    void ReportResults(List<double> times, int foundCount)
    {
        if (times.Count == 0)
        {
            UnityEngine.Debug.LogWarning("没有有效的测试运行。");
            return;
        }

        double totalTime = times.Sum();
        double averageTime = times.Average();
        double maxTime = times.Max();
        double minTime = times.Min();

        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("--- A* 性能测试报告 ---");
        report.AppendLine($"总运行次数: {numberOfRuns}");
        report.AppendLine($"成功找到路径次数: {foundCount} ({((float)foundCount / numberOfRuns) * 100:F1}%)");
        report.AppendLine("--------------------------");
        report.AppendLine($"总耗时: {totalTime:F4} ms");
        report.AppendLine($"平均耗时: {averageTime:F4} ms");
        report.AppendLine($"最快一次耗时: {minTime:F4} ms");
        report.AppendLine($"最慢一次耗时: {maxTime:F4} ms");
        report.AppendLine("--- 测试结束 ---");

        UnityEngine.Debug.Log(report.ToString());
    }

    /// <summary>
    /// Unity的特殊函数，用于在Scene视图中绘制Gizmos
    /// </summary>
    void OnDrawGizmos()
    {
        if (!drawGizmos || nodeMap == null) return;

        Vector3 offset = new Vector3(-mapWidth * gizmoCellSize / 2f, -mapHeight * gizmoCellSize / 2f, 0);

        // 1. 绘制地图网格的障碍物
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (nodeMap[x, y].state == TileNodeState.Occupy)
                {
                    Gizmos.color = Color.black;
                    Vector3 position = new Vector3(x * gizmoCellSize, y * gizmoCellSize, 0) + offset;
                    Gizmos.DrawCube(position, Vector3.one * gizmoCellSize);
                }
            }
        }

        // 2. 绘制关闭列表 (被探索过的区域)
        if (lastClosedSet != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // 橙色半透明
            foreach (Node node in lastClosedSet)
            {
                Vector3 position = new Vector3(node.coordinate.x * gizmoCellSize, node.coordinate.y * gizmoCellSize, 0) + offset;
                Gizmos.DrawCube(position, Vector3.one * gizmoCellSize * 0.8f);
            }
        }

        // 3. 绘制最终路径
        if (lastPath != null)
        {
            Gizmos.color = Color.green;
            foreach (Node node in lastPath)
            {
                Vector3 position = new Vector3(node.coordinate.x * gizmoCellSize, node.coordinate.y * gizmoCellSize, 0) + offset;
                Gizmos.DrawCube(position, Vector3.one * gizmoCellSize * 0.9f);
            }
        }

        // 4. 突出显示起点和终点
        Gizmos.color = Color.blue;
        Vector3 startGizmoPos = new Vector3(lastStartPos.x * gizmoCellSize, lastStartPos.y * gizmoCellSize, 0) + offset;
        Gizmos.DrawCube(startGizmoPos, Vector3.one * gizmoCellSize);

        Gizmos.color = Color.red;
        Vector3 endGizmoPos = new Vector3(lastEndPos.x * gizmoCellSize, lastEndPos.y * gizmoCellSize, 0) + offset;
        Gizmos.DrawCube(endGizmoPos, Vector3.one * gizmoCellSize);
    }
}