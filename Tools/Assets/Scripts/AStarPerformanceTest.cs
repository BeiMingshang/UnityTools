using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics; // ���� Stopwatch
using System.Linq; // ���ڼ���ƽ��ֵ

// ȷ�������д��� AStar ���
[RequireComponent(typeof(AStar))]
public class AStarPerformanceTest : MonoBehaviour
{
    [Header("��ͼ����")]
    [Tooltip("��ͼ�Ŀ��")]
    public int mapWidth = 100;
    [Tooltip("��ͼ�ĸ߶�")]
    public int mapHeight = 100;

    [Header("�ϰ�������")]
    [Tooltip("��ͼ���ϰ���İٷֱ� (0 �� 1)")]
    [Range(0f, 1f)]
    public float obstaclePercentage = 0.3f;

    [Header("���Բ���")]
    [Tooltip("Ҫ���е�Ѱ·���Դ���")]
    public int numberOfRuns = 100;

    [Header("���ӻ�����")]
    [Tooltip("�Ƿ��ڳ�����ͼ�л���Gizmos")]
    public bool drawGizmos = true;
    [Tooltip("���ӵĴ�С�������ڳ����л���")]
    public float gizmoCellSize = 1.0f;

    // ˽�б���
    private AStar aStar;
    private NodeMap nodeMap;

    // --- ���ڿ��ӻ��ı��� (�洢���һ�����е�����) ---
    private HashSet<Node> lastClosedSet;
    private List<Node> lastPath;
    private Vector2Int lastStartPos;
    private Vector2Int lastEndPos;

    void Start()
    {
        // ��ȡ AStar ���������
        aStar = GetComponent<AStar>();

        // ���в���
        RunCombinedTest();
    }

    // ���¿ո��������������һ�β��ԺͿ��ӻ�
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UnityEngine.Debug.Log("--- ���¿ո��������в��� ---");
            RunCombinedTest();
        }
    }

    /// <summary>
    /// ���н�������ܲ��ԺͿ��ӻ���������
    /// </summary>
    public void RunCombinedTest()
    {
        UnityEngine.Debug.Log("--- A* �ۺϲ��Կ�ʼ ---");

        // 1. ���ɲ��Ե�ͼ (ֻ��Ҫ����һ��)
        GenerateTestMap();
        UnityEngine.Debug.Log($"��ͼ������: {mapWidth}x{mapHeight}, �ϰ������: {obstaclePercentage * 100}%");

        List<double> executionTimes = new List<double>();
        Stopwatch stopwatch = new Stopwatch();
        int pathsFound = 0;

        // 2. ѭ�����в���
        for (int i = 0; i < numberOfRuns; i++)
        {
            // --- ��ÿ��Ѱ·ǰ���õ�ͼ״̬ ---
            nodeMap.ResetNodeStates();
            // Ϊÿ�β������ѡ���ͨ�е������յ�
            Vector2Int startPos = GetRandomWalkablePosition();
            Vector2Int endPos = GetRandomWalkablePosition(startPos);

            // 3. ���в���ʱ
            stopwatch.Restart(); // ���ò���ʼ��ʱ

            // ��������֮ǰ�����ġ��ܷ����������ݵķ���
            (List<Node> path, HashSet<Node> closedSet) result = aStar.FindPathAndReturnClosedSet(nodeMap, startPos, endPos);

            stopwatch.Stop(); // ֹͣ��ʱ

            // 4. ��¼��������
            executionTimes.Add(stopwatch.Elapsed.TotalMilliseconds);
            if (result.path != null)
            {
                pathsFound++;
            }

            // 5. ��������һ�����У��򱣴����������ڿ��ӻ�
            if (i == numberOfRuns - 1)
            {
                lastPath = result.path;
                lastClosedSet = result.closedSet;
                lastStartPos = startPos;
                lastEndPos = endPos;
                UnityEngine.Debug.Log("���һ��Ѱ·�����ѱ������ڿ��ӻ���");
            }
        }

        // 6. �������յ����ܽ��
        ReportResults(executionTimes, pathsFound);
    }

    /// <summary>
    /// ��ȡһ������Ŀ�ͨ������
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
    /// ����һ����������ϰ���Ĳ��Ե�ͼ
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
                i--; // ����ѭ����Ч��������һ��
            }
            else
            {
                nodeMap[pos].state = TileNodeState.Occupy;
            }
        }
    }

    /// <summary>
    /// �ڿ���̨�д�ӡ���Խ��
    /// </summary>
    void ReportResults(List<double> times, int foundCount)
    {
        if (times.Count == 0)
        {
            UnityEngine.Debug.LogWarning("û����Ч�Ĳ������С�");
            return;
        }

        double totalTime = times.Sum();
        double averageTime = times.Average();
        double maxTime = times.Max();
        double minTime = times.Min();

        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("--- A* ���ܲ��Ա��� ---");
        report.AppendLine($"�����д���: {numberOfRuns}");
        report.AppendLine($"�ɹ��ҵ�·������: {foundCount} ({((float)foundCount / numberOfRuns) * 100:F1}%)");
        report.AppendLine("--------------------------");
        report.AppendLine($"�ܺ�ʱ: {totalTime:F4} ms");
        report.AppendLine($"ƽ����ʱ: {averageTime:F4} ms");
        report.AppendLine($"���һ�κ�ʱ: {minTime:F4} ms");
        report.AppendLine($"����һ�κ�ʱ: {maxTime:F4} ms");
        report.AppendLine("--- ���Խ��� ---");

        UnityEngine.Debug.Log(report.ToString());
    }

    /// <summary>
    /// Unity�����⺯����������Scene��ͼ�л���Gizmos
    /// </summary>
    void OnDrawGizmos()
    {
        if (!drawGizmos || nodeMap == null) return;

        Vector3 offset = new Vector3(-mapWidth * gizmoCellSize / 2f, -mapHeight * gizmoCellSize / 2f, 0);

        // 1. ���Ƶ�ͼ������ϰ���
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

        // 2. ���ƹر��б� (��̽����������)
        if (lastClosedSet != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // ��ɫ��͸��
            foreach (Node node in lastClosedSet)
            {
                Vector3 position = new Vector3(node.coordinate.x * gizmoCellSize, node.coordinate.y * gizmoCellSize, 0) + offset;
                Gizmos.DrawCube(position, Vector3.one * gizmoCellSize * 0.8f);
            }
        }

        // 3. ��������·��
        if (lastPath != null)
        {
            Gizmos.color = Color.green;
            foreach (Node node in lastPath)
            {
                Vector3 position = new Vector3(node.coordinate.x * gizmoCellSize, node.coordinate.y * gizmoCellSize, 0) + offset;
                Gizmos.DrawCube(position, Vector3.one * gizmoCellSize * 0.9f);
            }
        }

        // 4. ͻ����ʾ�����յ�
        Gizmos.color = Color.blue;
        Vector3 startGizmoPos = new Vector3(lastStartPos.x * gizmoCellSize, lastStartPos.y * gizmoCellSize, 0) + offset;
        Gizmos.DrawCube(startGizmoPos, Vector3.one * gizmoCellSize);

        Gizmos.color = Color.red;
        Vector3 endGizmoPos = new Vector3(lastEndPos.x * gizmoCellSize, lastEndPos.y * gizmoCellSize, 0) + offset;
        Gizmos.DrawCube(endGizmoPos, Vector3.one * gizmoCellSize);
    }
}