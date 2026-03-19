using UnityEngine;

public class ObstacleCollisionReporter : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            GameManager.Instance.RegisterCollision();
        }
    }
}