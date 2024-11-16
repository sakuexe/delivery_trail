using UnityEngine;

public class Tire : MonoBehaviour
{
    // how grippy the tire is from 0.0 to 1.0
    // the axises are (speed in km/h, grip factor
    [SerializeField]
    public AnimationCurve gripCurve = new(new Keyframe(0, 0), new Keyframe(150, 1));
    // how much the tire weighs (kg)
    [Range(6.8f, 22)]
    public float mass = 10.0f;
    // the size of the tire in unity units
    // you can get this value by making a box collider around the tire 
    // and getting the y scale
    [Range(0.2f, 3)]
    public float wheelSize = 0.66f;
    // the calculated radius in meters
    public float radius => wheelSize / 2;
}
