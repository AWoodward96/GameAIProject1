using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraScript : MonoBehaviour {

    int[] speed = { 10, 25, 50 };
    int speedIndex = 0;
    int cameraIndex = 0;
    public Camera[] Cameras;
    Camera currentCamera;

    int unitType = 1; 

    public GameObject tempUnit; // A unit that we move around when we want to generate a new unit
    public GameObject realUnit; // A unit generated when we click
    UnitScript tempUnitScript;

    public GameObject UiPerspective;
    public GameObject UiTopDown;
    public GameObject ConstantUI;
    public bool UIToggle;
    public bool tileToggle;
   

	// Use this for initialization
	void Start () {
		foreach(Camera c in Cameras)
        {
            c.enabled = false;
        }

        tempUnitScript = tempUnit.GetComponent<UnitScript>();
        UIToggle = true;
        tileToggle = false;
	}

    // Update is called once per frame
    void Update() {

        // Handle the camera controls
        handleCameras();

        handleUnits();

        // Change the UIs based on which camera we're looking through
        UiPerspective.SetActive((cameraIndex == 0) && UIToggle);
        UiTopDown.SetActive((cameraIndex == 1) && UIToggle);
        ConstantUI.SetActive(UIToggle);

        if(Input.GetKeyDown(KeyCode.Tab))
        {
            tileToggle = !tileToggle;
            Grid.instance.ShowTiles(tileToggle);
        }

    }


    void handleCameras()
    {


        currentCamera = Cameras[cameraIndex];

        if (cameraIndex == 0) // If we're in the main camera
        {
            Perspective();
        }

        if (cameraIndex == 1) // If we're in the top down camera
        {
            TopDown();
        }


        // These key strokes are valid no matter what camera we're in
        if (Input.GetKeyDown(KeyCode.E))
        {
            speedIndex++;
            if (speedIndex > 2)
                speedIndex = 2;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            speedIndex--;
            if (speedIndex < 0)
                speedIndex = 0;
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
            UIToggle = !UIToggle;


        // Move between cameras with the f keys
        if (Input.GetKeyDown(KeyCode.F1))
            cameraIndex = 0;

        if (Input.GetKeyDown(KeyCode.F2))
            cameraIndex = 1;

        for(int i = 0; i < Cameras.Length; i++)
        {
            Cameras[i].enabled = (i == cameraIndex);
        }
    }

    void Perspective()
    { 
        
        if (Input.GetKey(KeyCode.A))
        {
            currentCamera.transform.Rotate(Vector3.up * Time.deltaTime * -speed[speedIndex] * 1.5f, Space.World);
        }

        if (Input.GetKey(KeyCode.D))
        {
            currentCamera.transform.Rotate(Vector3.up * Time.deltaTime * speed[speedIndex] * 1.5f, Space.World);
        }

        if (Input.GetKey(KeyCode.W))
        {
            currentCamera.transform.Rotate(Vector3.left * Time.deltaTime * -speed[speedIndex] * 1.5f);
        }

        if (Input.GetKey(KeyCode.S))
        {
            currentCamera.transform.Rotate(Vector3.left * Time.deltaTime * speed[speedIndex] * 1.5f);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            currentCamera.transform.position += transform.forward * Time.deltaTime * speed[speedIndex];
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentCamera.transform.position += -transform.forward * Time.deltaTime * speed[speedIndex];
        }
    }

    void TopDown()
    {
        if (Input.GetKey(KeyCode.A))
        {
            currentCamera.transform.position += Vector3.left * Time.deltaTime * speed[speedIndex] * 1.5f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            currentCamera.transform.position += Vector3.right * Time.deltaTime * speed[speedIndex] * 1.5f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            currentCamera.transform.position += Vector3.forward* Time.deltaTime* speed[speedIndex] *1.5f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            currentCamera.transform.position += Vector3.back * Time.deltaTime * speed[speedIndex] * 1.5f;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            currentCamera.transform.position += -transform.up * Time.deltaTime * speed[speedIndex];
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentCamera.transform.position += transform.up * Time.deltaTime * speed[speedIndex];
        }
    }

    void handleUnits()
    {
        bool input = false;

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4))
            input = true;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            unitType = 1;
        if (Input.GetKeyDown(KeyCode.Alpha2))
            unitType = 2;
        if (Input.GetKeyDown(KeyCode.Alpha3))
            unitType = 3;
        if (Input.GetKeyDown(KeyCode.Alpha4))
            unitType = 4;

        if(input)
        {
            tempUnitScript.strength = unitType;
            tempUnitScript.setUnitColor();
        }


        // Raycast from the camera perspective towards the ground
        // If we collide with the ground, convert that position to the grid location
        Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y); // This is the vector 2 location of where the mouse is on your 2d screen
        Ray r = currentCamera.ScreenPointToRay(mousePosition); // This turns that mouse position into a vector (or ray rather) that starts at the cursors location (in world space) and points in the direction the camera faces.
        RaycastHit hit;
        Vector3 hitPos = Vector3.zero;
        if(Physics.Raycast(r,out hit,50,LayerMask.GetMask("Ground")))
        {
            hitPos = hit.point;
        }

        if (hitPos != Vector3.zero)
        {
            // Translate that into a grid node
            Node temp = Grid.instance.getNodeFromWorldSpace(hitPos);
            hitPos = temp.WorldPosition;

            tempUnit.transform.position = hit.point+  Vector3.up;

            if (Input.GetMouseButtonDown(0))
            {
                // Check to see if the node is occupied
                if (temp.Occupied)
                {
                    Destroy(temp.myOccupant.gameObject); 
                }

                GameObject newObj = (GameObject)Instantiate(realUnit, hitPos, Quaternion.identity);
                UnitScript newScript = newObj.GetComponent<UnitScript>();
                newScript.strength = unitType;
                newScript.setUnitColor();
                Grid.instance.UpdateTiles();
            }
        }

    }
}
