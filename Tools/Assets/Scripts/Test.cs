using UnityEngine;
using System.Collections; // For IEnumerator if we need delays

public class SimpleGameObjectPoolTester : MonoBehaviour
{
    public GameObject prefabToPool1; // 在Inspector中指定第一个预制体
    public GameObject prefabToPool2; // 在Inspector中指定第二个预制体

    private GameObjectPool poolManager;

    void Start()
    {
        // --- 初始化 ---
        // 确保对象池管理器存在
        poolManager = GameObjectPool.Instance;
        if (poolManager == null)
        {
            Debug.Log("对象池管理器不存在，正在创建一个...");
            GameObject poolManagerGO = new GameObject("GameObjectPool_RuntimeInstance");
            poolManager = poolManagerGO.AddComponent<GameObjectPool>();
            // 注意: 如果 GameObjectPool 的 Awake 方法中有重要初始化，
            // 第一次通过 Instance 获取时可能还未执行完毕。
            // 但对于这个简单的测试，直接 AddComponent 应该可以。
        }

        if (prefabToPool1 == null || prefabToPool2 == null)
        {
            Debug.LogError("请在 Inspector 中分配 prefabToPool1 和 prefabToPool2!");
            enabled = false; // 禁用此脚本，防止后续出错
            return;
        }

        Debug.Log("--- 开始对象池测试 ---");

        // --- 执行测试场景 ---
        TestScenario_GetAndRelease();
        TestScenario_MultipleObjectsSamePool();
        TestScenario_DifferentPools();
        TestScenario_ReleaseNonPooledObject();
        TestScenario_PoolMaxSize(); // （可选，如果你的Pool类有可配置的初始最大大小）

        Debug.Log("--- 对象池测试完成 ---");
        Debug.Log("请检查控制台输出和场景层级结构。");
    }

    void TestScenario_GetAndRelease()
    {
        Debug.Log("<color=yellow>场景1: 获取对象，释放对象，然后重新获取 (测试重用)</color>");

        // 1. 从池中获取一个对象
        Debug.Log($"尝试从池中获取 '{prefabToPool1.name}'...");
        GameObject item1 = poolManager.GetItem4Pool(prefabToPool1);
        if (item1 == null) { Debug.LogError("场景1失败: GetItem4Pool 返回 null!"); return; }

        // 断言和检查
        Debug.Assert(item1.activeSelf, $"场景1断言失败: item1 获取后应为激活状态 (Prefab: {prefabToPool1.name})");
        PoolMember member1 = item1.GetComponent<PoolMember>();
        Debug.Assert(member1 != null, $"场景1断言失败: item1 应有 PoolMember 组件 (Prefab: {prefabToPool1.name})");
        Debug.Assert(member1.prefab == prefabToPool1, $"场景1断言失败: item1 的 PoolMember.prefab 不正确 (Prefab: {prefabToPool1.name})");
        int item1InstanceId = item1.GetInstanceID();
        Debug.Log($"成功获取 item1 (ID: {item1InstanceId}), 状态: {(item1.activeSelf ? "激活" : "未激活")}, 父对象: {item1.transform.parent?.name}");

        // 2. 释放该对象回池中
        Debug.Log($"尝试释放 item1 (ID: {item1InstanceId}) 回池中...");
        poolManager.ReleaseItem(item1);
        Debug.Assert(!item1.activeSelf, $"场景1断言失败: item1 释放后应为非激活状态 (Prefab: {prefabToPool1.name})");
        Debug.Log($"成功释放 item1, 状态: {(item1.activeSelf ? "激活" : "未激活")}");

        // 3. 再次从池中获取对象，检查是否为同一个
        Debug.Log($"尝试再次为 '{prefabToPool1.name}' 获取对象...");
        GameObject item2 = poolManager.GetItem4Pool(prefabToPool1);
        if (item2 == null) { Debug.LogError("场景1失败: 第二次 GetItem4Pool 返回 null!"); return; }
        Debug.Assert(item2.activeSelf, $"场景1断言失败: item2 获取后应为激活状态 (Prefab: {prefabToPool1.name})");
        Debug.Assert(item2.GetInstanceID() == item1InstanceId, $"场景1断言失败: item2 (ID: {item2.GetInstanceID()}) 应与 item1 (ID: {item1InstanceId}) 是同一个实例，表示重用 (Prefab: {prefabToPool1.name})");
        Debug.Log($"成功获取 item2 (ID: {item2.GetInstanceID()}), 状态: {(item2.activeSelf ? "激活" : "未激活")}. 与item1实例ID相同，表示重用成功。");

        // 清理：释放 item2 以便其他测试，并检查其父对象
        string expectedPoolParentName = prefabToPool1.name + "_Pool";
        Transform poolParentTransform = poolManager.transform.Find(expectedPoolParentName);
        Debug.Assert(poolParentTransform != null, $"场景1断言失败: 找不到池的父对象 '{expectedPoolParentName}'");
        Debug.Assert(item2.transform.parent == poolParentTransform, $"场景1断言失败: item2 的父对象 ({item2.transform.parent?.name}) 不是预期的池父对象 ({expectedPoolParentName})");

        poolManager.ReleaseItem(item2);
        Debug.Log("场景1结束.");
        PrintSeparator();
    }

    void TestScenario_MultipleObjectsSamePool()
    {
        Debug.Log("<color=yellow>场景2: 从同一池中获取多个不同对象</color>");

        GameObject itemA = poolManager.GetItem4Pool(prefabToPool1);
        GameObject itemB = poolManager.GetItem4Pool(prefabToPool1); // 你的池会自动扩容

        if (itemA == null || itemB == null) { Debug.LogError("场景2失败: GetItem4Pool 返回 null!"); return; }

        Debug.Assert(itemA.activeSelf, "场景2断言失败: itemA 未激活");
        Debug.Assert(itemB.activeSelf, "场景2断言失败: itemB 未激活");
        Debug.Assert(itemA.GetInstanceID() != itemB.GetInstanceID(), $"场景2断言失败: itemA 和 itemB 应该是不同实例 (Prefab: {prefabToPool1.name})");
        Debug.Log($"itemA (ID: {itemA.GetInstanceID()}) 和 itemB (ID: {itemB.GetInstanceID()}) 已获取，它们是不同实例。");

        Transform poolParent = poolManager.transform.Find(prefabToPool1.name + "_Pool");
        Debug.Assert(poolParent != null, $"场景2断言失败: 找不到 {prefabToPool1.name}_Pool 父对象");
        if (poolParent)
        {
            Debug.Assert(itemA.transform.parent == poolParent, "场景2断言失败: itemA 不在正确的池父对象下");
            Debug.Assert(itemB.transform.parent == poolParent, "场景2断言失败: itemB 不在正确的池父对象下");
            // 初始时，prefabToPool1的池应该有一个对象(来自上一个测试被释放的item2) + 新创建的一个对象给itemA + 新创建的一个对象给itemB
            // 但由于上一个测试释放了item2，所以这里itemA会复用它，itemB会是新创建的。
            // 我们主要关心的是它们都在同一个父对象下。
            Debug.Log($"{prefabToPool1.name}_Pool 下有 {poolParent.childCount} 个子对象。itemA 和 itemB 都在其下。");
        }

        poolManager.ReleaseItem(itemA);
        poolManager.ReleaseItem(itemB);
        Debug.Log("场景2结束.");
        PrintSeparator();
    }

    void TestScenario_DifferentPools()
    {
        Debug.Log("<color=yellow>场景3: 从不同池中获取对象</color>");

        GameObject itemP1 = poolManager.GetItem4Pool(prefabToPool1);
        GameObject itemP2 = poolManager.GetItem4Pool(prefabToPool2);

        if (itemP1 == null || itemP2 == null) { Debug.LogError("场景3失败: GetItem4Pool 返回 null!"); return; }

        Debug.Assert(itemP1.activeSelf, "场景3断言失败: itemP1 未激活");
        Debug.Assert(itemP2.activeSelf, "场景3断言失败: itemP2 未激活");
        Debug.Assert(itemP1.GetInstanceID() != itemP2.GetInstanceID(), "场景3断言失败: 不同预制体的对象实例ID不应相同");
        Debug.Assert(itemP1.GetComponent<PoolMember>().prefab == prefabToPool1, $"场景3断言失败: itemP1 的预制体不匹配 (应为 {prefabToPool1.name})");
        Debug.Assert(itemP2.GetComponent<PoolMember>().prefab == prefabToPool2, $"场景3断言失败: itemP2 的预制体不匹配 (应为 {prefabToPool2.name})");
        Debug.Log($"itemP1 (Prefab: {prefabToPool1.name}, ID: {itemP1.GetInstanceID()}) 和 itemP2 (Prefab: {prefabToPool2.name}, ID: {itemP2.GetInstanceID()}) 已获取。");

        Transform pool1Parent = poolManager.transform.Find(prefabToPool1.name + "_Pool");
        Transform pool2Parent = poolManager.transform.Find(prefabToPool2.name + "_Pool");
        Debug.Assert(pool1Parent != null && itemP1.transform.parent == pool1Parent, $"场景3断言失败: itemP1 不在 {prefabToPool1.name}_Pool 下");
        Debug.Assert(pool2Parent != null && itemP2.transform.parent == pool2Parent, $"场景3断言失败: itemP2 不在 {prefabToPool2.name}_Pool 下");
        Debug.Assert(pool1Parent != pool2Parent, "场景3断言失败: 不同预制体的池父对象应该是不同的");

        Debug.Log($"已为 '{prefabToPool1.name}' 和 '{prefabToPool2.name}' 创建了不同的池。");

        poolManager.ReleaseItem(itemP1);
        poolManager.ReleaseItem(itemP2);
        Debug.Log("场景3结束.");
        PrintSeparator();
    }

    void TestScenario_ReleaseNonPooledObject()
    {
        Debug.Log("<color=yellow>场景4: 尝试释放一个非池中对象 (应被销毁并打印警告)</color>");
        GameObject nonPooledObj = new GameObject("Test_NonPooledObject_Manual"); // 手动创建
        nonPooledObj.AddComponent<BoxCollider>(); // 使其更像一个真实对象

        Debug.Log($"创建了一个非池对象: {nonPooledObj.name} (ID: {nonPooledObj.GetInstanceID()})");
        Debug.LogWarning("期望下一条日志是GameObjectPool关于无法释放对象的警告...");
        poolManager.ReleaseItem(nonPooledObj); // 这应该会触发警告并销毁对象

        // 检查对象是否被销毁 (Destroy是延迟的，所以需要一点时间)
        StartCoroutine(CheckIfDestroyed(nonPooledObj, "非池对象(手动创建)"));
        // Debug.Log("场景4结束 (请检查控制台是否有警告，并且对象是否从场景中消失)."); // 移到协程后
        PrintSeparator();
    }

    void TestScenario_PoolMaxSize()
    {
        Debug.Log("<color=yellow>场景5: 测试池达到最大数量限制 (默认100)</color>");
        GameObject tempPrefab = new GameObject("TempPrefabForMaxSizeTest"); // 使用一个临时的、唯一的预制体
        tempPrefab.SetActive(false); // 预制体本身不需要激活

        int maxPoolSize = 100; // Pool类构造函数中的默认m_MaxSize
        GameObject[] obtainedItems = new GameObject[maxPoolSize+5 ]; // 多申请几个看效果
        int itemsSuccessfullyObtained = 0;

        Debug.Log($"将从新池 ('{tempPrefab.name}') 中连续获取对象，直到达到最大限制 ({maxPoolSize})...");

        for (int i = 0; i < obtainedItems.Length; i++)
        {
            // Debug.Log($"尝试获取第 {i + 1} 个 '{tempPrefab.name}' 对象...");
            obtainedItems[i] = poolManager.GetItem4Pool(tempPrefab);
            if (obtainedItems[i] != null)
            {
                itemsSuccessfullyObtained++;
                if (i < maxPoolSize)
                    Debug.Assert(obtainedItems[i] != null, $"场景5断言失败: 第 {i + 1} 个对象不应为null (在最大容量内)");
            }
            else
            {
                Debug.Log($"第 {i + 1} 次获取 '{tempPrefab.name}' 失败 (返回null)，可能是因为已达到池的最大数量。");
                Debug.Assert(i >= maxPoolSize, $"场景5断言失败: 在达到最大容量 ({maxPoolSize}) 之前不应返回null，但在第 {i + 1} 次获取时返回了null。");
                break; // 池满了，停止获取
            }
        }

        Debug.Log($"成功从 '{tempPrefab.name}' 池中获取了 {itemsSuccessfullyObtained} 个对象。");
        Debug.Assert(itemsSuccessfullyObtained == maxPoolSize, $"场景5断言失败: 获取的对象数量 ({itemsSuccessfullyObtained}) 应等于池的最大容量 ({maxPoolSize})。");

        // 验证获取超出最大数量的对象时，会打印警告并返回null
        Debug.LogWarning("期望下一条日志是 Pool reached max size 的警告...");
        GameObject extraItem = poolManager.GetItem4Pool(tempPrefab);
        Debug.Assert(extraItem == null, "场景5断言失败: 超出最大容量后，GetItem4Pool 应返回 null。");


        Debug.Log($"尝试释放所有为 '{tempPrefab.name}' 获取的对象...");
        for (int i = 0; i < itemsSuccessfullyObtained; i++)
        {
            if (obtainedItems[i] != null)
            {
                poolManager.ReleaseItem(obtainedItems[i]);
            }
        }
        Debug.Log($"已释放所有为 '{tempPrefab.name}' 获取的对象。");

        // 清理临时预制体（在编辑器模式下，GameObject.Destroy 可能不立即生效，用DestroyImmediate）
        if (Application.isEditor && !Application.isPlaying)
            GameObject.DestroyImmediate(tempPrefab);
        else
            GameObject.Destroy(tempPrefab); // 运行时用Destroy

        Debug.Log("场景5结束.");
        PrintSeparator();
    }


    IEnumerator CheckIfDestroyed(GameObject objToCheck, string objDescription)
    {
        if (objToCheck == null) // 可能在 ReleaseItem 中立即被销毁了（如果用了DestroyImmediate）
        {
            Debug.Log($"{objDescription} 在ReleaseItem调用后立即为null，可能已被销毁。");
            yield break;
        }

        // Debug.Log($"等待一帧检查 '{objDescription}' 是否被销毁...");
        yield return null; // 等待一帧，让Destroy生效

        if (objToCheck == null) // Unity中被销毁的GameObject在C#层面会变为null
        {
            Debug.Log($"{objDescription} 已被成功销毁 (在下一帧检查结果)。");
        }
        else
        {
            Debug.LogError($"{objDescription} 未被销毁！这是一个问题。当前状态: {(objToCheck.activeSelf ? "激活" : "未激活")}, 父对象: {objToCheck.transform.parent?.name}");
        }
    }

    void PrintSeparator()
    {
        Debug.Log("--------------------------------------------------");
    }
}