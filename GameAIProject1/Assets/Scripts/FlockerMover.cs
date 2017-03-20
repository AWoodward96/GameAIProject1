using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(CharacterController))]
public class FlockerMover : MonoBehaviour {

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

    public Transform target;
    public List<GameObject> flock;
    public Vector3 flockCentroid;
    public float arrivalDistance;
    public float separationDistance;
    public float lookAhead;
    public float wallForce;
    public float congestionRadius;

    private Vector3[] whiskers = new Vector3[3];
    private Vector3 normalizedVelocity;
    private Vector3 left;
    private Vector3 right;
    private bool cohesionFlag = true;
    private bool separationFlag = true;

    Vector3 Acceleration;
    Vector3 Velocity; // There's a built in velocity in the myCC but we'll have our own

    void Start()
    {
        myCC = GetComponent<CharacterController>();
    }

    void Update()
    {
        //calculate the left and right of the flocker
        normalizedVelocity = myCC.velocity.normalized;
        left = new Vector3(normalizedVelocity.z, normalizedVelocity.y, -normalizedVelocity.x);
        right = new Vector3(-normalizedVelocity.z, normalizedVelocity.y, normalizedVelocity.x);

        ApplyForce(Vector3.down * Gravity); // To simulate gravity
        // Unity will detect a collision and prevent the cube from moving through the ground therefore no raycast is necessary

        //Calculate flocking forces
        Seek();

        //cast navmesh raycasts to the left and right of the flocker, if they both hit, don't run cohesion
        NavMeshHit hit1, hit2;
        NavMesh.Raycast(transform.position, transform.position + (left * congestionRadius), out hit1, NavMesh.AllAreas);
        NavMesh.Raycast(transform.position, transform.position + (right * congestionRadius), out hit2, NavMesh.AllAreas);

        if (!(hit1.hit && hit2.hit))
        {
            if (cohesionFlag)
                Cohesion();
        }

        Separation();
        AvoidEdge();

        HandleInput();

        CalcForces();

        Debug.DrawLine(transform.position, transform.position + myCC.velocity, Color.black);
    }

    //Add a new force to acceleration
    void ApplyForce(Vector3 _force)
    {
        Acceleration += _force;
    }

    //Use all of the forces to calculate the velocity, and apply it
    void CalcForces()
    {
        Velocity += Acceleration;
        Velocity = Vector3.ClampMagnitude(Velocity, MaxSpeed);
        myCC.Move(Velocity * Time.deltaTime);
        Velocity *= Friction;
        Acceleration = Vector3.zero;
    }

    void Seek()
    {
        if (Vector3.Distance(transform.position, target.position) <= arrivalDistance)
        {
            return;
        }

        Vector3 desired;

        //Draw a debug line to the target
        Debug.DrawLine(transform.position, target.position, Color.red);

        desired = target.position - transform.position;                          //vector from the flocker to the target/center
        desired = zeroYComponent(desired);
        desired.Normalize();                                                //normalize it so it can be used as a velocity

        ApplyForce(desired * Speed);                                       //set velocity to be towards the center
    }

    void Cohesion()
    {
        Vector3 desired;

        Debug.DrawLine(transform.position, flockCentroid, Color.yellow);

        desired = flockCentroid - transform.position;                                                 //vector from the flocker to the target
        desired = zeroYComponent(desired);
        desired.Normalize();                                                                    //nromalize it so it can be used as a velocity

        ApplyForce(desired); 																//set velocity to be towards the target

    }

    void Separation()
    {

        foreach (GameObject flocker in flock)
        {
            if (transform.position != flocker.transform.position)
            {

                Debug.DrawLine(transform.position, flocker.transform.position, Color.black);

                float dist = Vector3.Distance(transform.position, flocker.transform.position);  //calculate the distance between flocker and target/center

                if (dist < separationDistance)
                {                                                       //if they are too close to each other, push away from eachother
                    Vector3 separationVector = flocker.transform.position - transform.position;     //vector from flocker to flocker
                    separationVector *= -1;                                                         //invert the vector, now a vector in opposite direction from flocker
                    separationVector.Normalize();                                                   //turn into unit vector
                    separationVector = zeroYComponent(separationVector);

                    ApplyForce(separationVector);                                                //add the separation to total velocity
                }
            }
        }
    }

    void AvoidEdge()
    {
        NavMeshHit data;

        Vector3 middleWhisker = normalizedVelocity;
        middleWhisker *= lookAhead;
        middleWhisker += transform.position;

        Vector3 rightWhisker = middleWhisker + left;
        Vector3 leftWhisker = middleWhisker + right;

        /*  Draw debug lines for the whiskers
        Debug.DrawLine(transform.position, rightWhisker, Color.magenta);
        Debug.DrawLine(transform.position, middleWhisker, Color.magenta);
        Debug.DrawLine(transform.position, leftWhisker, Color.magenta);
        */

        whiskers[0] = leftWhisker;
        whiskers[1] = middleWhisker;
        whiskers[2] = rightWhisker;

        for (int i = 0; i < whiskers.Length; i++)
        {
            NavMesh.Raycast(transform.position, whiskers[i], out data, NavMesh.AllAreas);

            if (data.hit)
            {
                float distanceToHit = Vector3.Distance(new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z), data.position);

                NavMeshHit closeEdgeData;
                NavMesh.FindClosestEdge(transform.position, out closeEdgeData, NavMesh.AllAreas);
                float distanceToEdge = Vector3.Distance(new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z), closeEdgeData.position);

                //Debug.Log(distanceToEdge);

                //Don't push me, cuz I'm close to the edge.
                if (distanceToEdge < 0.15)
                {
                    Bounce(closeEdgeData);
                }

                float distanceCoefficient = distanceToHit / Vector3.Distance(transform.position, whiskers[i]);
                ApplyForce(data.normal * (wallForce * (1 - distanceCoefficient)));
                return;
            }
        }
    }

    //Called when the flocker needs to stop moving
    void Bounce(NavMeshHit closeEdgeData)
    {
        ApplyForce((closeEdgeData.normal * 4));
    }

    Vector3 zeroYComponent(Vector3 _vec)
    {
        return new Vector3(_vec.x, 0, _vec.z);
    }

    void HandleInput()
    {
        //doubles cohesion strength
        if (Input.GetKeyDown(KeyCode.C))
        {
            cohesionFlag = !cohesionFlag;
        }

        //doubles separation strength
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (separationFlag)
                separationDistance *= 4;
            else
                separationDistance *= 0.25f;

            separationFlag = !separationFlag;
        }
    }
}