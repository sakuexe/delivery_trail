using System;
using UnityEngine;

public class Tire : MonoBehaviour
{
    // how grippy the tire is from 0.0 to 1.0
    // the axises are (speed in km/h, grip factor
    [SerializeField]
    public AnimationCurve gripCurve = new(new Keyframe(0, 0), new Keyframe(150, 1));
    // how much the tire weighs (kg)
    public float mass = 10.0f;
    // tire's size in inches
    public float wheelSizeInches = 18;
    [NonSerialized]
    // the calculated radius in meters
    public float radius;

    public Tire()
    {
        radius = wheelSizeInches * 0.0254f;
    }
}
