using UnityEngine;
using Cinemachine;

public class HpBarFollow : MonoBehaviour
{
    [Header("绑定顾客本体")]
    public Transform targetCustomer;
    [Header("头顶偏移")]
    public Vector3 offset = new Vector3(0, 1.3f, 0);
    private Transform cameraTrans;

    void Start()
    {
        var brain = CinemachineCore.Instance.GetActiveBrain(0);
        if (brain != null)
        {
            var vCam = brain.ActiveVirtualCamera as CinemachineVirtualCamera;
            cameraTrans = vCam?.transform;
        }
        if (cameraTrans == null) cameraTrans = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (targetCustomer == null)
        {
            Destroy(gameObject);
            return;
        }
        //位置跟随
        transform.position = targetCustomer.position + offset;
        //水平朝向相机
        Vector3 dir = transform.position - cameraTrans.position;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}