using UnityEngine;
using System.Collections;

public class Spawn : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		Vector3 position = transform.position;
		position.y = Chunk.ComputeHeight(transform.position.x, transform.position.z) + 5;
		transform.position = position;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
