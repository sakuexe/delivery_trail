using UnityEngine;

public class NavigationArrow : MonoBehaviour
{
    [SerializeField]
    private GameObject arrow;
    [SerializeField]
    private Camera mainCamera;
    private GameObject nextCheckpoint;

    void Start()
    {
        SetNextCheckpoint();
        GameManager.Instance.onCheckpointCleared += SetNextCheckpoint;
    }

    void OnDisable()
    {
        GameManager.Instance.onCheckpointCleared -= SetNextCheckpoint;
    }

    void Update()
    {
        PointToCheckpoint();
    }

    private void PointToCheckpoint()
    {
        Vector3 direction = (nextCheckpoint.transform.position - mainCamera.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Find the difference between the target rotation and the camera's current rotation
        Quaternion rotationDifference = Quaternion.Inverse(mainCamera.transform.rotation) * targetRotation;

        // Apply this relative rotation to the arrow
        arrow.transform.rotation = rotationDifference;

        /*GameManager.Instance.player.transform.rotation = Quaternion.LookRotation(direction);*/
        /*arrow.transform.rotation = Quaternion.FromToRotation(mainCamera.transform.position, nextCheckpoint.transform.position);*/

        Debug.DrawLine(GameManager.Instance.player.transform.position, nextCheckpoint.transform.position, Color.red);
    }

    private void SetNextCheckpoint()
    {
        nextCheckpoint = GetClosestCheckpoint();
        Debug.Log($"Next checkpoint is: {nextCheckpoint.name}");
    }

    private GameObject GetClosestCheckpoint()
    {
        GameObject closestCheckpoint = null;
        Vector3 playerPosition = GameManager.Instance.player.transform.position;
        foreach (GameObject checkpoint in GameManager.Instance.checkpoints)
        {
            if (IsCheckpointCleared(checkpoint))
                continue;
            if (!closestCheckpoint)
            {
                closestCheckpoint = checkpoint;
                continue;
            }

            float distanceToCheckpoint = Vector3.Distance(playerPosition, checkpoint.transform.position);
            if (distanceToCheckpoint < Vector3.Distance(playerPosition, closestCheckpoint.transform.position))
                closestCheckpoint = checkpoint;
        }
        return closestCheckpoint;
    }

    private bool IsCheckpointCleared(GameObject checkpoint)
    {
        foreach (Checkpoint clearedCheckpoint in GameManager.Instance.checkpointsCleared)
        {
            if (clearedCheckpoint.position == checkpoint.transform.position)
                return true;
        }
        return false;
    }
}
