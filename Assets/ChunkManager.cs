using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class ChunkManager : MonoBehaviour 
{
	public const int poolStart = 1500;
	
	public const int 	chunkSize = 32;
	public const float 	size = 1f;
		
	const int maxViewDistance = 7;
	const int maxVerticalViewDistance = 5;
	const int intialViewDistance = 3; 	// less than maxViewDistance!
	
	public Material[] materials;
	
	Transform cameraTransform;
	Transform cachedTransform;
	
	Vector3 chunkPos;
	Vector3 newChunkPos;
	
	// helper
	//float sqrSpawnDistance = maxViewDistance * chunkSize * size * maxViewDistance * chunkSize * size; 
	float sqrDeleteDistance = ((maxViewDistance * 2) * chunkSize * size) * ((maxViewDistance * 2) * chunkSize * size);
	
	// chunk lists
	List<Chunk> chunks = new List<Chunk>();
	[System.NonSerialized] public List<Chunk> loadVoxelList = new List<Chunk>();
	[System.NonSerialized] public List<Chunk> loadMeshList = new List<Chunk>();
	[System.NonSerialized] public List<Chunk> linkMeshList = new List<Chunk>();
	[System.NonSerialized] public List<Chunk> lightenList = new List<Chunk>();
	public Mutex mutexLoadMeshList = new Mutex();
	public Mutex mutexLinkMeshList = new Mutex();
	public Mutex mutexLightenList = new Mutex();
	
	void Awake()
	{
		Singleton = this;
		cachedTransform = transform;
		SimplexNoise.simplexSeed = Random.Range(int.MinValue, int.MaxValue);
		FillPool();
		Chunk.FillPool();
	}
	
	void Start () 
	{
		List<Chunk> initialLoad = new List<Chunk>();
		
		cameraTransform =  Camera.main.transform;
		chunkPos =  new Vector3(Mathf.Floor(cameraTransform.position.x / (chunkSize * size)), Mathf.Floor(cameraTransform.position.y / (chunkSize * size)), Mathf.Floor(cameraTransform.position.z / (chunkSize * size)));		
		if (cameraTransform.position.x < 0) chunkPos.x--;
		if (cameraTransform.position.y < 0) chunkPos.y--;
		if (cameraTransform.position.z < 0) chunkPos.z--;

		for (int x = -maxViewDistance; x <= maxViewDistance; x++)
			for (int y = -maxVerticalViewDistance; y <= maxVerticalViewDistance; y++)
				for (int z = -maxViewDistance; z <= maxViewDistance; z++)
				{
					Chunk chunk = AddChunkToWorld(chunkPos, x, y, z);
					if (Mathf.Abs(x) <= intialViewDistance &&
						Mathf.Abs(y) <= intialViewDistance &&
						Mathf.Abs(z) <= intialViewDistance)
							initialLoad.Add(chunk);
				}
		foreach (Chunk chunk in initialLoad)
		{
			chunk.LoadVoxel();
			loadVoxelList.Remove(chunk);
		}
		
		foreach (Chunk chunk in lightenList)
			chunk.Lighten();
		lightenList.Clear();
		
		foreach (Chunk chunk in loadMeshList)
		{
			chunk.LoadMesh();
			chunk.linkMesh();
		}
		loadMeshList.Clear();
		linkMeshList.Clear();
	}
	
	Chunk AddChunkToWorld(Vector3 chunkPos, int x, int y, int z)
	{
		Vector3 vectorPosition = new Vector3((chunkPos.x + x) * (chunkSize * size), (chunkPos.y + y) * (chunkSize * size), (chunkPos.z + z) * (chunkSize * size));
		foreach(Chunk chunkLoop in chunks)
			if ((chunkLoop.cachedTransform.position - vectorPosition).sqrMagnitude < 1)
				return null;

		Chunk chunk = getChunkFromPool();
		chunk.cachedTransform.position = new Vector3((chunkPos.x + x) * (chunkSize * size), (chunkPos.y + y) * (chunkSize * size), (chunkPos.z + z) * (chunkSize * size));
		
		chunk.initializeLink();
		
		chunks.Add(chunk);

		return chunk;
	}
	
	bool chunkChangedRunning = false;
	
	IEnumerator UpdateChunkChanged()
	{
		chunkChangedRunning = true;
		
		newChunkPos =  new Vector3(Mathf.Floor(cameraTransform.position.x / (chunkSize * size)), Mathf.Floor(cameraTransform.position.y / (chunkSize * size)), Mathf.Floor(cameraTransform.position.z / (chunkSize * size)));
		
		int n = 0;
		const int max = 1;
		
		if (chunkPos != newChunkPos)
		{
			//player has moved to another chunk
			//instantiate new chunks
			Vector3 delta = newChunkPos - chunkPos;
			
			if (Mathf.Abs(delta.x) > 0.1)
			{
				int x = maxViewDistance * Mathf.RoundToInt(delta.x);
				for (int y = - maxVerticalViewDistance; y <= maxVerticalViewDistance; y++)
					for (int z = - maxViewDistance; z <= maxViewDistance; z++)
					{
						AddChunkToWorld(newChunkPos, x, y , z);
						if (++n >= max){ n = 0; yield return null; }
					}
			}
	
			if (Mathf.Abs(delta.y) > 0.1)
			{
				int y = maxVerticalViewDistance * Mathf.RoundToInt(delta.y);
				for (int z = - maxViewDistance; z <= maxViewDistance; z++)
					for (int x = - maxViewDistance; x <= maxViewDistance; x++)
					{
						AddChunkToWorld(newChunkPos, x, y , z);
						if (++n >= max){ n = 0; yield return null; }
					}
			}
			
			if (Mathf.Abs(delta.z) > 0.1)
			{
				int z = maxViewDistance * Mathf.RoundToInt(delta.z);
				for (int y = - maxVerticalViewDistance; y <= maxVerticalViewDistance; y++)
					for (int x = - maxViewDistance; x <= maxViewDistance; x++)
					{
						AddChunkToWorld(newChunkPos, x, y , z);
						if (++n >= max){ n = 0; yield return null; }
					}
			}
		}
		chunkPos = newChunkPos;
		
		chunkChangedRunning = false;
	}
	
	void UpdateLoadVoxel()
	{
		foreach (Chunk chunk in loadVoxelList)
		{
			//Debug.Log(chunk.name + ", load voxel");
			if (chunk.numberRef != 0x3f)
				Debug.Log("error in list loadVoxelList");
			
			ThreadPool.QueueUserWorkItem(ThreadLoadVoxel, chunk);
			//chunk.LoadVoxel();
		}
		loadVoxelList.Clear();
	}
	
	static void ThreadLoadVoxel(System.Object chunk)
	{
		try
		{
			((Chunk)chunk).LoadVoxel();
		}
		catch (System.Exception e)
		{
			Debug.LogException(e, (Chunk)chunk);	
		}
	}
	
	void UpdateLoadMesh()
	{
		mutexLoadMeshList.WaitOne();
		foreach (Chunk chunk in loadMeshList)
		{
			//Debug.Log(chunk.name + ", load mesh");
			if (chunk.state != State.lighted)
				continue;
			if (chunk.numberLighten != 0x3f)
				Debug.Log("error in list loadMeshList");
				
			ThreadPool.QueueUserWorkItem(ThreadLoadMesh, chunk);
			//chunk.LoadMesh();
		}
		loadMeshList.Clear();
		mutexLoadMeshList.ReleaseMutex();
		
		mutexLinkMeshList.WaitOne();
		foreach (Chunk chunk in linkMeshList)
			chunk.linkMesh();
		linkMeshList.Clear();
		mutexLinkMeshList.ReleaseMutex();
	}
	
	static void ThreadLoadMesh(System.Object chunk)
	{
		try
		{
			((Chunk)chunk).LoadMesh();
		}
		catch (System.Exception e)
		{
			Debug.LogException(e, (Chunk)chunk);	
		}
	}
	
	void UpdateLighten()
	{
		//Debug.Log("UpdateLighten " + lightenList.Count);
		mutexLightenList.WaitOne();
		foreach (Chunk chunk in lightenList)
		{
			//Debug.Log(chunk.name + ", lighten");
			if (chunk.state != State.voxelLoaded)
				continue;
			if (chunk.numberLoadedVoxel != 0x3f)
				Debug.Log("error in list Lighten");
				
				ThreadPool.QueueUserWorkItem(ThreadLighten, chunk);
				//chunk.Lighten();
		}
		lightenList.Clear();
		mutexLightenList.ReleaseMutex();
	}
	
	static void ThreadLighten(System.Object chunk)
	{
		try
		{
			((Chunk)chunk).Lighten();
		}
		catch (System.Exception e)
		{
			Debug.LogException(e, (Chunk)chunk);	
		}
	}
	
	void UpdateRemoveOutOfRange()
	{
		for (int i = 0; i < chunks.Count; ++i)
		{
			Chunk chunk = chunks[i];
			if ((cameraTransform.position - chunk.cachedTransform.position).sqrMagnitude > sqrDeleteDistance)
			{
				chunk.Dispose();
				chunks.Remove(chunk);
				loadMeshList.Remove(chunk);
				loadVoxelList.Remove(chunk);
				--i;
			}
		}
	}
	
	int updateSwitch = 0;
	//const float maxUpdateTime = 0.015f;
	//float targetTime;
	
	void Update()
	{
		//targetTime = Time.time + maxUpdateTime;
		
		// split updates
		switch (updateSwitch)
		{
			case 0: UpdateRemoveOutOfRange(); break;
			case 1: if(!chunkChangedRunning) StartCoroutine(UpdateChunkChanged()); break;
			case 2: UpdateLoadVoxel(); break;
			case 3: UpdateLighten(); break;
			case 4: UpdateLoadMesh(); break;
		}
		if (++updateSwitch == 5) updateSwitch = 0;
	}
	
	public void AddBlock(int x, int y, int z)
	{
		foreach (Chunk chunk in chunks)
			if (chunk.Contains(x, y, z))
			{
				chunk.AddBlock(x, y, z);
				break;	
			}
	}
	
	public void RemoveBlock(int x, int y, int z)
	{
		foreach (Chunk chunk in chunks)
			if (chunk.Contains(x, y, z))
			{
				chunk.RemoveBlock(x, y, z);
				break;	
			}
	}
	
	public Chunk getChunkAtPosition(Vector3 position)
	{
		foreach (Chunk chunk in chunks)
		{
			if (chunk.cachedTransform.position == position)
				return chunk;
		}
		return null;	
	}
	
	void OnDestroy()
	{
		chunkPool.Clear();
	}
	
	static public ChunkManager Singleton {get; private set;}
	
	#region Chunk pool
	public static Stack<Chunk> chunkPool = new Stack<Chunk>();
	
	static Chunk instanciateChunk()
	{
		GameObject  go = new GameObject();
		go.transform.parent = ChunkManager.Singleton.cachedTransform;
		return go.AddComponent(typeof(Chunk)) as Chunk;
	}
	
	static void FillPool()
	{
		while (chunkPool.Count < poolStart)
			chunkPool.Push(instanciateChunk());
	}
	
	static Chunk getChunkFromPool()
	{
		Chunk chunk = null;
		if (chunkPool.Count == 0)
			chunk = instanciateChunk();
		else
			chunk = chunkPool.Pop();
		return chunk;
	}
	#endregion
}
