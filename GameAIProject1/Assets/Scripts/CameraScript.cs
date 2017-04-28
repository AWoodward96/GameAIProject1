using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey ("left")) {
			this.transform.Rotate (Vector3.up * Time.deltaTime * -10, Space.World);
		}
		else if (Input.GetKey ("right")) {
			this.transform.Rotate (Vector3.up * Time.deltaTime * 10, Space.World);
		}
		else if (Input.GetKey ("up")) {
			this.transform.Rotate (Vector3.left * Time.deltaTime * -10);
		}
		else if (Input.GetKey ("down")) {
			this.transform.Rotate (Vector3.left * Time.deltaTime * 10);
		}

		if (Input.GetKey (KeyCode.Space)) {
			this.transform.position += transform.forward * Time.deltaTime * 25;
		}

	}
}
