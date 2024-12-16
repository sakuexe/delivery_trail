using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CheckpointController : MonoBehaviour
{
    private BoxCollider trigger;

    void Start()
    {
        trigger = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // ignore everything that does not have the playerTag
        if (!other.transform.parent.gameObject)
            return;
        if (other.transform.parent.gameObject.tag != "Player")
            return;

        // disable this checkpoint's trigger, so that the player cannot re-write the checkpoint
        trigger.enabled = false;

        CarController playerCar = other.transform.parent.gameObject.GetComponent<CarController>();
        Checkpoint checkpoint = new(playerCar.rigidBody, playerCar.powertrain.GetCurrentRpm());

        GameManager.Instance.checkpointsCleared.Add(checkpoint);
        GameManager.Instance.onCheckpointCleared?.Invoke();
    }
}
