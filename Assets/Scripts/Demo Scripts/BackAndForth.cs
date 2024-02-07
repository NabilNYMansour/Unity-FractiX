using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackAndForth : MonoBehaviour
{
    public float distanceToTravel = 1;
    public Vector3 movementAxis = Vector3.right;
    public float movementSpeed = 1f;

    private Vector3 originalPos;
    void Start()
    {
        originalPos = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 movement = movementAxis * Time.time * movementSpeed;
        transform.position = originalPos + new Vector3(Mathf.Sin(movement.x), Mathf.Sin(movement.y), Mathf.Sin(movement.z)) * distanceToTravel;
    }
}
