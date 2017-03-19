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
    public float cohesionDistance;
    public float separationDistance;
    public float lookAhead;

    private float whiskerTheta = Mathf.PI / 18; //10 degrees
    private Vector3[] whiskers = new Vector3[3];

    Vector3 Acceleration;
    Vector3 Velocity; // There's a built in velocity in the myCC but we'll have our own

    void Start()
    {
        myCC = GetComponent<CharacterController>();
    }

    void Update()
    {
        ApplyForce(Vector3.down * Gravity); // To simulate gravity
        // Unity will detect a collision and prevent the cube from moving through the ground therefore no raycast is necessary

        //Calculate flocking forces
        Seek();
        Cohesion();
        Separation();
        AvoidEdge();

        CalcForces();
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
        if (Vector3.Distance(transform.position, target.position) <= cohesionDistance)
            return;

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

        Vector3 normalizedVelocity = myCC.velocity.normalized;
        Vector3 left = new Vector3(normalizedVelocity.z, normalizedVelocity.y, -normalizedVelocity.x);
        Vector3 right = new Vector3(-normalizedVelocity.z, normalizedVelocity.y, normalizedVelocity.x);

        Vector3 middleWhisker = normalizedVelocity;
        middleWhisker *= lookAhead;
        middleWhisker += transform.position;

        //Vector3 leftWhisker = Vector3.rot

        for (int i = 0; i < whiskers.Length; i++)
        {
            
        }


        //NavMesh.Raycast(transform.position, transform.position + rayTarget, out data, NavMesh.AllAreas);

        //Draw a debug line where the raycast hits the edge
        Debug.DrawLine(data.position, new Vector3(data.position.x, data.position.y + 1, data.position.z), Color.magenta);
    }

    Vector3 zeroYComponent(Vector3 _vec)
    {
        return new Vector3(_vec.x, 0, _vec.z);
    }
}
