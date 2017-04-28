using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Divides the world up into nodes
/// How To Use:
/// Make an object and attach this script to it.
/// Change the Grid Space so that it encapsulates everything that you want to be included in the grid
///     Any difference in terrain hight will be taken into account just as long as this object is above the highest point (and it's less then 100 units away)
/// </summary>
public class Grid : MonoBehaviour {

    public static Grid instance;

    public Vector2 GridSpace; // How big the Grid is in world space
    public float NodeSize; // How large each node will represent

    public LayerMask GroundMask; // The ground, where the node positions will be
    public LayerMask ObstacleMask; // Obstacles that might want to be avoided. Will be checking for this mask at every node position to mark a node as unwalkable

    public bool ShowGridGizmos; // For visualizing the grids WARNING: CAN BECOME EXTREMELY LAGGY IF YOU HAVE A VERY SMALL NODE SIZE OVER A LARGE AREA

    int gridSizeX; // How many nodes are on the X Axis
    int gridSizeY; // How many nodes are on the Y Axis

    public Node[,] GridNodes;

	private List<GameObject> units = new List<GameObject>(); //a list of all the units on the terrain

	// Use this for initialization
	void Awake () {
        instance = this;

        gridSizeX = (int)(GridSpace.x / NodeSize);
        gridSizeY = (int)(GridSpace.y / NodeSize);
        GridNodes = new Node[gridSizeX, gridSizeY];

        int totalNodes = gridSizeX * gridSizeY; // Keeping it in 2 dimensions because 3d is hard and gravity already exists. There's no need for it.

        // Make a bunch of nodes
        // For every node we're going to have to do 2 checks
        // 1 for the actual world position of the node (Raycast)
        // 1 for checking for obstacles to mark as walkable or unwalkable (Physics check)
        // Note: The mountains will not be marked as obstacles by default. We'll have to dump an invisible object on them thats marked as an obstacle to fix this (should we not want units to move over the mountain)
        Vector3 nodeStartPosition = new Vector3(transform.position.x - GridSpace.x/2, transform.position.y, transform.position.z- GridSpace.y/2);
        for (int x = 0; x < gridSizeX; x++)
        {
            for(int y = 0; y < gridSizeY; y++)
            { 
                Node currentNode = new Node(x,y);

                // Do the raycast check for the ground
                Vector3 currentDisplacement = new Vector3(x * NodeSize, 0, y * NodeSize);
                Ray r = new Ray(nodeStartPosition + currentDisplacement, Vector3.down); // make a ray at the position
                RaycastHit hit;
                if(Physics.Raycast(r,out hit,100,GroundMask)) // Then raycast in the ray direction (down)
                { 
                    // If we hit then there's the potential for this node to be walkable
                    // Set the world position to this point
                    currentNode.WorldPosition = hit.point;

                    // Check that area for an obstacle
                    if(!Physics.CheckBox(hit.point, new Vector3(NodeSize / 2, NodeSize / 2, NodeSize / 2), Quaternion.identity, ObstacleMask))
                    {
                        // If we didn't find one mark it as walkable
                        currentNode.State = Node.NodeState.Walkable;
                    }else
                    {
                        // Otherwise we found one, mark it as unwalkable
                        currentNode.State = Node.NodeState.UnWalkable;
                    }
                }
                else
                {
                    // If we didn't hit then there's no ground beneath us, it's air, don't bother
                    currentNode.State = Node.NodeState.UnWalkable;
                }

                // After all of that everything should be set up for this node so dump it into the array
                GridNodes[x, y] = currentNode;
            }
        }

	}
	
	// We need a way to get a node from a world space
    // This method ignores the Y axis entirely and returns the node that encapsulates the x and z position
    public Node getNodeFromWorldSpace(Vector3 worldPosition)
    {
        // Convert the world position to a percentage
        // Far left = 0%, middle - .5%, Far Right = 1%
        float percentX = (worldPosition.x + GridSpace.x / 2 + (-transform.position.x)) / GridSpace.x;
        float percentY = (worldPosition.z + GridSpace.y / 2 + (-transform.position.z)) / GridSpace.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return GridNodes[x, y];
    }

    private void OnDrawGizmos()
    {
		//adds every Unit to the list
		units.AddRange (GameObject.FindGameObjectsWithTag ("Units"));

        // Draw the wire cube
        Gizmos.DrawWireCube(transform.position, new Vector3(GridSpace.x, 3, GridSpace.y));

        // Show each node depending on its state
        if(ShowGridGizmos && Application.isPlaying)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    //Gizmos.color = (GridNodes[x, y].State == Node.NodeState.Walkable) ? Color.grey : Color.red;

					//checks against every unit on the terrain
					for (int i = 0; i < units.Count; i++) {
						float dist = Vector3.Distance (units [i].transform.position, GridNodes [x, y].WorldPosition); //distance between cube and the unit

						//every unit
						//create the weaker color as far as the units influence goes
						if (units [i].GetComponent<UnitScript> ().strength > 0) {
							if (dist <= NodeSize * (units [i].GetComponent<UnitScript> ().strength+1)) {
								Gizmos.color = units [i].GetComponent<UnitScript> ().col * (Color.white / 2);
							}
						}
						//every unit with strength 2+
						//create the medium color 2/3 of the influence closest to center
						if (units [i].GetComponent<UnitScript> ().strength > 1) {
							if (dist <= NodeSize * (units [i].GetComponent<UnitScript> ().strength+1)/1.5) {
								Gizmos.color = units [i].GetComponent<UnitScript> ().col * (Color.white);
							}
						}
						//every unit with strength 4
						//create the darkest color 1/3 of the influence closest to center
						if (units [i].GetComponent<UnitScript> ().strength > 3) {
							if (dist <= NodeSize * (units [i].GetComponent<UnitScript> ().strength+1)/3) {
								Gizmos.color = units [i].GetComponent<UnitScript> ().col * 2;
							}
						}

					}
					//draw the cubes
					Gizmos.DrawCube (GridNodes [x, y].WorldPosition, new Vector3 (NodeSize, NodeSize, NodeSize));
                }
            } 
        }

    }

    /// <summary>
    /// Returns a list of nodes that are adjacent to the _start node
    /// </summary>
    /// <param name="_start">The node we're starting from</param>
    /// <param name="_diagonals">Do we want to get diagonal neighbors</param>
    /// <returns></returns>
    public List<Node> getNeighbors(Node _start, bool _diagonals) 
    {
        List<Node> neighbors = new List<Node>();

        if (_diagonals)
        {

            // Search in a 3 x 3 block around the node
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) // It's the same node. continue
                        continue;

                    int checkX = _start.gridX + x;
                    int checkY = _start.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        neighbors.Add(GridNodes[checkX, checkY]);
                    }

                }
            }

        }
        else
        {
            // We're doing the plus check 
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0) // It's the same node. continue
                    continue;

                int checkX = Mathf.RoundToInt(_start.gridX) + x;
                int checkY = Mathf.RoundToInt(_start.gridY);

                if (checkX >= 0 && checkX < gridSizeX)
                {
                    neighbors.Add(GridNodes[checkX, checkY]);
                }
            }

            for (int y = -1; y <= 1; y++)
            {
                if (y == 0)
                    continue;

                int checkX = Mathf.RoundToInt(_start.gridX);
                int checkY = Mathf.RoundToInt(_start.gridY) + y;

                if (checkY >= 0 && checkY < gridSizeY)
                {
                    neighbors.Add(GridNodes[checkX, checkY]);
                }

            }
        }

        return neighbors;

    }

    /// <summary>
    /// I don't want to make an entirely new class to just dump the a* algorithm into
    /// Therefore I'll just keep it here for now
    /// </summary>
    public List<Node> returnAStarPath(Vector3 _startPosition, Vector3 _endPosition)
    {
        List<Node> Path = new List<Node>(); // I just like lists honestly
        bool pathFound = false; // Lets us know if the path exists

        Heap<Node> openSet = new Heap<Node>(gridSizeX*gridSizeY); // Any node we're still looking at
        List<Node> closedSet = new List<Node>(); // Any node we're done looking at

        Node startNode = getNodeFromWorldSpace(_startPosition);
        Node endNode = getNodeFromWorldSpace(_endPosition);

        openSet.Push(startNode); // Start us off here

        // Loop until we die
        while(openSet.Count > 0)
        {
            Node current = openSet.Pop(); // Get the first node on the heap
            closedSet.Add(current);

            // Early exit
            if(current == endNode)
            {
                pathFound = true;
                break;
            } 

            // Get all the neighbors of this node
            foreach(Node neighbor in getNeighbors(current,false))
            {
                // If we've already looked at this node or it's unwalkable ignore it
                if (neighbor.State == Node.NodeState.UnWalkable || closedSet.Contains(neighbor))
                    continue;

                int currentcost = current.gCost + basicHeuristic(current, neighbor); // Caluclate how much it would cost to move there
                if (!openSet.Contains(neighbor) || currentcost < neighbor.gCost) // if the neighbor isn't in the openset OR if the cost is less then the current recorded gcost of the neighbor
                {
                    // update the info
                    neighbor.gCost = currentcost;
                    neighbor.hCost = basicHeuristic(neighbor, endNode); // HCost should be to the target not the current
                    neighbor.parent = current; 

                    if (openSet.Contains(neighbor))
                        openSet.UpdateItem(neighbor);
                    else
                        openSet.Push(neighbor);  
                }
            }
        }


        if (pathFound)
        {
            // At this point what we have is a heap that has a bunch of parented/child nodes
            // So we need to go back to the end and add walk back through the path using the parents as steping stones
            // Otherwise what'll happen is all that checking every neighbor buisness will be retraced (making a 400+ node long path)
            Node currentNode = endNode;
            while (currentNode != startNode)
            {
                Path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            //Path.Add(startNode);
            // Ok so now we have a path. Unfortunetely it starts at the end
            // Reverse it
            Path.Reverse();

        }
        else
            Debug.Log("Path was not found!");
                
        return Path;
    }


    int basicHeuristic(Node _n1, Node _n2)
    {
         
            int distX = Mathf.Abs(_n1.gridX - _n2.gridY);
            int distY = Mathf.Abs(_n1.gridY - _n2.gridY);

 
            return distX + distY;
    }
}
