using UnityEngine;
using System.Collections.Generic;
public class CheckoutPointManager : MonoBehaviour
{
    public static CheckoutPointManager Instance;
    private List<CheckoutPoint> allCheckoutPoints = new List<CheckoutPoint>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        RefreshAllPoints();
    }

    /// <summary>刷新全部收银点位，防止启动或者物体实例化遗漏</summary>
    public void RefreshAllPoints()
    {
        allCheckoutPoints.Clear();
        CheckoutPoint[] points = FindObjectsOfType<CheckoutPoint>();
        allCheckoutPoints.AddRange(points);
    }

    public CheckoutPoint GetFreeCheckoutPoint()
    {
        //调试打印全部收银台状态
        foreach (var p in allCheckoutPoints)
        {
            Debug.Log($"收银台:{p.gameObject.name}  占用状态:{p.isOccupied}");
        }

        foreach (var p in allCheckoutPoints)
        {
            if (!p.isOccupied)
            {
                return p;
            }
        }
        return null;
    }

    //【应急工具方法】全部强制释放，测试用；关卡重置调用
    public void ForceFreeAllPoint()
    {
        foreach (var p in allCheckoutPoints)
        {
            p.FreePoint();
        }
    }
}
