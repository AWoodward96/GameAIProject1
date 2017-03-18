using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStarFollower : MonoBehaviour {


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


    [Header("Path Finding")]
    public List<Node> myPath;
    public Transform Target;
    public int pathIndex;

    Vector3 Acceleration;
    Vector3 Velocity; // There's a built in velocity in the myCC but we'll have our own

    void Start()
    {
        myPath = Grid.instance.returnAStarPath(transform.position, Target.position);
        myCC = GetComponent<CharacterController>();
        pathIndex = 0;
    }

    void Update()
    { 
        ApplyForce(Vector3.down * Gravity); // To simulate gravity
        // Unity will detect a collision and prevent the cube from moving through the ground therefore no raycast is necessary

        CalculateMove();

        // Now move to each grid position
        Vector3 dist = myPath[pathIndex].WorldPosition - transform.position;
        dist = zeroYComponent(dist);
        if(dist.magnitude < Grid.instance.NodeSize * 2.5)
        {
            pathIndex++;
        }
        else
        {
            MoveTowards(myPath[pathIndex].WorldPosition);
        }

    }

    void MoveTowards(Vector3 _target)
    {
        Vector3 dist = _target - transform.position;
        dist = zeroYComponent(dist);
        ApplyForce(dist.normalized * Speed);
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

    Vector3 zeroYComponent(Vector3 _vec)
    {
        return new Vector3(_vec.x, 0, _vec.z);
    }

}
