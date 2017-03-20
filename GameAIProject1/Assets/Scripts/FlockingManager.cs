using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingManager : MonoBehaviour {

    public int population;
    public GameObject flockerPrefab;
    public Transform target;
    public Vector3 startingPoint;

    private Vector3 centroid; //center of the flock

    private List<GameObject> flock;
    public List<GameObject> Flock
    {
        get { return flock; }
    }

    void Start()
    {
        //A list that will contain the flockers
        flock = new List<GameObject>();
        //The center  of the flock
        centroid = Vector3.zero;


        //CREATING THE FLOCK
        for (int i = 0; i < population; i++)
        {

            Vector3 pos = new Vector3(Random.Range(startingPoint.x -1.5f, startingPoint.x + 1.5f), startingPoint.y, Random.Range(startingPoint.z - 1.5f, startingPoint.z + 1.5f));
            GameObject unit = (GameObject)Instantiate(flockerPrefab, pos, Quaternion.identity);

            unit.GetComponent<FlockerMover>().target = target; //fills the target with something

            flock.Add(unit);
        }

        centroid = CalcCentroid(flock);

        //set the flock to the flocker's script
        foreach (GameObject flockerPrefab in flock)
        {
            flockerPrefab.GetComponent<FlockerMover>().flock = flock;
        }
    }

    void Update()
    {
        Vector3 newCentroid = CalcCentroid(flock);

        //UPDATING FLOCK CENTER
        if (centroid != newCentroid)
        {                           //only update if the center has changed
            foreach (GameObject flocker in flock)
            {           //sets the new center as the target for each flocker
                flocker.GetComponent<FlockerMover>().flockCentroid = CalcCentroid(flock);
            }
        }
        centroid = newCentroid;

        //Draw a debug line where the centroid is
        Debug.DrawLine(centroid, new Vector3(centroid.x, centroid.y + 1, centroid.z), Color.cyan);

        //left click
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit data;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out data))
            {
                target.position = new Vector3(data.point.x, data.point.y + 0.5f, data.point.z);
            }
        }
    }

    //Calculates the center of the entire flock
    public Vector3 CalcCentroid(List<GameObject> group)
    {
        Vector3 middle = Vector3.zero;

        foreach (GameObject flocker in group)
        { //adds all the positions
            middle += flocker.transform.position;
        }
        middle /= population; //devides to find the center

        return middle;
    }

}
