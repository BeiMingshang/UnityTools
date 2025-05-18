using UnityEngine;
using System.Collections; // For IEnumerator if we need delays

public class SimpleGameObjectPoolTester : MonoBehaviour
{
    public GameObject prefabToPool1; // ��Inspector��ָ����һ��Ԥ����
    public GameObject prefabToPool2; // ��Inspector��ָ���ڶ���Ԥ����

    private GameObjectPool poolManager;

    void Start()
    {
        // --- ��ʼ�� ---
        // ȷ������ع���������
        poolManager = GameObjectPool.Instance;
        if (poolManager == null)
        {
            Debug.Log("����ع����������ڣ����ڴ���һ��...");
            GameObject poolManagerGO = new GameObject("GameObjectPool_RuntimeInstance");
            poolManager = poolManagerGO.AddComponent<GameObjectPool>();
            // ע��: ��� GameObjectPool �� Awake ����������Ҫ��ʼ����
            // ��һ��ͨ�� Instance ��ȡʱ���ܻ�δִ����ϡ�
            // ����������򵥵Ĳ��ԣ�ֱ�� AddComponent Ӧ�ÿ��ԡ�
        }

        if (prefabToPool1 == null || prefabToPool2 == null)
        {
            Debug.LogError("���� Inspector �з��� prefabToPool1 �� prefabToPool2!");
            enabled = false; // ���ô˽ű�����ֹ��������
            return;
        }

        Debug.Log("--- ��ʼ����ز��� ---");

        // --- ִ�в��Գ��� ---
        TestScenario_GetAndRelease();
        TestScenario_MultipleObjectsSamePool();
        TestScenario_DifferentPools();
        TestScenario_ReleaseNonPooledObject();
        TestScenario_PoolMaxSize(); // ����ѡ��������Pool���п����õĳ�ʼ����С��

        Debug.Log("--- ����ز������ ---");
        Debug.Log("�������̨����ͳ����㼶�ṹ��");
    }

    void TestScenario_GetAndRelease()
    {
        Debug.Log("<color=yellow>����1: ��ȡ�����ͷŶ���Ȼ�����»�ȡ (��������)</color>");

        // 1. �ӳ��л�ȡһ������
        Debug.Log($"���Դӳ��л�ȡ '{prefabToPool1.name}'...");
        GameObject item1 = poolManager.GetItem4Pool(prefabToPool1);
        if (item1 == null) { Debug.LogError("����1ʧ��: GetItem4Pool ���� null!"); return; }

        // ���Ժͼ��
        Debug.Assert(item1.activeSelf, $"����1����ʧ��: item1 ��ȡ��ӦΪ����״̬ (Prefab: {prefabToPool1.name})");
        PoolMember member1 = item1.GetComponent<PoolMember>();
        Debug.Assert(member1 != null, $"����1����ʧ��: item1 Ӧ�� PoolMember ��� (Prefab: {prefabToPool1.name})");
        Debug.Assert(member1.prefab == prefabToPool1, $"����1����ʧ��: item1 �� PoolMember.prefab ����ȷ (Prefab: {prefabToPool1.name})");
        int item1InstanceId = item1.GetInstanceID();
        Debug.Log($"�ɹ���ȡ item1 (ID: {item1InstanceId}), ״̬: {(item1.activeSelf ? "����" : "δ����")}, ������: {item1.transform.parent?.name}");

        // 2. �ͷŸö���س���
        Debug.Log($"�����ͷ� item1 (ID: {item1InstanceId}) �س���...");
        poolManager.ReleaseItem(item1);
        Debug.Assert(!item1.activeSelf, $"����1����ʧ��: item1 �ͷź�ӦΪ�Ǽ���״̬ (Prefab: {prefabToPool1.name})");
        Debug.Log($"�ɹ��ͷ� item1, ״̬: {(item1.activeSelf ? "����" : "δ����")}");

        // 3. �ٴδӳ��л�ȡ���󣬼���Ƿ�Ϊͬһ��
        Debug.Log($"�����ٴ�Ϊ '{prefabToPool1.name}' ��ȡ����...");
        GameObject item2 = poolManager.GetItem4Pool(prefabToPool1);
        if (item2 == null) { Debug.LogError("����1ʧ��: �ڶ��� GetItem4Pool ���� null!"); return; }
        Debug.Assert(item2.activeSelf, $"����1����ʧ��: item2 ��ȡ��ӦΪ����״̬ (Prefab: {prefabToPool1.name})");
        Debug.Assert(item2.GetInstanceID() == item1InstanceId, $"����1����ʧ��: item2 (ID: {item2.GetInstanceID()}) Ӧ�� item1 (ID: {item1InstanceId}) ��ͬһ��ʵ������ʾ���� (Prefab: {prefabToPool1.name})");
        Debug.Log($"�ɹ���ȡ item2 (ID: {item2.GetInstanceID()}), ״̬: {(item2.activeSelf ? "����" : "δ����")}. ��item1ʵ��ID��ͬ����ʾ���óɹ���");

        // �����ͷ� item2 �Ա��������ԣ�������丸����
        string expectedPoolParentName = prefabToPool1.name + "_Pool";
        Transform poolParentTransform = poolManager.transform.Find(expectedPoolParentName);
        Debug.Assert(poolParentTransform != null, $"����1����ʧ��: �Ҳ����صĸ����� '{expectedPoolParentName}'");
        Debug.Assert(item2.transform.parent == poolParentTransform, $"����1����ʧ��: item2 �ĸ����� ({item2.transform.parent?.name}) ����Ԥ�ڵĳظ����� ({expectedPoolParentName})");

        poolManager.ReleaseItem(item2);
        Debug.Log("����1����.");
        PrintSeparator();
    }

    void TestScenario_MultipleObjectsSamePool()
    {
        Debug.Log("<color=yellow>����2: ��ͬһ���л�ȡ�����ͬ����</color>");

        GameObject itemA = poolManager.GetItem4Pool(prefabToPool1);
        GameObject itemB = poolManager.GetItem4Pool(prefabToPool1); // ��ĳػ��Զ�����

        if (itemA == null || itemB == null) { Debug.LogError("����2ʧ��: GetItem4Pool ���� null!"); return; }

        Debug.Assert(itemA.activeSelf, "����2����ʧ��: itemA δ����");
        Debug.Assert(itemB.activeSelf, "����2����ʧ��: itemB δ����");
        Debug.Assert(itemA.GetInstanceID() != itemB.GetInstanceID(), $"����2����ʧ��: itemA �� itemB Ӧ���ǲ�ͬʵ�� (Prefab: {prefabToPool1.name})");
        Debug.Log($"itemA (ID: {itemA.GetInstanceID()}) �� itemB (ID: {itemB.GetInstanceID()}) �ѻ�ȡ�������ǲ�ͬʵ����");

        Transform poolParent = poolManager.transform.Find(prefabToPool1.name + "_Pool");
        Debug.Assert(poolParent != null, $"����2����ʧ��: �Ҳ��� {prefabToPool1.name}_Pool ������");
        if (poolParent)
        {
            Debug.Assert(itemA.transform.parent == poolParent, "����2����ʧ��: itemA ������ȷ�ĳظ�������");
            Debug.Assert(itemB.transform.parent == poolParent, "����2����ʧ��: itemB ������ȷ�ĳظ�������");
            // ��ʼʱ��prefabToPool1�ĳ�Ӧ����һ������(������һ�����Ա��ͷŵ�item2) + �´�����һ�������itemA + �´�����һ�������itemB
            // ��������һ�������ͷ���item2����������itemA�Ḵ������itemB�����´����ġ�
            // ������Ҫ���ĵ������Ƕ���ͬһ���������¡�
            Debug.Log($"{prefabToPool1.name}_Pool ���� {poolParent.childCount} ���Ӷ���itemA �� itemB �������¡�");
        }

        poolManager.ReleaseItem(itemA);
        poolManager.ReleaseItem(itemB);
        Debug.Log("����2����.");
        PrintSeparator();
    }

    void TestScenario_DifferentPools()
    {
        Debug.Log("<color=yellow>����3: �Ӳ�ͬ���л�ȡ����</color>");

        GameObject itemP1 = poolManager.GetItem4Pool(prefabToPool1);
        GameObject itemP2 = poolManager.GetItem4Pool(prefabToPool2);

        if (itemP1 == null || itemP2 == null) { Debug.LogError("����3ʧ��: GetItem4Pool ���� null!"); return; }

        Debug.Assert(itemP1.activeSelf, "����3����ʧ��: itemP1 δ����");
        Debug.Assert(itemP2.activeSelf, "����3����ʧ��: itemP2 δ����");
        Debug.Assert(itemP1.GetInstanceID() != itemP2.GetInstanceID(), "����3����ʧ��: ��ͬԤ����Ķ���ʵ��ID��Ӧ��ͬ");
        Debug.Assert(itemP1.GetComponent<PoolMember>().prefab == prefabToPool1, $"����3����ʧ��: itemP1 ��Ԥ���岻ƥ�� (ӦΪ {prefabToPool1.name})");
        Debug.Assert(itemP2.GetComponent<PoolMember>().prefab == prefabToPool2, $"����3����ʧ��: itemP2 ��Ԥ���岻ƥ�� (ӦΪ {prefabToPool2.name})");
        Debug.Log($"itemP1 (Prefab: {prefabToPool1.name}, ID: {itemP1.GetInstanceID()}) �� itemP2 (Prefab: {prefabToPool2.name}, ID: {itemP2.GetInstanceID()}) �ѻ�ȡ��");

        Transform pool1Parent = poolManager.transform.Find(prefabToPool1.name + "_Pool");
        Transform pool2Parent = poolManager.transform.Find(prefabToPool2.name + "_Pool");
        Debug.Assert(pool1Parent != null && itemP1.transform.parent == pool1Parent, $"����3����ʧ��: itemP1 ���� {prefabToPool1.name}_Pool ��");
        Debug.Assert(pool2Parent != null && itemP2.transform.parent == pool2Parent, $"����3����ʧ��: itemP2 ���� {prefabToPool2.name}_Pool ��");
        Debug.Assert(pool1Parent != pool2Parent, "����3����ʧ��: ��ͬԤ����ĳظ�����Ӧ���ǲ�ͬ��");

        Debug.Log($"��Ϊ '{prefabToPool1.name}' �� '{prefabToPool2.name}' �����˲�ͬ�ĳء�");

        poolManager.ReleaseItem(itemP1);
        poolManager.ReleaseItem(itemP2);
        Debug.Log("����3����.");
        PrintSeparator();
    }

    void TestScenario_ReleaseNonPooledObject()
    {
        Debug.Log("<color=yellow>����4: �����ͷ�һ���ǳ��ж��� (Ӧ�����ٲ���ӡ����)</color>");
        GameObject nonPooledObj = new GameObject("Test_NonPooledObject_Manual"); // �ֶ�����
        nonPooledObj.AddComponent<BoxCollider>(); // ʹ�����һ����ʵ����

        Debug.Log($"������һ���ǳض���: {nonPooledObj.name} (ID: {nonPooledObj.GetInstanceID()})");
        Debug.LogWarning("������һ����־��GameObjectPool�����޷��ͷŶ���ľ���...");
        poolManager.ReleaseItem(nonPooledObj); // ��Ӧ�ûᴥ�����沢���ٶ���

        // �������Ƿ����� (Destroy���ӳٵģ�������Ҫһ��ʱ��)
        StartCoroutine(CheckIfDestroyed(nonPooledObj, "�ǳض���(�ֶ�����)"));
        // Debug.Log("����4���� (�������̨�Ƿ��о��棬���Ҷ����Ƿ�ӳ�������ʧ)."); // �Ƶ�Э�̺�
        PrintSeparator();
    }

    void TestScenario_PoolMaxSize()
    {
        Debug.Log("<color=yellow>����5: ���Գشﵽ����������� (Ĭ��100)</color>");
        GameObject tempPrefab = new GameObject("TempPrefabForMaxSizeTest"); // ʹ��һ����ʱ�ġ�Ψһ��Ԥ����
        tempPrefab.SetActive(false); // Ԥ���屾����Ҫ����

        int maxPoolSize = 100; // Pool�๹�캯���е�Ĭ��m_MaxSize
        GameObject[] obtainedItems = new GameObject[maxPoolSize+5 ]; // �����뼸����Ч��
        int itemsSuccessfullyObtained = 0;

        Debug.Log($"�����³� ('{tempPrefab.name}') ��������ȡ����ֱ���ﵽ������� ({maxPoolSize})...");

        for (int i = 0; i < obtainedItems.Length; i++)
        {
            // Debug.Log($"���Ի�ȡ�� {i + 1} �� '{tempPrefab.name}' ����...");
            obtainedItems[i] = poolManager.GetItem4Pool(tempPrefab);
            if (obtainedItems[i] != null)
            {
                itemsSuccessfullyObtained++;
                if (i < maxPoolSize)
                    Debug.Assert(obtainedItems[i] != null, $"����5����ʧ��: �� {i + 1} ������ӦΪnull (�����������)");
            }
            else
            {
                Debug.Log($"�� {i + 1} �λ�ȡ '{tempPrefab.name}' ʧ�� (����null)����������Ϊ�Ѵﵽ�ص����������");
                Debug.Assert(i >= maxPoolSize, $"����5����ʧ��: �ڴﵽ������� ({maxPoolSize}) ֮ǰ��Ӧ����null�����ڵ� {i + 1} �λ�ȡʱ������null��");
                break; // �����ˣ�ֹͣ��ȡ
            }
        }

        Debug.Log($"�ɹ��� '{tempPrefab.name}' ���л�ȡ�� {itemsSuccessfullyObtained} ������");
        Debug.Assert(itemsSuccessfullyObtained == maxPoolSize, $"����5����ʧ��: ��ȡ�Ķ������� ({itemsSuccessfullyObtained}) Ӧ���ڳص�������� ({maxPoolSize})��");

        // ��֤��ȡ������������Ķ���ʱ�����ӡ���沢����null
        Debug.LogWarning("������һ����־�� Pool reached max size �ľ���...");
        GameObject extraItem = poolManager.GetItem4Pool(tempPrefab);
        Debug.Assert(extraItem == null, "����5����ʧ��: �������������GetItem4Pool Ӧ���� null��");


        Debug.Log($"�����ͷ�����Ϊ '{tempPrefab.name}' ��ȡ�Ķ���...");
        for (int i = 0; i < itemsSuccessfullyObtained; i++)
        {
            if (obtainedItems[i] != null)
            {
                poolManager.ReleaseItem(obtainedItems[i]);
            }
        }
        Debug.Log($"���ͷ�����Ϊ '{tempPrefab.name}' ��ȡ�Ķ���");

        // ������ʱԤ���壨�ڱ༭��ģʽ�£�GameObject.Destroy ���ܲ�������Ч����DestroyImmediate��
        if (Application.isEditor && !Application.isPlaying)
            GameObject.DestroyImmediate(tempPrefab);
        else
            GameObject.Destroy(tempPrefab); // ����ʱ��Destroy

        Debug.Log("����5����.");
        PrintSeparator();
    }


    IEnumerator CheckIfDestroyed(GameObject objToCheck, string objDescription)
    {
        if (objToCheck == null) // ������ ReleaseItem �������������ˣ��������DestroyImmediate��
        {
            Debug.Log($"{objDescription} ��ReleaseItem���ú�����Ϊnull�������ѱ����١�");
            yield break;
        }

        // Debug.Log($"�ȴ�һ֡��� '{objDescription}' �Ƿ�����...");
        yield return null; // �ȴ�һ֡����Destroy��Ч

        if (objToCheck == null) // Unity�б����ٵ�GameObject��C#������Ϊnull
        {
            Debug.Log($"{objDescription} �ѱ��ɹ����� (����һ֡�����)��");
        }
        else
        {
            Debug.LogError($"{objDescription} δ�����٣�����һ�����⡣��ǰ״̬: {(objToCheck.activeSelf ? "����" : "δ����")}, ������: {objToCheck.transform.parent?.name}");
        }
    }

    void PrintSeparator()
    {
        Debug.Log("--------------------------------------------------");
    }
}