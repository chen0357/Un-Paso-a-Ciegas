using UnityEngine;

public class FollowPlayerUI : MonoBehaviour
{
    public Transform playerHead;
    public float distance = 2f;

    void LateUpdate()
    {
        if (playerHead == null) return;

        transform.position = playerHead.position + playerHead.forward * distance;

        transform.LookAt(playerHead);
        transform.Rotate(0, 180, 0);
    }
}