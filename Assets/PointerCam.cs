using UnityEngine;
using System.Collections;

public class PointerCam : MonoBehaviour
{

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		int x = Screen.width / 2;
        int y = Screen.height / 2;
        Ray ray = camera.ScreenPointToRay(new Vector3(x, y));
		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo))
		{
			if (hitInfo.collider.tag == "Chunk")
			{
					if (Input.GetMouseButtonDown(0))
						((Chunk)hitInfo.collider.GetComponent(typeof(Chunk))).AddBlock(Mathf.FloorToInt(hitInfo.point.x / ChunkManager.size + hitInfo.normal.x * 0.7f * ChunkManager.size), 
																						 Mathf.FloorToInt(hitInfo.point.y / ChunkManager.size + hitInfo.normal.y * 0.7f * ChunkManager.size),
																						 Mathf.FloorToInt(hitInfo.point.z / ChunkManager.size + hitInfo.normal.z * 0.7f * ChunkManager.size));
					else if (Input.GetMouseButtonDown(1))
						((Chunk)hitInfo.collider.GetComponent(typeof(Chunk))).RemoveBlock(Mathf.FloorToInt(hitInfo.point.x / ChunkManager.size - hitInfo.normal.x * 0.1f * ChunkManager.size), 
																						 	Mathf.FloorToInt(hitInfo.point.y / ChunkManager.size - hitInfo.normal.y * 0.1f * ChunkManager.size),
																						 	Mathf.FloorToInt(hitInfo.point.z / ChunkManager.size - hitInfo.normal.z * 0.1f * ChunkManager.size));
			}
			else
				Debug.Log(hitInfo.collider.name);
		}
	}
}
