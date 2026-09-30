using UnityEngine;

namespace Supermarket
{
    /// <summary>
    /// 收银台交互：挂在收银台物体上
    /// 玩家在收银台附近点击正在等待的顾客 → 完成收银
    /// </summary>
    public class CheckoutCounter : MonoBehaviour
    {
        [Header("收银台交互范围")]
        public float interactRange = 3f;

        private Transform player;

        void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        void Update()
        {
            if (player == null) return;

            // 检测鼠标点击
            if (Input.GetMouseButtonDown(0))
            {
                // 通过射线检测点击的物体
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, interactRange * 2f))
                {
                    CustomerAI customer = hit.collider.GetComponent<CustomerAI>();
                    if (customer != null)
                    {
                        // 检查距离
                        float dist = Vector3.Distance(customer.transform.position, player.position);
                        if (dist <= interactRange)
                        {
                            // 尝试收银
                            customer.CheckoutByPlayer();
                        }
                    }
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
