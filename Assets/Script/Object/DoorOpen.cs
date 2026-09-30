using UnityEngine;

public class DoorOpen : MonoBehaviour
{
    public Animator doorAnimator;
    private bool isOpen = false;

    // 进入触发区域开门
    private void OnTriggerEnter(Collider other)
    {
        // 玩家 或者 顾客 进入就开门
        if ((other.CompareTag("Player") || other.CompareTag("Customer")) && !isOpen)
        {
            isOpen = true;
            doorAnimator.SetBool("DoorOpen", true);
        }
    }

    // 离开触发区域关门
    private void OnTriggerExit(Collider other)
    {
        // 玩家/顾客离开就关门
        if ((other.CompareTag("Player") || other.CompareTag("Customer")) && isOpen)
        {
            isOpen = false;
            doorAnimator.SetBool("DoorOpen", false);
        }
    }
}