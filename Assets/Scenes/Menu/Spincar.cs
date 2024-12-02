using UnityEngine;

public class Spincar : MonoBehaviour
{
    [SerializeField]
    [Range(0f, 100f)]
    private float spinspeed = 1.0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float desiretRotation =  spinspeed * Time.time;
        transform.rotation = Quaternion.Euler(0,desiretRotation, 0);
    }
}
