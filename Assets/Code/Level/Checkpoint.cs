using System.Collections;
using UnityEngine;

// a struct is used here, since it is a value type instead of a reference value (class)
[System.Serializable]
public struct Checkpoint
{
    public float rpm { get; private set; }
    // all the information about the rigidbody at the time of the checkpoint
    public Vector3 position { get; private set; }
    public Quaternion rotation { get; private set; }
    public Vector3 linearVelocity { get; private set; }
    public Vector3 angularVelocity { get; private set; }
    public Vector3 inertiaTensor { get; private set; }
    public Quaternion inertiaTensorRotation { get; private set; }
    public float linearDamping { get; private set; }
    public float angularDamping { get; private set; }

    /// <summary>
    /// Coroutine for applying the checkpoint state to a rigidbody.
    /// It is a coroutine, so that it will synchronize with the physics engine.
    /// </summary>
    /// <param name="rigidbody">
    /// The rigidbody that the state should be applied to
    /// </param>
    public IEnumerator ApplyToRigidbody(Rigidbody rigidbody)
    {
        rigidbody.position = position;
        rigidbody.rotation = rotation;

        yield return new WaitForFixedUpdate();

        rigidbody.linearVelocity = linearVelocity;
        rigidbody.angularVelocity = angularVelocity;

        yield return new WaitForFixedUpdate();

        rigidbody.inertiaTensor = inertiaTensor;
        rigidbody.inertiaTensorRotation = inertiaTensorRotation;

        yield return new WaitForFixedUpdate();

        rigidbody.linearDamping = linearDamping;
        rigidbody.angularDamping = angularDamping;

        yield return new WaitForFixedUpdate();

        // redo the application a second time, because for some reason that
        // makes the checkpoint respawn work way better
        // it is a weird solution, but it works, so shush

        rigidbody.position = position;
        rigidbody.rotation = rotation;

        yield return new WaitForFixedUpdate();

        rigidbody.linearVelocity = linearVelocity;
        rigidbody.angularVelocity = angularVelocity;

        yield return new WaitForFixedUpdate();

        rigidbody.inertiaTensor = inertiaTensor;
        rigidbody.inertiaTensorRotation = inertiaTensorRotation;

        yield return new WaitForFixedUpdate();

        rigidbody.linearDamping = linearDamping;
        rigidbody.angularDamping = angularDamping;

        yield return new WaitForFixedUpdate();
    }

    public Checkpoint(Rigidbody rigidbody, float rpm = 0)
    {
        this.rpm = rpm;
        position = rigidbody.position;
        rotation = rigidbody.rotation;
        linearVelocity = rigidbody.linearVelocity;
        angularVelocity = rigidbody.angularVelocity;
        inertiaTensor = rigidbody.inertiaTensor;
        inertiaTensorRotation = rigidbody.inertiaTensorRotation;
        linearDamping = rigidbody.linearDamping;
        angularDamping = rigidbody.angularDamping;
    }
}
