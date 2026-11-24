using UnityEngine;
using System.Collections;

public class PlayerWait : MonoBehaviour 
{
	public float delay = 1.0f;
	CharacterMotor characterMotor;
	
	// Use this for initialization
	void Start () 
	{
		characterMotor = GetComponent(typeof(CharacterMotor)) as CharacterMotor;
		characterMotor.enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		delay -= Time.deltaTime;
		if (delay < 0)
		{
			Destroy(this);
			characterMotor.enabled = true;
		}
	}
}
