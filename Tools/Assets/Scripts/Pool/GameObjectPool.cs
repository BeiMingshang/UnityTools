using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// ���г�ģʽģʽ
/// </summary>
public class GameObjectPool : Singleton<GameObjectPool>
{
    Dictionary<GameObject, Pool> poolDictionary = new Dictionary<GameObject, Pool>();

    /// <summary>
    /// ����Ƿ��ж�Ӧ�Ķ������
    /// ���û��������ӣ��ʹ���
    /// </summary>
    /// <param name="item"></param>
    public bool CheckPool(GameObject item)
    {
        if (!poolDictionary.ContainsKey(item))
        {
            GameObject obj = new(item.name + "_Pool");
            obj.transform.SetParent(transform);
            var pool = new Pool(item, poolParent: obj.transform);
            poolDictionary.Add(item, pool);
            return false;
        }
        return true;

    }
    public void ReleaseItem(GameObject instance)
    {
        var marker = instance.GetComponent<PoolMember>();
        if (marker != null && poolDictionary.TryGetValue(marker.prefab, out var pool))
        {
            pool.Release(instance);
        }
        else
        {
            Debug.LogWarning($"[GameObjectPool] �޷��ͷŶ���δ�ҵ������������ ({instance.name})");
            GameObject.Destroy(instance); // ��ѡ���ԣ����ٲ������صĶ���
        }
    }


    public GameObject GetItem4Pool(GameObject item)
    {
        //return poolDictionary[item].Get();
        CheckPool(item);
        return poolDictionary[item].AvailableObject();

    }
}

public class PoolMember : MonoBehaviour
{
    public GameObject prefab; // ָ��ԭʼ prefab
}

public class Pool
{
    [SerializeField] private GameObject gameobj_prefab;
    [SerializeField] private int size = 1;
    private Queue<GameObject> pool;
    private Transform poolParent;
    private readonly int m_MaxSize;
    private string itemPath;
    public GameObject ObjectPrefab { get => gameobj_prefab; }

    public Pool(GameObject prefab, int size = 1, int maxSize = 100, Transform poolParent = null)
    {
        m_MaxSize = maxSize;
        gameobj_prefab = prefab;
        this.size = size;
        this.poolParent = poolParent;
        pool = new Queue<GameObject>();
        for (int i = 0; i < this.size; i++)
        {
            pool.Enqueue(Clone());
        }
    }
    /// <summary>
    /// ����Ԥװ�ض���
    /// </summary>
    /// <returns></returns>
    private GameObject Clone()
    {
        var obj = GameObject.Instantiate(gameobj_prefab, poolParent);
        obj.SetActive(false);

        // ���� PoolMember ������
        var marker = obj.AddComponent<PoolMember>();
        marker.prefab = gameobj_prefab;

        return obj;
    }

    internal void Release(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
    private GameObject Get()
    {
        GameObject obj;
        if (pool.Count > 0 && !pool.Peek().activeSelf)
        {
            obj = pool.Dequeue();
        }
        else if (size < m_MaxSize)
        {
            obj = Clone();
            size++;
        }
        else
        {
            Debug.LogError("Pool reached max size.");
            return null;
        }
        pool.Enqueue(obj);
        return obj;
    }
    public GameObject AvailableObject()
    {
        var available_obj = Get();
        if(available_obj != null)
        available_obj.SetActive(true);
        return available_obj;
    }

}