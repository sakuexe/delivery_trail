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
        GameManager.Instance.onLevelFinished += HideArrow;
    }

    void OnDisable()
    {
        GameManager.Instance.onCheckpointCleared -= SetNextCheckpoint;
        GameManager.Instance.onLevelFinished -= HideArrow;
    }

    void Update()
    {
        PointToCheckpoint();
    }

    private void PointToCheckpoint()
    {
        Vector3 direction = (nextCheckpoint.transform.position - mainCamera.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // find the difference between the target rotation and the camera's current rotation
        Quaternion rotationDifference = Quaternion.Inverse(mainCamera.transform.rotation) * targetRotation;
        // apply this relative rotation to the arrow
        arrow.transform.rotation = rotationDifference;
    }

    private void SetNextCheckpoint()
    {
        int clearedCheckpoints = GameManager.Instance.checkpointsCleared.Count - 1;
        if (clearedCheckpoints >= GameManager.Instance.checkpoints.Length)
            nextCheckpoint = GameObject.FindWithTag("Goal");
        else 
            nextCheckpoint = GameManager.Instance.checkpoints[clearedCheckpoints];
        Debug.Log($"Next checkpoint is: {nextCheckpoint.name}");
    }

    private void HideArrow()
    {
        arrow.SetActive(false);
    }
}
