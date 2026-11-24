using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{	
	void Awake () 
	{
		if (singleton != null)
		{
			DestroyImmediate(gameObject);
			return;
		}
		singleton = this;
		DontDestroyOnLoad(gameObject);
	}
	
	void OnLevelWasLoaded()
	{
	}
	
	void Start()
	{
	}
	
	void OnGUI () 
	{
	}
	
	static GameManager singleton;
}
