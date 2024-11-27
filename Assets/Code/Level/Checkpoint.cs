using UnityEngine;

// a struct is used here, since it is a value type instead of a reference value (class)
[System.Serializable]
public struct Checkpoint
{
    public Vector3 position;
    public Quaternion rotation;
    // we can add here extra info in the future if we want
    public Vector3 linearVelocity;
    public Vector3 angularVelocity;
    public float rpm;

    public Checkpoint(Vector3 pos, Quaternion rot, Vector3 linearVel = new(), Vector3 angularVel = new(), float rpm = 0)
    {
        position = pos;
        rotation = rot;
        linearVelocity = linearVel;
        angularVelocity = angularVel;
        this.rpm = rpm;
    }
}
