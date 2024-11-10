using System;
using UnityEngine;

public class Tire : MonoBehaviour
{
    // how grippy the tire is from 0.0 to 1.0
    public float gripFactor = 0.8f;
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
