using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Supermarket;

public class CustomerAI : MonoBehaviour
{
    [Header("巡逻参数（随机漫游，基于出生位置偏移）")]
    public float patrolRadius = 8f;
    public float minStayTime = 2f;
    public float maxStayTime = 5f;
    public float totalWanderTimeMin = 20f;
    public float totalWanderTimeMax = 30f;

    [Header("感叹号前置准备时间")]
    [Tooltip("感叹号单独显示多少秒后，才开始出现进度条倒计时")]
    public float prepareTime = 4f;

    [Header("表情素材")]
    public Sprite[] normalEmotions;
    public Sprite exclamationSprite;
    public Sprite angrySprite;
    public Sprite happySprite;
    [Tooltip("优先使用customerData内的头像，此字段逐步废弃")]
    public Sprite customerPortrait;

    [Header("计时参数")]
    public float waitOrderTime = 20f;
    public float serveTime = 20f;
    public float minEmotionInterval = 5f;
    public float maxEmotionInterval = 15f;
    public float minEmotionDuration = 2f;
    public float maxEmotionDuration = 4f;

    [Header("UI预制体与世界缩放")]
    public GameObject uiBarPrefab;
    [Tooltip("UI在3D世界中的整体缩放大小，建议设置在 0.005 左右")]
    public float uiWorldScale = 0.005f;

    [Header("交互设置")]
    public float checkoutInteractRange = 4f;

    [Header("背包管理器引用，拖场景上InventoryManager")]
    public InventoryManager inventoryManager;

    [Header("顾客数据资产")]
    public CustomerData customerData;

    private NavMeshAgent agent;
    private Animator anim;

    // 当前顾客本次任务随机出来的食物与数量
    private FoodData requiredFood;
    private int requiredFoodCount;

    private enum State
    {
        Wander,         // 随机漫游巡逻
        PrepareOrder,   // 感叹号弹出，停止移动
        WaitForOrder,   // 等待玩家接单
        Serving,        // 已接单，等待玩家交付
        GoToCheckout,   // 任务完成，前往收银台
        WaitingCheckout,// 到达收银台，等待玩家点击结账
        AngryLeave      // 生气离开
    }

    private State currentState;
    private Vector3 spawnPosition;
    private float totalWanderTimer;
    private float stayTimer;
    private bool isStaying;
    private float emotionTimer;
    private float emotionDuration;
    private float prepareTimer;
    private float orderTimer;
    private float serveTimer;

    //头顶UI
    private GameObject barInstance;
    private Canvas barCanvas;
    private Image emotionImage;
    private GameObject progressBarRoot;
    private Image progressFill;
    private bool uiReady;

    private TaskItemUI myTaskItem;
    private Transform playerTransform;

    // ============ 新增：当前占用的收银点位 ============
    private CheckoutPoint occupiedCheckoutPoint;

    /// <summary>
    /// 给CheckoutCounter调用：是否处于等待收银点击状态
    /// </summary>
    public bool IsWaitingCheckout
    {
        get
        {
            return currentState == State.WaitingCheckout;
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        spawnPosition = transform.position;

        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerTransform = playerObj.transform;

        currentState = State.Wander;
        totalWanderTimer = Random.Range(totalWanderTimeMin, totalWanderTimeMax);
        isStaying = false;
        emotionTimer = Random.Range(minEmotionInterval, maxEmotionInterval);

        CreateHeadUI();
    }

    /// <summary>
    /// 兼容BirthManager调用，移除checkouts参数，不再接收收银点位数组
    /// </summary>
    public void Init(Transform[] patrols)
    {
    }

    /// <summary>
    /// 兼容CheckoutCounter旧脚本调用
    /// </summary>
    public void CheckoutByPlayer()
    {
        if (currentState == State.WaitingCheckout)
            CheckoutComplete();
    }

    void CreateHeadUI()
    {
        if (uiBarPrefab == null)
        {
            Debug.LogWarning($"顾客 {gameObject.name} uiBarPrefab 为空！头顶UI不会生成");
            return;
        }
        barInstance = Instantiate(uiBarPrefab);
        HpBarFollow follow = barInstance.GetComponent<HpBarFollow>();
        if (follow != null)
            follow.targetCustomer = transform;

        barCanvas = barInstance.GetComponent<Canvas>();
        if (barCanvas != null)
        {
            barCanvas.renderMode = RenderMode.WorldSpace;
            if (Camera.main != null)
                barCanvas.worldCamera = Camera.main;
            CanvasScaler scaler = barInstance.GetComponent<CanvasScaler>();
            if (scaler != null)
                scaler.enabled = false;
        }
        barInstance.transform.localScale = Vector3.one * uiWorldScale;

        Transform emoTf = barInstance.transform.Find("EmotionImage");
        if (emoTf != null) emotionImage = emoTf.GetComponent<Image>();

        Transform barTf = barInstance.transform.Find("ProgressBar");
        if (barTf != null)
        {
            progressBarRoot = barTf.gameObject;
            Transform fillTf = barTf.Find("Fill");
            if (fillTf != null)
                progressFill = fillTf.GetComponent<Image>();
        }

        uiReady = true;
        if (emotionImage != null) emotionImage.enabled = false;
        if (progressBarRoot != null) progressBarRoot.SetActive(false);
    }

    private void OnDestroy()
    {
        // 【关键】销毁顾客的时候，释放占用的收银点位，防止卡死
        if (occupiedCheckoutPoint != null)
        {
            occupiedCheckoutPoint.FreePoint();
            occupiedCheckoutPoint = null;
        }
        if (barInstance != null)
            Destroy(barInstance);
        if (myTaskItem != null && TaskPanel.Instance != null)
        {
            TaskPanel.Instance.RemoveTask(myTaskItem);
        }
    }

    void Update()
    {
        if (anim != null && agent != null)
            anim.SetFloat("Speed", agent.velocity.magnitude);

        UpdateStateLogic();
    }

    void UpdateStateLogic()
    {
        switch (currentState)
        {
            case State.Wander:
                UpdateWanderLogic();
                if (uiReady) UpdateEmotionLogic();
                break;
            case State.PrepareOrder:
                UpdatePrepareLogic();
                break;
            case State.WaitForOrder:
                if (uiReady) UpdateWaitOrderLogic();
                break;
            case State.Serving:
                if (uiReady) UpdateServingLogic();
                break;
            case State.GoToCheckout:
                UpdateGoCheckoutLogic();
                break;
            case State.WaitingCheckout:
                break;
            case State.AngryLeave:
                break;
        }
    }

    void UpdateWanderLogic()
    {
        totalWanderTimer -= Time.deltaTime;
        if (totalWanderTimer <= 0)
        {
            StartPrepareOrder();
            return;
        }

        if (!isStaying && agent != null && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            isStaying = true;
            stayTimer = Random.Range(minStayTime, maxStayTime);
        }

        if (isStaying)
        {
            stayTimer -= Time.deltaTime;
            if (stayTimer <= 0)
            {
                Vector3 randomOffset = new Vector3(
                    Random.Range(-patrolRadius, patrolRadius),
                    0,
                    Random.Range(-patrolRadius, patrolRadius)
                );
                Vector3 nextPoint = spawnPosition + randomOffset;
                if (agent != null)
                {
                    agent.SetDestination(nextPoint);
                }
                isStaying = false;
            }
        }
    }

    void UpdateEmotionLogic()
    {
        if (emotionImage == null) return;
        emotionTimer -= Time.deltaTime;
        if (emotionTimer <= 0 && !emotionImage.enabled)
        {
            if (normalEmotions != null && normalEmotions.Length > 0)
            {
                emotionImage.sprite = normalEmotions[Random.Range(0, normalEmotions.Length)];
                emotionImage.enabled = true;
                emotionDuration = Random.Range(minEmotionDuration, maxEmotionDuration);
            }
        }
        if (emotionImage.enabled)
        {
            emotionDuration -= Time.deltaTime;
            if (emotionDuration <= 0)
            {
                emotionImage.enabled = false;
                emotionTimer = Random.Range(minEmotionInterval, maxEmotionInterval);
            }
        }
    }

    void StartPrepareOrder()
    {
        currentState = State.PrepareOrder;
        if (agent != null) agent.isStopped = true;
        if (!uiReady || emotionImage == null) return;
        emotionImage.enabled = true;
        emotionImage.sprite = exclamationSprite;
        if (progressBarRoot != null) progressBarRoot.SetActive(false);
        prepareTimer = prepareTime;
    }

    void UpdatePrepareLogic()
    {
        prepareTimer -= Time.deltaTime;
        if (prepareTimer <= 0)
        {
            StartWaitOrder();
        }
    }

    void StartWaitOrder()
    {
        currentState = State.WaitForOrder;
        if (!uiReady || emotionImage == null || progressFill == null || progressBarRoot == null)
        {
            Debug.LogWarning($"顾客 {gameObject.name} UI组件缺失");
            return;
        }
        emotionImage.enabled = true;
        emotionImage.sprite = exclamationSprite;
        progressBarRoot.SetActive(true);
        progressFill.fillAmount = 0f;
        orderTimer = waitOrderTime;
    }

    void UpdateWaitOrderLogic()
    {
        orderTimer -= Time.deltaTime;
        float progressRate = Mathf.Clamp01(1f - orderTimer / waitOrderTime);
        SetProgressColor(progressRate);
        if (orderTimer <= 0)
        {
            AngryLeave();
        }
    }

    void UpdateServingLogic()
    {
        serveTimer -= Time.deltaTime;
        float progressRate = Mathf.Clamp01(1f - serveTimer / serveTime);
        SetProgressColor(progressRate);
        if (serveTimer <= 0)
        {
            AngryLeave();
        }
    }

    void OnMouseDown()
    {
        // 距离限制：只限制【接单】，Serving状态（交付）不受距离限制，远处也可以打开背包
        bool playerInRange = true;
        if (playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            playerInRange = dist <= checkoutInteractRange;
        }

        switch (currentState)
        {
            case State.PrepareOrder:
            case State.WaitForOrder:
                //接单需要玩家在范围内
                if (playerInRange)
                {
                    AcceptOrder();
                }
                break;
            case State.Serving:
                //交付查看任务：不受距离限制，远处也能点开背包
                OpenInventoryForDelivery();
                break;
            case State.WaitingCheckout:
                CheckoutComplete();
                break;
        }
    }

    void AcceptOrder()
    {
        currentState = State.Serving;
        serveTimer = serveTime;
        //执行随机抽取
        bool pickOk = PickRandomFoodTask();
        if (!pickOk)
        {
            //抽取失败直接让顾客生气离开，避免传null进任务面板报错
            AngryLeave();
            return;
        }
        if (TaskPanel.Instance != null)
        {
            myTaskItem = TaskPanel.Instance.AddTask(this, requiredFood, requiredFoodCount);
        }
    }

    /// <summary>从CustomerData候选池随机抽取食物与数量</summary>
    private bool PickRandomFoodTask()
    {
        if (customerData == null)
        {
            Debug.LogError($"{gameObject.name} customerData为空！");
            return false;
        }
        if (customerData.candidateFoods == null || customerData.candidateFoods.Length == 0)
        {
            Debug.LogError($"{gameObject.name} CustomerData的候选食物池为空！");
            return false;
        }
        // 随机选一个食物
        int randomIndex = Random.Range(0, customerData.candidateFoods.Length);
        requiredFood = customerData.candidateFoods[randomIndex];
        // 随机数量 [min,max] 包含两端
        requiredFoodCount = Random.Range(customerData.minRequireCount, customerData.maxRequireCount + 1);
        return true;
    }

    void OpenInventoryForDelivery()
    {
        if (inventoryManager == null) return;
        if (!inventoryManager.InventoryMenu.activeSelf)
        {
            inventoryManager.ToggleInventory();
        }
        CustomerPortraitUI portraitUI = inventoryManager.InventoryMenu.GetComponentInChildren<CustomerPortraitUI>();
        if (portraitUI != null && customerData != null)
        {
            Sprite headPic = customerData.customerPortrait;
            portraitUI.SetPortrait(headPic);
        }
        // 场景里 DeliveryPanel 默认 Inactive，Awake 不会跑，Instance 一直是 null。
        // 必须走 InventoryManager 上拖好的引用，否则交付面板永远打不开，点背包食物也会被静默忽略。
        DeliveryPanel panel = inventoryManager.deliveryPanel;
        if (panel == null)
            panel = DeliveryPanel.Instance;
        if (panel != null)
        {
            panel.Open(this);
        }
        else
        {
            Debug.LogError("打不开交付面板：InventoryManager.deliveryPanel 和 DeliveryPanel.Instance 都为空");
        }
    }

    public void OnFoodDelivered()
    {
        GoToCheckout();
    }

    void GoToCheckout()
    {
        currentState = State.GoToCheckout;
        if (agent != null) agent.isStopped = false;
        if (emotionImage != null) emotionImage.enabled = false;
        if (progressBarRoot != null) progressBarRoot.SetActive(false);

        if (CheckoutPointManager.Instance == null)
        {
            Debug.LogError("场景缺少CheckoutPointManager，挂到GameManager上！");
            return;
        }
        occupiedCheckoutPoint = CheckoutPointManager.Instance.GetFreeCheckoutPoint();
        if (occupiedCheckoutPoint == null)
        {
            Debug.LogWarning("没有空闲收银台！顾客原地等待，切回漫游");
            currentState = State.Wander;
            totalWanderTimer = Random.Range(totalWanderTimeMin, totalWanderTimeMax);
            if (emotionImage != null) emotionImage.enabled = false;
            return;
        }
        occupiedCheckoutPoint.OccupyPoint();
        if (agent != null)
        {
            agent.SetDestination(occupiedCheckoutPoint.GetPosition());
        }
    }

    void UpdateGoCheckoutLogic()
    {
        if (agent == null) return;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = State.WaitingCheckout;
        }
    }

    void CheckoutComplete()
    {
        Debug.Log("✅顾客收银结算完成");
        //=====收银结算：计算订单收入加钱=====
        if (requiredFood != null)
        {
            int totalOrderMoney = requiredFood.price * requiredFoodCount;
            Debug.Log($"📋订单：{requiredFood.foodName} ×{requiredFoodCount}，总价：{totalOrderMoney}");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddMoney(totalOrderMoney);
                GameManager.Instance.SpawnFloatText(transform.position + Vector3.up * 2.2f, totalOrderMoney);
            }
        }
        //释放收银点位
        if (occupiedCheckoutPoint != null)
        {
            occupiedCheckoutPoint.FreePoint();
            occupiedCheckoutPoint = null;
        }
        //正常结算完毕，清空本次任务数据
        requiredFood = null;
        requiredFoodCount = 0;

        if (emotionImage != null && happySprite != null)
        {
            emotionImage.enabled = true;
            emotionImage.sprite = happySprite;
        }

        if (myTaskItem != null && TaskPanel.Instance != null)
        {
            TaskPanel.Instance.RemoveTask(myTaskItem);
        }
        if (barInstance != null)
            Destroy(barInstance);

        Destroy(gameObject, 1.2f);
    }

    void AngryLeave()
    {
        currentState = State.AngryLeave;
        Debug.Log("😠顾客生气离开，触发罚金");

        //生气跑单，不给奖励，立刻清空任务数据
        requiredFood = null;
        requiredFoodCount = 0;

        //释放收银点位，防止卡死
        if (occupiedCheckoutPoint != null)
        {
            occupiedCheckoutPoint.FreePoint();
            occupiedCheckoutPoint = null;
        }

        if (emotionImage != null && angrySprite != null)
        {
            emotionImage.sprite = angrySprite;
            emotionImage.enabled = true;
        }
        if (progressBarRoot != null)
        {
            progressBarRoot.SetActive(false);
        }

        //扣罚金
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SubMoney(GameManager.Instance.penaltyMoney);
            GameManager.Instance.SpawnFloatText(transform.position + Vector3.up * 2f, -GameManager.Instance.penaltyMoney);
            Debug.Log($"😠顾客生气离开，扣除罚金 {GameManager.Instance.penaltyMoney}");
        }

        if (myTaskItem != null && TaskPanel.Instance != null)
        {
            TaskPanel.Instance.RemoveTask(myTaskItem);
        }
        if (barInstance != null)
            Destroy(barInstance);

        Destroy(gameObject, 1f);
    }

    void SetProgressColor(float rate)
    {
        if (progressFill == null) return;
        progressFill.fillAmount = rate;
        Color color;
        if (rate < 0.5f)
        {
            color = Color.Lerp(Color.green, Color.yellow, rate * 2);
        }
        else
        {
            color = Color.Lerp(Color.yellow, Color.red, (rate - 0.5f) * 2);
        }
        progressFill.color = color;
    }

    //对外接口，适配TaskPanel、DeliveryPanel
    public FoodData GetRequiredFoodData()
    {
        return requiredFood;
    }
    public int GetRequiredFoodCount()
    {
        return requiredFoodCount;
    }

    public Sprite GetCustomerPortrait()
    {
        //优先拿CustomerData的数据资产头像
        if (customerData != null && customerData.customerPortrait != null)
        {
            return customerData.customerPortrait;
        }
        return customerPortrait;
    }
}
