using UnityEngine;
using System.Collections.Generic;
using Supermarket;

namespace Supermarket
{
    /// <summary>
    /// 任务栏管理器
    ///  - 初始隐藏，接到第一个任务时显示
    ///  - 每条任务是一个 TaskItemUI 预制体实例
    ///  - Vertical Layout Group 自动按添加顺序从上到下排列
    ///    （先接的任务在上，完成后 Destroy，下面的自动上移填补）
    /// </summary>
    public class TaskPanel : MonoBehaviour
    {
        public static TaskPanel Instance;

        [Tooltip("任务条目容器")]
        public Transform taskContainer;

        [Tooltip("单条任务条目预制体")]
        public GameObject taskItemPrefab;

        // 按添加顺序维护（索引 0 = 最旧/最上方）
        private List<TaskItemUI> activeTasks = new List<TaskItemUI>();

        void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// 新增一条任务（由 CustomerAI.AcceptOrder 调用）
        /// </summary>
        public TaskItemUI AddTask(CustomerAI customer, FoodData food, int count)
        {
            if (taskItemPrefab == null || taskContainer == null)
            {
                Debug.LogWarning("TaskPanel: taskItemPrefab 或 taskContainer 未赋值！");
                return null;
            }

            // 有任务了，显示任务栏
            gameObject.SetActive(true);

            GameObject itemObj = Instantiate(taskItemPrefab, taskContainer, false);
            TaskItemUI itemUI = itemObj.GetComponent<TaskItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(customer, food, count);
            }

            activeTasks.Add(itemUI);
            // Vertical Layout Group 会自动按添加顺序从上到下排列
            return itemUI;
        }

        /// <summary>
        /// 移除一条任务（任务完成 / 顾客生气时调用）
        /// </summary>
        public void RemoveTask(TaskItemUI task)
        {
            if (task == null) return;

            activeTasks.Remove(task);
            Destroy(task.gameObject);

            // 没有任务了，隐藏整个任务栏
            if (activeTasks.Count == 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
