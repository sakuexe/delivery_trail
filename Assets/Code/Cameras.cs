using UnityEngine;
using Unity.Cinemachine;

public class Cameras : MonoBehaviour
{
    [SerializeField]
    private CinemachineCamera chaseCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        chaseCam.Follow = GameManager.Instance.player.transform;
        chaseCam.LookAt = GameManager.Instance.player.transform;

        chaseCam.Lens.FieldOfView = 60;
    }
}
