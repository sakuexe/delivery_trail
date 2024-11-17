using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CarController))]
public class PizzaBoxPhysics : MonoBehaviour
{
    private Transform[] pizzaBoxes;
    private Transform[] pizzaBoxInitialPositions;
    [SerializeField]
    [Range(1f, 10)]
    private float movementSensitivity = 2f;
    [SerializeField]
    [Range(0.01f, 1)]
    private float maxMovementSideways = 0.1f;
    [SerializeField]
    [Range(0.1f, 5)]
    private float heightMultiplier = 1f;

    private CarController car;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        car = gameObject.GetComponent<CarController>();
        Transform pizzaBoxContainer = transform.Find("PizzaBoxes");

        HashSet<Transform> transforms = new(pizzaBoxContainer.GetComponentsInChildren<Transform>());
        transforms.Remove(pizzaBoxContainer.transform);
        pizzaBoxes = transforms.ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        HandlePizzaBoxMovemement();
    }

    /// <summary>
    /// moves the pizza boxes depending on the car's sideways rotation
    /// </summary>
    private void HandlePizzaBoxMovemement()
    {
        foreach (var (box, index) in pizzaBoxes.Select((v, i) => (v, i)))
        {
            float normalizedAngle = NormalizeAngle(car.transform.eulerAngles.z) * (0.001f * movementSensitivity);
            float xPosition =  Mathf.Clamp(normalizedAngle, -maxMovementSideways, maxMovementSideways);
            /*Debug.Log(desiredMovement);*/
            box.localPosition = new Vector3(-xPosition * (index + heightMultiplier), box.localPosition.y, box.localPosition.z);
        }
    }

    /// <summary>
    /// turns an euler angle to -180 to 180 angle.
    /// this is more understandable for humans and the editor uses it too.
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        if (angle > 180)
            return angle - 360;
        return angle;
    }
}
