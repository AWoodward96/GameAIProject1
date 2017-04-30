using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{

    public int strength;
    public Color col;
    public Node myNode;
    Renderer myRend;

    // Use this for initialization
    void Start()
    {
        //strength = Random.Range(1, 5);
        setUnitColor();

        myNode = Grid.instance.getNodeFromWorldSpace(transform.position);
        transform.position = myNode.WorldPosition + Vector3.up;
        myNode.myOccupant = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void setUnitColor()
    {
        myRend = this.GetComponent<Renderer>();
        float alpha = myRend.material.color.a;

        Color C;
        switch (strength)
        {
            case 1:
                C = Color.white;
                break;
            case 2:
                C = Color.blue;
                break;
            case 3:
                C = Color.yellow;
                break;
            default:
                C = Color.black;
                break;

        }

        C.a = alpha;
        myRend.material.color = C;
        col = C;
    }
}
