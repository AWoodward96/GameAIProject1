using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{

    CharacterController myCC; // Use Unities Character controller so we don't have to do the entire physics engine

    [Header("Movement Stats")]
    [Range(1, 20)]
    public int Speed;
    [Range(1, 100)]
    public int MaxSpeed;
    [Range(0, 1)]
    public float Friction;
    [Range(0, 10)]
    public float Gravity;


    Vector3 Acceleration;
    Vector3 Velocity; // There's a built in velocity in the myCC but we'll have our own

    void Start()
    {
        myCC = GetComponent<CharacterController>();
    }

    void Update()
    {
        handleInput();
        ApplyForce(Vector3.down * Gravity); // To simulate gravity
        // Unity will detect a collision and prevent the cube from moving through the ground therefore no raycast is necessary

        CalculateMove();
    }

    void ApplyForce(Vector3 _force)
    {
        Acceleration += _force;
    }

    void CalculateMove()
    {
        Velocity += Acceleration;
        Velocity = Vector3.ClampMagnitude(Velocity, MaxSpeed);
        myCC.Move(Velocity * Time.deltaTime);
        Velocity *= Friction;
        Acceleration = Vector3.zero;
    }

    void handleInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            ApplyForce(Vector3.right * Speed);
        }

        if (Input.GetKey(KeyCode.A))
        {
            ApplyForce(Vector3.forward * Speed);
        }

        if (Input.GetKey(KeyCode.S))
        {
            ApplyForce(Vector3.left * Speed);
        }

        if (Input.GetKey(KeyCode.D))
        {
            ApplyForce(Vector3.back * Speed);
        }
    }
}
