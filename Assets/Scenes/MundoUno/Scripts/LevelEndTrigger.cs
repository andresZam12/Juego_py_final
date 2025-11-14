using UnityEngine;

public class LevelEndTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
        {
            Debug.Log("LevelEndTrigger: Player reached goal -> GoToNextWorld()");
            GameManager.Instance.GoToNextWorld();
        }
        else
        {
            Debug.LogWarning("LevelEndTrigger: GameManager.Instance is null");
        }
    }
}
