using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PizzaBoxPhysics : MonoBehaviour
{
    [Header("Parent car")]
    [SerializeField]
    private CarController car;

    [Header("Pizza box")]
    [SerializeField]
    private GameObject pizzaBoxPrefab;
    [SerializeField]
    [Range(0, 10)]
    private int numberOfBoxes = 3;

    [Header("Box physics")]
    [SerializeField]
    [Range(1f, 10)]
    private float movementSensitivity = 2f;
    [SerializeField]
    [Range(0.01f, 1)]
    private float maxMovementSideways = 0.1f;
    [SerializeField]
    [Range(0.1f, 5)]
    private float heightMultiplier = 1f;

    // references
    private GameObject[] pizzaBoxes;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!car)
            throw new ArgumentNullException("You have to pass a car controller to the PizzaBoxContainer");
        if (!pizzaBoxPrefab)
            throw new ArgumentNullException("You have to pass a pizza box prefab to the PizzaBoxContainer");

        List<GameObject> pizzaBoxesList = new();

        for (int i = 0; i < numberOfBoxes; i++)
        {
            // spawn the box and make it a child of the object that this script is attached to
            GameObject newPizzaBox = Instantiate(pizzaBoxPrefab);
            newPizzaBox.transform.SetParent(transform);
            // position the pizza boxes on top of one another
            newPizzaBox.transform.localPosition = new Vector3(0, newPizzaBox.transform.localPosition.y + (0.2f * i), 0);
            // random rotation, so the tower will look more natural
            int randomRotation = UnityEngine.Random.Range(-8, 8);
            // make the boxes face forward (for some reason they start at -45 deg)
            newPizzaBox.transform.localRotation = Quaternion.Euler(new Vector3(0, randomRotation ,0));
            // add the box to the list
            pizzaBoxesList.Add(newPizzaBox);
        }

        pizzaBoxes = pizzaBoxesList.ToArray();

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
            float xPosition = Mathf.Clamp(normalizedAngle, -maxMovementSideways, maxMovementSideways);
            /*Debug.Log(desiredMovement);*/
            box.transform.localPosition = new Vector3(-xPosition * (index + heightMultiplier), box.transform.localPosition.y, box.transform.localPosition.z);
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
