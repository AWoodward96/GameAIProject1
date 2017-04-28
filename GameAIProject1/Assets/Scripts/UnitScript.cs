using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour {

	public int strength;
	public Color col;

	// Use this for initialization
	void Start () {
		strength = Random.Range (1,5);

		switch (strength) {
		case 1:
			this.GetComponent<Renderer> ().material.color = Color.white;
			col = Color.white;
			break;
		case 2:
			this.GetComponent<Renderer> ().material.color = Color.blue;
			col = Color.blue;
			break;
		case 3:
			this.GetComponent<Renderer> ().material.color = Color.yellow;
			col = Color.yellow;
			break;
		default:
			this.GetComponent<Renderer> ().material.color = Color.black;
			col = Color.black;
			break;

		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
