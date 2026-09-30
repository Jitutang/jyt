using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("基础配置")]
    [Tooltip("顾客预制体集合，会随机从中生成")]
    public GameObject[] customerPrefabs;
    [Tooltip("顾客生成位置")]
    public Transform spawnPoint;
    [Tooltip("场景最大存活顾客数量")]
    public int maxCustomerCount = 5;
    [Tooltip("生成新顾客的间隔秒数")]
    public float spawnInterval = 10f;

    private List<CustomerAI> currentCustomers = new List<CustomerAI>();
    private float spawnTimer;

    void Start()
    {
        spawnTimer = spawnInterval;
        ValidateConfig();
    }

    void Update()
    {
        currentCustomers.RemoveAll(customer => customer == null);

        if (currentCustomers.Count >= maxCustomerCount) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnOneCustomer();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnOneCustomer()
    {
        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.LogWarning("CustomerSpawner：没有分配任何顾客预制体！", this);
            return;
        }
        if (spawnPoint == null)
        {
            Debug.LogError("CustomerSpawner：请设置spawnPoint生成点！", this);
            return;
        }

        int randomIdx = Random.Range(0, customerPrefabs.Length);
        GameObject selectedPrefab = customerPrefabs[randomIdx];

        GameObject newCustomerObj = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation, transform);
        CustomerAI customerAi = newCustomerObj.GetComponent<CustomerAI>();

        if (customerAi != null)
        {
            //✅不再传入巡逻点数组！CustomerAI内部Awake/Start自己处理行走逻辑
            currentCustomers.Add(customerAi);
        }
        else
        {
            Debug.LogError($"预制体 {selectedPrefab.name} 身上没有挂载 CustomerAI 组件", this);
            Destroy(newCustomerObj);
        }
    }

    /// <summary>
    /// 关卡重置：清空全部顾客
    /// </summary>
    public void DespawnAllCustomers()
    {
        foreach (var customer in currentCustomers)
        {
            if (customer != null)
            {
                Destroy(customer.gameObject);
            }
        }
        currentCustomers.Clear();
    }

    private void ValidateConfig()
    {
#if UNITY_EDITOR
        if (spawnPoint == null)
            Debug.LogError($"{gameObject.name} 的 CustomerSpawner：SpawnPoint 未赋值", this);
        if (customerPrefabs == null || customerPrefabs.Length == 0)
            Debug.LogWarning($"{gameObject.name} 的 CustomerSpawner：customerPrefabs 为空", this);
#endif
    }
}
