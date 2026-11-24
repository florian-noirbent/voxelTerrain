using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

public enum VoxelType : byte 
{
	air,
	airLit,
	grass,
	rock,
	water,
	sand,
	wood
};

public enum State : byte
{
	disposed,
	initialised,
	voxelLoaded,
	lighted,
	meshGenerated
}

public struct Voxel
{
	public byte type;
	public byte data;
	public short lightning;
	
	public Voxel(byte t, byte d, bool sun = true) { type = t; data = d; lightning = 0; if (sun) lightning = 15;}
}

public class Chunk : MonoBehaviour 
{	
	public State			state = State.disposed;
	public Transform 		cachedTransform;
	public Vector3			position;
	
	static int chunkID = 0;
	
	MeshRenderer	meshRenderer;
	MeshFilter 		meshFilter;
	MeshCollider	meshCollider;
	
	Mesh			mesh;
	Mesh			meshColliderGeometry;
	
	//voxels
	Voxel[,,]		voxels = null;
	int 			voxelsEnabledCount = 0;
	
	//mesh building list
	List<Vector3>  	vertex = 		new List<Vector3>();
	List<Vector2>  	uv = 			new List<Vector2>();
	List<Vector3> 	normal = 		new List<Vector3>();
	List<Vector4>  	tangent = 		new List<Vector4>();
	List<int>[]  	triangle = 		new List<int>[2];
	List<Vector2>  	atlasCoord = 	new List<Vector2>();
	List<Color32> 	lightning = 	new List<Color32>();
	
	List<Vector3>  	vertexCollider = 		new List<Vector3>();
	List<int>	  	triangleCollider = 		new List<int>();
	List<Vector3> 	normalCollider = 		new List<Vector3>();
	
	//neighbours reference
	public byte 	numberRef = 0; // 0 0 X+ X- Y+ Y- Z+ Z-
	Chunk	YpRef;
	Chunk	YmRef;
	Chunk	XpRef;
	Chunk	XmRef;
	Chunk	ZpRef;
	Chunk	ZmRef;
	public byte	numberLoadedVoxel = 0;
	public byte	numberLighten = 0;
	
	void Awake()
	{	
		cachedTransform = transform;
		meshRenderer =	gameObject.AddComponent(typeof(MeshRenderer)) 	as MeshRenderer;
		meshFilter = 	gameObject.AddComponent(typeof(MeshFilter)) 	as MeshFilter;
		meshCollider = 	gameObject.AddComponent(typeof(MeshCollider)) 	as MeshCollider;
			
		mesh = new Mesh();
		meshColliderGeometry = new Mesh();
		gameObject.tag = "Chunk";
		
		gameObject.name = "Chunk" + chunkID++;
		
		renderer.sharedMaterials = ChunkManager.Singleton.materials;
		
		for(int i = 0; i < triangle.Length; i++)
			triangle[i] = new List<int>();
		
		meshRenderer.enabled = false;
	}
	
	public void LoadVoxel()
	{
		ChunkManager.Singleton.mutexLightenList.WaitOne();
		
		if (state != State.initialised)
			Debug.LogError( "LoadVoxel:" + state);
		if (numberRef != 0x3f)
			Debug.LogError( numberRef);
		
		if ((XpRef.numberLoadedVoxel & 16) != 0)
			Debug.LogError( "loadVoxel error 16");
		XpRef.numberLoadedVoxel |= 16;
		if (XpRef.numberLoadedVoxel == 0x3f)
			ChunkManager.Singleton.lightenList.Add(XpRef);
		
		if ((XmRef.numberLoadedVoxel & 32) != 0)
			Debug.LogError( "loadVoxel error 32");
		XmRef.numberLoadedVoxel |= 32;
		if (XmRef.numberLoadedVoxel == 0x3f)
			ChunkManager.Singleton.lightenList.Add(XmRef);
		
		if ((YpRef.numberLoadedVoxel & 4) != 0)
			Debug.LogError( "loadVoxel error 4");
		YpRef.numberLoadedVoxel |= 4;
		if (YpRef.numberLoadedVoxel == 0x3f)
			ChunkManager.Singleton.lightenList.Add(YpRef);
		
		if ((YmRef.numberLoadedVoxel & 8) != 0)
			Debug.LogError("loadVoxel error 8");
		YmRef.numberLoadedVoxel |= 8;
		if (YmRef.numberLoadedVoxel == 0x3f)
			ChunkManager.Singleton.lightenList.Add(YmRef);
	
		if ((ZpRef.numberLoadedVoxel & 1) != 0)
			Debug.LogError("loadVoxel error 1");
		ZpRef.numberLoadedVoxel |= 1;
		if (ZpRef.numberLoadedVoxel == 0x3f)
			ChunkManager.Singleton.lightenList.Add(ZpRef);
		
		if ((ZmRef.numberLoadedVoxel & 2) != 0)
			Debug.LogError("loadVoxel error 2");
		ZmRef.numberLoadedVoxel |= 2;
		if (ZmRef.numberLoadedVoxel == 0x3f)
			ChunkManager.Singleton.lightenList.Add(ZmRef);
		
		if (numberLoadedVoxel == 0x3f)
			ChunkManager.Singleton.lightenList.Add(this);
		
		ChunkManager.Singleton.mutexLightenList.ReleaseMutex();
		
		voxels = generateVoxels();
		state = State.voxelLoaded;
	}
	
	public void Lighten()
	{
		ChunkManager.Singleton.mutexLoadMeshList.WaitOne();
		
		if (state != State.voxelLoaded)
			Debug.LogError( "LoadVoxel:" + state);
		if (numberRef != 0x3f)
			Debug.LogError( numberRef);
		
		if ((XpRef.numberLighten & 16) != 0)
			Debug.LogError( "lighten error 16");
		XpRef.numberLighten |= 16;
		if (XpRef.numberLighten == 0x3f)
			ChunkManager.Singleton.loadMeshList.Add(XpRef);
		
		if ((XmRef.numberLighten & 32) != 0)
			Debug.LogError( "lighten error 32");
		XmRef.numberLighten |= 32;
		if (XmRef.numberLighten == 0x3f)
			ChunkManager.Singleton.loadMeshList.Add(XmRef);
		
		if ((YpRef.numberLighten & 4) != 0)
			Debug.LogError( "lighten error 4");
		YpRef.numberLighten |= 4;
		if (YpRef.numberLighten == 0x3f)
			ChunkManager.Singleton.loadMeshList.Add(YpRef);
		
		if ((YmRef.numberLighten & 8) != 0)
			Debug.LogError("lighten error 8");
		YmRef.numberLighten |= 8;
		if (YmRef.numberLighten == 0x3f)
			ChunkManager.Singleton.loadMeshList.Add(YmRef);
	
		if ((ZpRef.numberLighten & 1) != 0)
			Debug.LogError("lighten error 1");
		ZpRef.numberLighten |= 1;
		if (ZpRef.numberLighten == 0x3f)
			ChunkManager.Singleton.loadMeshList.Add(ZpRef);
		
		if ((ZmRef.numberLighten & 2) != 0)
			Debug.LogError("lighten error 2");
		ZmRef.numberLighten |= 2;
		if (ZmRef.numberLighten == 0x3f)
			ChunkManager.Singleton.loadMeshList.Add(ZmRef);
		
		if (numberLighten == 0x3f)
			ChunkManager.Singleton.loadMeshList.Add(this);
		
		ChunkManager.Singleton.mutexLoadMeshList.ReleaseMutex();
		
		computeLightning();
		state = State.lighted;
	}
	
	public void LoadMesh()
	{
		if (state != State.lighted)
			Debug.LogError( "LoadMesh:" + state);
		if (numberRef != 0x3f)
			Debug.LogError( numberRef);
		
		BuildMesh();
		BuildMeshCollider();
		
		ChunkManager.Singleton.mutexLinkMeshList.WaitOne();
		ChunkManager.Singleton.linkMeshList.Add(this);
		ChunkManager.Singleton.mutexLinkMeshList.ReleaseMutex();
		
		state = State.meshGenerated;
	}
	
	public void initializeLink()
	{
		if (state != State.disposed)
			Debug.LogError("initializeLink:" + state);
		
		position = cachedTransform.position;
		numberRef = 0;
		numberLoadedVoxel = 0;

		XpRef = ChunkManager.Singleton.getChunkAtPosition(new Vector3(position.x + ChunkManager.chunkSize, position.y, position.z));
		if (XpRef != null) 
		{
			if ((XpRef.numberRef & 16) != 0)
				Debug.Log( position + "---" + XpRef.position);
			
			if (ChunkManager.Singleton.loadVoxelList.Contains(XpRef))
				Debug.Log("double loadVoxelList XpRef " + gameObject.name);
			
			XpRef.XmRef = this;
			numberRef |= 32;
			XpRef.numberRef |= 16;
			if (XpRef.numberRef == 0x3f && XpRef.state == State.initialised)
			{
				ChunkManager.Singleton.loadVoxelList.Add(XpRef);
			}
		}
	
		XmRef = ChunkManager.Singleton.getChunkAtPosition(new Vector3(position.x - ChunkManager.chunkSize, position.y, position.z));
		if (XmRef != null) 
		{
			if ((XmRef.numberRef & 32) != 0)
				Debug.Log( position + "---" + XmRef.position);
			
			if (ChunkManager.Singleton.loadVoxelList.Contains(XmRef))
				Debug.Log("double loadVoxelList XmRef " + gameObject.name);
			
			XmRef.XpRef = this;
			numberRef |= 16;
			XmRef.numberRef |= 32;
			if (XmRef.numberRef == 0x3f && XmRef.state == State.initialised)
			{
				ChunkManager.Singleton.loadVoxelList.Add(XmRef);
			}
		}
	
		YpRef = ChunkManager.Singleton.getChunkAtPosition(new Vector3(position.x, position.y + ChunkManager.chunkSize, position.z));
		if (YpRef != null) 
		{
			if ((YpRef.numberRef & 4) != 0)
				Debug.Log(position + "---" + XpRef.position);
			
			if (ChunkManager.Singleton.loadVoxelList.Contains(YpRef))
				Debug.Log("double loadVoxelList YpRef " + gameObject.name);
			
			YpRef.YmRef = this;
			numberRef |= 8;
			YpRef.numberRef |= 4;
			if (YpRef.numberRef == 0x3f && YpRef.state == State.initialised)
			{
				ChunkManager.Singleton.loadVoxelList.Add(YpRef);
			}		
		}
	
		YmRef = ChunkManager.Singleton.getChunkAtPosition(new Vector3(position.x, position.y - ChunkManager.chunkSize, position.z));
		if (YmRef != null)
		{
			if ((YmRef.numberRef & 8) != 0)
				Debug.Log( position + "---" + YmRef.position);

			if (ChunkManager.Singleton.loadVoxelList.Contains(YmRef))
				Debug.Log("double loadVoxelList YmRef " + gameObject.name);
			
			YmRef.YpRef = this;
			numberRef |= 4;
			YmRef.numberRef |= 8;
			if (YmRef.numberRef == 0x3f && YmRef.state == State.initialised)
			{
				ChunkManager.Singleton.loadVoxelList.Add(YmRef);
			}		
		}
	
		ZpRef = ChunkManager.Singleton.getChunkAtPosition(new Vector3(position.x, position.y, position.z + ChunkManager.chunkSize));
		if (ZpRef != null) 
		{
			if ((ZpRef.numberRef & 1) != 0)
				Debug.Log(position + "---" + ZpRef.position);

			if (ChunkManager.Singleton.loadVoxelList.Contains(ZpRef))
				Debug.Log("double loadVoxelList ZpRef " + gameObject.name);
			
			ZpRef.ZmRef = this;
			numberRef |= 2;
			ZpRef.numberRef |= 1;
			if (ZpRef.numberRef == 0x3f && ZpRef.state == State.initialised)
			{
				ChunkManager.Singleton.loadVoxelList.Add(ZpRef);
			}		
		}
	
		ZmRef = ChunkManager.Singleton.getChunkAtPosition(new Vector3(position.x, position.y, position.z - ChunkManager.chunkSize));
		if (ZmRef != null)
		{
			if ((ZmRef.numberRef  & 2) != 0)
				Debug.Log(position + "---" + ZmRef.position);
			
			if (ChunkManager.Singleton.loadVoxelList.Contains(ZmRef))
				Debug.Log("double loadVoxelList ZmRef " + gameObject.name);
			
			ZmRef.ZpRef = this;
			numberRef |= 1;
			ZmRef.numberRef |= 2;
			
			if (ZmRef.numberRef == 0x3f && ZmRef.state == State.initialised)
			{
				ChunkManager.Singleton.loadVoxelList.Add(ZmRef);
			}		
		}
		
		if (numberRef == 0x3f)
		{
			ChunkManager.Singleton.loadVoxelList.Add(this);
			Debug.Log("init to loadvoxel");
		}
		
		state = State.initialised;
	}
	
	public void Dispose()
	{
		state = State.disposed;
		
		if (voxels != null)
		{
			voxelsPool.Push(voxels);
			voxels = null;
		}
		
		numberLoadedVoxel = 0;
		numberRef = 0;
		
		ChunkManager.Singleton.mutexLoadMeshList.WaitOne();
		ChunkManager.Singleton.loadMeshList.Remove(XpRef);
		ChunkManager.Singleton.loadMeshList.Remove(XmRef);
		ChunkManager.Singleton.loadMeshList.Remove(XpRef);
		ChunkManager.Singleton.loadMeshList.Remove(YmRef);
		ChunkManager.Singleton.loadMeshList.Remove(ZpRef);
		ChunkManager.Singleton.loadMeshList.Remove(ZmRef);
		ChunkManager.Singleton.mutexLoadMeshList.ReleaseMutex();
		
		ChunkManager.Singleton.loadVoxelList.Remove(XpRef);
		ChunkManager.Singleton.loadVoxelList.Remove(XmRef);
		ChunkManager.Singleton.loadVoxelList.Remove(XpRef);
		ChunkManager.Singleton.loadVoxelList.Remove(YmRef);
		ChunkManager.Singleton.loadVoxelList.Remove(ZpRef);
		ChunkManager.Singleton.loadVoxelList.Remove(ZmRef);
		
		if (XpRef != null) { XpRef.XmRef = null; XpRef.numberRef &= 0x3f - 16; XpRef.numberLoadedVoxel &= 0x3f - 16; }
		if (XmRef != null) { XmRef.XpRef = null; XmRef.numberRef &= 0x3f - 32; XmRef.numberLoadedVoxel &= 0x3f - 32; }
		if (YpRef != null) { YpRef.YmRef = null; YpRef.numberRef &= 0x3f - 4; YpRef.numberLoadedVoxel &= 0x3f - 4; }
		if (YmRef != null) { YmRef.YpRef = null; YmRef.numberRef &= 0x3f - 8; YmRef.numberLoadedVoxel &= 0x3f - 8; }
		if (ZpRef != null) { ZpRef.ZmRef = null; ZpRef.numberRef &= 0x3f - 1; ZpRef.numberLoadedVoxel &= 0x3f - 1; }
		if (ZmRef != null) { ZmRef.ZpRef = null; ZmRef.numberRef &= 0x3f - 2; ZmRef.numberLoadedVoxel &= 0x3f - 2; }
		
		XpRef = null;
		XmRef = null;
		YpRef = null;
		YmRef = null;
		ZpRef = null;
		ZmRef = null;
		
		voxelsEnabledCount = 0;
		meshRenderer.enabled = false;
		mesh.Clear();
		ChunkManager.chunkPool.Push(this);
	}
	
	void OnDestroy()
	{
		if (voxels != null)
			voxelsPool.Push(voxels);
		mesh.Clear();
	}
	
	#region voxel modification
	
	public bool Contains(int x, int y, int z)
	{
		x -= (int)(position.x / ChunkManager.size);
		y -= (int)(position.y / ChunkManager.size);
		z -= (int)(position.z / ChunkManager.size);
		
		return x >= 0 && x < ChunkManager.chunkSize &&
			   y >= 0 && y < ChunkManager.chunkSize &&
			   z >= 0 && z < ChunkManager.chunkSize;
	}
	
	void unlit()
	{
		state = State.voxelLoaded;
		ChunkManager.Singleton.lightenList.Add(this);
		
		XmRef.numberLighten ^= 32;
		YmRef.numberLighten ^= 8;
		ZmRef.numberLighten ^= 2;
		XpRef.numberLighten ^= 16;
		YpRef.numberLighten ^= 4;
		ZpRef.numberLighten ^= 1;
	}
	
	void voxelChanged(int x, int y, int z)
	{
		ChunkManager.Singleton.mutexLightenList.WaitOne();
		unlit();
		if (x == 0 && XmRef.state == State.meshGenerated) XmRef.unlit();
		if (y == 0 && YmRef.state == State.meshGenerated) YmRef.unlit();
		if (z == 0 && ZmRef.state == State.meshGenerated) ZmRef.unlit();
		if (x == ChunkManager.size - 1 && XpRef.state == State.meshGenerated) XpRef.unlit();
		if (y == ChunkManager.size - 1 && YpRef.state == State.meshGenerated) YpRef.unlit();
		if (z == ChunkManager.size - 1 && ZpRef.state == State.meshGenerated) ZpRef.unlit();
		
		ChunkManager.Singleton.mutexLightenList.ReleaseMutex();
	}
	
	public void RemoveBlock(int x, int y, int z)
	{
		//Debug.Log(x + " " + y + " " + z);
		if (Contains(x, y, z))
		{
			x -= (int)(position.x / ChunkManager.size);
			y -= (int)(position.y / ChunkManager.size);
			z -= (int)(position.z / ChunkManager.size);
			
			voxels[x, y, z].type = (byte)VoxelType.air;
			voxelChanged(x, y, z);
		}
		else
			ChunkManager.Singleton.RemoveBlock(x, y, z);
	}
	
	public void AddBlock(int x, int y, int z)
	{
		//Debug.Log(x + " " + y + " " + z);
		if (Contains(x, y, z))
		{
			x -= (int)(position.x / ChunkManager.size);
			y -= (int)(position.y / ChunkManager.size);
			z -= (int)(position.z / ChunkManager.size);
			
			if (voxels == null)
				voxels = getClearVoxelsFromPool();
			
			voxels[x, y, z].type = (byte)VoxelType.wood;
			voxelChanged(x, y, z);
		}
		else
			ChunkManager.Singleton.AddBlock(x, y, z);
	}
	
	#endregion
	
	#region voxels generation
	
	public static float ComputeHeight(float x, float z)
	{
		float dx, dy;
		float n1, n2, n3, n4, n5;
		
		n1 = SimplexNoise.sdnoise2(x / 1024, z / 1024, out dx, out dy) * 64;
		n2 = -SimplexNoise.sdnoise2(x / 512, z / 512, out dx, out dy) * 32;
		n3 = SimplexNoise.sdnoise2(x / 256, z / 256, out dx, out dy) * 16;
		n4 = -SimplexNoise.sdnoise2(x / 128, z / 128, out dx, out dy) * 8;
		n5 = SimplexNoise.sdnoise2(x / 64, z / 64, out dx, out dy) * 4;
		
		return n1 + n2 + n3 + n4 + n5;
	}
	
	Voxel[,,] generateVoxels()
	{
		// malloc
		Voxel[,,] genVoxels = getVoxelsFromPool();
				
		// setup
		for (int x = 0; x < ChunkManager.chunkSize; x++)
			for (int z = 0; z < ChunkManager.chunkSize; z++)
			{
				float heighMapValue = ComputeHeight(position.x + x * ChunkManager.size, position.z + z * ChunkManager.size);	
				for (int y = ChunkManager.chunkSize - 1; y >= 0; y--)
				{
					float blockAltitude = position.y + y * ChunkManager.size;
					float blockRelativeAltitude = blockAltitude + heighMapValue;
				
					if (blockRelativeAltitude > 0)
					{
						if (blockAltitude < 0)
							genVoxels[x, y, z].type = (byte)VoxelType.water;
						else
							genVoxels[x, y, z].type = (byte)VoxelType.airLit;
					}
					else
					{
						if (blockRelativeAltitude > -2)
						{
							if (blockAltitude < 1)
								genVoxels[x, y, z].type = (byte)VoxelType.sand;
							else
								genVoxels[x, y, z].type = (byte)VoxelType.grass;
						}
						else
							genVoxels[x, y, z].type = (byte)VoxelType.rock;
					}
				
					if (genVoxels[x, y, z].type != (byte)VoxelType.air && genVoxels[x, y, z].type != (byte)VoxelType.airLit)
						++voxelsEnabledCount;
				}
			}
		
		// output
		if (voxelsEnabledCount != 0)
			return genVoxels;
		else
		{
			voxelsPool.Push(genVoxels);
			return null;
		}
	}
	#endregion
	
	#region lightning
	
	Color getColorFromShort(short s)
	{
		return new Color32((byte)((s >> 12) * 16), 
							(byte)(((s >> 8) & 0xf) * 16),
							(byte)(((s >> 4) & 0xf) * 16),
							(byte)((s & 0xf) * 16));
	}
	
	void computeLightning()
	{
		if (voxels == null)
			return;
		
		//TODO: diffuse light
		for (int x = 0; x < ChunkManager.chunkSize; x++)
			for (int y = 0; y < ChunkManager.chunkSize; y++)
				for (int z = 0; z < ChunkManager.chunkSize; z++)
					if (isEmitter[voxels[x, y, z].type])
					{					
						voxels[x, y, z].lightning = EmitterColor[voxels[x, y, z].type];
					}
					else
					{
						voxels[x, y, z].lightning = 0;
					}
	}
	
	#endregion
	
	#region mesh generation
	
	static public byte getByteFromCoord(int x, int y)
	{
		return (byte)(y << 4 | x);
	}
	
	void BuildMesh()
	{
		if (voxels != null)
		{
			for (int x = 0; x < ChunkManager.chunkSize; x++)
				for (int y = 0; y < ChunkManager.chunkSize; y++)
					for (int z = 0; z < ChunkManager.chunkSize; z++)
						AppendCube(x, y, z);
		}
	}
	
	void BuildMeshCollider()
	{
		if (voxels != null)
		{
			bool[,,] isGen = new bool[ChunkManager.chunkSize, ChunkManager.chunkSize, ChunkManager.chunkSize];
			for (int x = 0; x < ChunkManager.chunkSize; x++)
				for (int y = 0; y < ChunkManager.chunkSize; y++)
					for (int z = 0; z < ChunkManager.chunkSize; z++)
						if (!isGen[x,y,z] && isSolid[voxels[x, y, z].type])
							AppendFaceCollider(x, y, z, ref isGen);
		}
	}
	
	void AppendFaceCollider(int x, int y, int z, ref bool[,,] isGen)
	{
		Voxel neighbour = (y != ChunkManager.chunkSize - 1 ? voxels[x, y + 1, z] : YpRef.getVoxel(x, 0, z));
		if (isSolid[neighbour.type])
		{
			isGen[x, y, z] = true;
			return;
		}
		
		int faceSizeX = 0;
		int faceSizeZ = 1;
		do
		{
			isGen[x + faceSizeX, y, z] = true;
			++faceSizeX;
			if (x + faceSizeX >= ChunkManager.chunkSize)
				break;
			neighbour = (y != ChunkManager.chunkSize - 1 ? voxels[x + faceSizeX, y + 1, z] : YpRef.getVoxel(x + faceSizeX, 0, z));
		}
		while (!isGen[x + faceSizeX, y, z] && !isSolid[neighbour.type] && isSolid[voxels[x + faceSizeX, y, z].type]);
			
		while (z + faceSizeZ < ChunkManager.chunkSize)
		{
			int faceIteration = 0;
			while (faceIteration <= faceSizeX - 1)
			{
				neighbour = (y != ChunkManager.chunkSize - 1 ? voxels[x + faceIteration, y + 1, z + faceSizeZ] : YpRef.getVoxel(x + faceIteration, 0, z + faceSizeZ));
				if (isGen[x + faceIteration, y, z + faceSizeZ] || isSolid[neighbour.type] || !isSolid[voxels[x + faceIteration, y, z + faceSizeZ].type])
					break;
				++faceIteration;
			}
			if (faceIteration >= faceSizeX)
			{
				for (faceIteration = 0; faceIteration < faceSizeX; ++faceIteration)
					isGen[x + faceIteration, y, z + faceSizeZ] = true;
				++faceSizeZ;
			}
			else
				break;
		}
		
		int n = vertexCollider.Count;
		//vertex
		vertexCollider.Add(new Vector3((x + faceSizeX) * ChunkManager.size,  (y + 1) * ChunkManager.size,       z * ChunkManager.size));
		vertexCollider.Add(new Vector3((x + faceSizeX) * ChunkManager.size,  (y + 1) * ChunkManager.size, (z + faceSizeZ) * ChunkManager.size));
		vertexCollider.Add(new Vector3(      x * ChunkManager.size,  (y + 1) * ChunkManager.size, (z + faceSizeZ) * ChunkManager.size));
		vertexCollider.Add(new Vector3(      x * ChunkManager.size,  (y + 1) * ChunkManager.size,       z * ChunkManager.size));
		
		//triangle
		triangleCollider.Add(3 + n);  triangleCollider.Add(1 + n);  triangleCollider.Add(0 + n);
		triangleCollider.Add(3 + n);  triangleCollider.Add(2 + n);  triangleCollider.Add(1 + n);
		
		//normal
		normalCollider.Add(new Vector3(0, 1, 0));
		normalCollider.Add(new Vector3(0, 1, 0));
		normalCollider.Add(new Vector3(0, 1, 0));
		normalCollider.Add(new Vector3(0, 1, 0));
	}
	
	// link the mesh to unity from the main thread once generated
	public void linkMesh()
	{
		mesh.Clear();
		meshColliderGeometry.Clear();
		meshFilter.sharedMesh = mesh;
		meshCollider.sharedMesh = null;
		
		Debug.Log(vertex.Count + " " + atlasCoord.Count + " " + lightning.Count + " " + normal.Count + " " + lightning.Count + " " + tangent.Count + " " + uv.Count);
		
		if (voxels != null)
		{
			mesh.subMeshCount = triangle.Length;
			
			mesh.vertices = vertex.ToArray();
			mesh.uv = uv.ToArray();
			for (int i = 0; i < triangle.Length; i++)
				mesh.SetTriangles(triangle[i].ToArray(), i);
			mesh.normals = normal.ToArray();
			mesh.uv2 = atlasCoord.ToArray();
			mesh.tangents = tangent.ToArray();
			mesh.colors32 = lightning.ToArray();
			
			meshColliderGeometry.vertices = vertexCollider.ToArray();
			meshColliderGeometry.triangles = triangleCollider.ToArray();
			meshColliderGeometry.normals = normalCollider.ToArray();
			
			lightning.Clear();
			vertex.Clear();
			uv.Clear();
			normal.Clear();
			foreach(List<int> t in triangle)
				t.Clear();
			atlasCoord.Clear();
			tangent.Clear();
			lightning.Clear();
			
			vertexCollider.Clear();
			triangleCollider.Clear();
			normalCollider.Clear();
			
			meshCollider.sharedMesh = meshColliderGeometry;
			meshRenderer.enabled = true;
		}
		else
		{
			meshRenderer.enabled = false;
		}
	}
	
	public Voxel getVoxel(int x, int y, int z)
	{
		return voxels == null ? new Voxel(0,0) : voxels[x, y, z];
	}
	
	public Voxel getVoxelRecursive(int x, int y, int z)
	{
		if (x >= ChunkManager.chunkSize) return XpRef.getVoxelRecursive(0, y, z);
		if (y >= ChunkManager.chunkSize) return YpRef.getVoxelRecursive(x, 0, z);
		if (z >= ChunkManager.chunkSize) return ZpRef.getVoxelRecursive(x, y, 0);
		if (x <= -1) return XmRef.getVoxelRecursive(ChunkManager.chunkSize - 1, y, z);
		if (y <= -1) return YmRef.getVoxelRecursive(x, ChunkManager.chunkSize - 1, z);
		if (z <= -1) return ZmRef.getVoxelRecursive(x, y, ChunkManager.chunkSize - 1);
		return voxels == null ? new Voxel(0,0) : voxels[x, y, z];
	}
	
	void AppendCube(int x, int y, int z)
	{
		if (voxels[x, y, z].type == (byte)VoxelType.air || voxels[x, y, z].type == (byte)VoxelType.airLit)
			return;
		
		byte coord = lookupCoord[voxels[x, y, z].type];
		Voxel neighbour;
		Color[] voxelColors = new Color[9];
		
		// -X WEST
		neighbour = (x != 0 ? voxels[x - 1, y, z] : XmRef.getVoxel(ChunkManager.chunkSize - 1, y, z));
		if (istransparent[neighbour.type] && neighbour.type != voxels[x, y, z].type)
		{
			int n = vertex.Count;
			//vertex
			vertex.Add(new Vector3(x * ChunkManager.size,  (y + 1) * ChunkManager.size,       z * ChunkManager.size));
			vertex.Add(new Vector3(x * ChunkManager.size,  (y + 1) * ChunkManager.size, (z + 1) * ChunkManager.size));
			vertex.Add(new Vector3(x * ChunkManager.size,        y * ChunkManager.size, (z + 1) * ChunkManager.size));
			vertex.Add(new Vector3(x * ChunkManager.size,        y * ChunkManager.size,       z * ChunkManager.size));
			
			//lightning
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			
			//normal
			normal.Add(new Vector3(-1, 0, 0));
			normal.Add(new Vector3(-1, 0, 0));
			normal.Add(new Vector3(-1, 0, 0));
			normal.Add(new Vector3(-1, 0, 0));
			
			//tangent
			tangent.Add(new Vector4( 0,  0, 1, 1));
			tangent.Add(new Vector4( 0,  0, 1, 1));
			tangent.Add(new Vector4( 0,  0, 1, 1));
			tangent.Add(new Vector4( 0,  0, 1, 1));
			
			//UV
			uv.Add(new Vector2(1.0f,  1.0f));
			uv.Add(new Vector2(0.0f,  1.0f));
			uv.Add(new Vector2(0.1f,  0.1f));
			uv.Add(new Vector2(1.0f,  0.0f));
						
			//Atlas texture number
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));

			//triangle
			int submeshID = submesh[voxels[x, y, z].type];
			triangle[submeshID].Add(3 + n);  triangle[submeshID].Add(1 + n);  triangle[submeshID].Add(0 + n);
			triangle[submeshID].Add(3 + n);  triangle[submeshID].Add(2 + n);  triangle[submeshID].Add(1 + n);
		}
		
		// +X EAST
		neighbour = (x != ChunkManager.chunkSize - 1 ? voxels[x + 1, y, z] : XpRef.getVoxel(0, y, z));
		if (istransparent[neighbour.type] && neighbour.type != voxels[x, y, z].type)
		{
			int n = vertex.Count;
			//vertex
			vertex.Add(new Vector3((x + 1) * ChunkManager.size,  (y + 1) * ChunkManager.size,       z * ChunkManager.size));
			vertex.Add(new Vector3((x + 1) * ChunkManager.size,  (y + 1) * ChunkManager.size, (z + 1) * ChunkManager.size));
			vertex.Add(new Vector3((x + 1) * ChunkManager.size,        y * ChunkManager.size, (z + 1) * ChunkManager.size));
			vertex.Add(new Vector3((x + 1) * ChunkManager.size,        y * ChunkManager.size,       z * ChunkManager.size));
			
			//lightning
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			
			//normal
			normal.Add(new Vector3(1, 0, 0));
			normal.Add(new Vector3(1, 0, 0));
			normal.Add(new Vector3(1, 0, 0));
			normal.Add(new Vector3(1, 0, 0));
			
			//tangent
			tangent.Add(new Vector4( 0,  0, -1, 1));
			tangent.Add(new Vector4( 0,  0, -1, 1));
			tangent.Add(new Vector4( 0,  0, -1, 1));
			tangent.Add(new Vector4( 0,  0, -1, 1));
			
			//UV
			uv.Add(new Vector2(1.0f,  1.0f));
			uv.Add(new Vector2(0.0f,  1.0f));
			uv.Add(new Vector2(0.1f,  0.1f));
			uv.Add(new Vector2(1.0f,  0.0f));
			
			//Atlas texture number
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			
			//triangle
			int submeshID = submesh[voxels[x, y, z].type];
			triangle[submeshID].Add(0 + n);  triangle[submeshID].Add(1 + n);  triangle[submeshID].Add(3 + n);
			triangle[submeshID].Add(1 + n);  triangle[submeshID].Add(2 + n);  triangle[submeshID].Add(3 + n);
		}
		
		// -Y BOTTOM
		neighbour = (y != 0 ? voxels[x, y - 1, z] : YmRef.getVoxel(x, ChunkManager.chunkSize - 1, z));
		if (istransparent[neighbour.type] && neighbour.type != voxels[x, y, z].type)
		{
			int n = vertex.Count;
			//vertex
			vertex.Add(new Vector3((x + 1) * ChunkManager.size, y * ChunkManager.size,       z * ChunkManager.size));
			vertex.Add(new Vector3((x + 1) * ChunkManager.size, y * ChunkManager.size, (z + 1) * ChunkManager.size));
			vertex.Add(new Vector3(      x * ChunkManager.size, y * ChunkManager.size, (z + 1) * ChunkManager.size));
			vertex.Add(new Vector3(      x * ChunkManager.size, y * ChunkManager.size,       z * ChunkManager.size));
			
			//lightning
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			
			//normal
			normal.Add(new Vector3(0, -1, 0));
			normal.Add(new Vector3(0, -1, 0));
			normal.Add(new Vector3(0, -1, 0));
			normal.Add(new Vector3(0, -1, 0));
			
			//tangent
			tangent.Add(new Vector4(-1,  0,  0, 1));
			tangent.Add(new Vector4(-1,  0,  0, 1));
			tangent.Add(new Vector4(-1,  0,  0, 1));
			tangent.Add(new Vector4(-1,  0,  0, 1));
			
			//UV
			uv.Add(new Vector2(1.0f,  0.0f));
			uv.Add(new Vector2(1.0f,  1.0f));
			uv.Add(new Vector2(0.0f,  1.0f));
			uv.Add(new Vector2(0.1f,  0.1f));
			
			//Atlas texture number
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			
			//triangle
			int submeshID = submesh[voxels[x, y, z].type];
			triangle[submeshID].Add(0 + n);  triangle[submeshID].Add(1 + n);  triangle[submeshID].Add(3 + n);
			triangle[submeshID].Add(1 + n);  triangle[submeshID].Add(2 + n);  triangle[submeshID].Add(3 + n);
		}
		
		// +Y TOP
		neighbour = (y != ChunkManager.chunkSize - 1 ? voxels[x, y + 1, z] : YpRef.getVoxel(x, 0, z));
		if (istransparent[neighbour.type] && neighbour.type != voxels[x, y, z].type)
		{
			int n = vertex.Count;
			//vertex
			vertex.Add(new Vector3((x + 1) * ChunkManager.size,  (y + 1) * ChunkManager.size,       z * ChunkManager.size));
			vertex.Add(new Vector3((x + 1) * ChunkManager.size,  (y + 1) * ChunkManager.size, (z + 1) * ChunkManager.size));
			vertex.Add(new Vector3(      x * ChunkManager.size,  (y + 1) * ChunkManager.size, (z + 1) * ChunkManager.size));
			vertex.Add(new Vector3(      x * ChunkManager.size,  (y + 1) * ChunkManager.size,       z * ChunkManager.size));
			
			//lightning
			voxelColors[0] = getColorFromShort(neighbour.lightning);
			voxelColors[1] = getColorFromShort(getVoxelRecursive(x - 1, y + 1, z + 1).lightning);
			voxelColors[2] = getColorFromShort(getVoxelRecursive(x, y + 1, z + 1).lightning);
			voxelColors[3] = getColorFromShort(getVoxelRecursive(x + 1, y + 1, z + 1).lightning);
			voxelColors[4] = getColorFromShort(getVoxelRecursive(x - 1, y + 1, z).lightning);
			voxelColors[5] = getColorFromShort(getVoxelRecursive(x + 1, y + 1, z).lightning);
			voxelColors[6] = getColorFromShort(getVoxelRecursive(x - 1, y + 1, z - 1).lightning);
			voxelColors[7] = getColorFromShort(getVoxelRecursive(x, y + 1, z - 1).lightning);
			voxelColors[8] = getColorFromShort(getVoxelRecursive(x + 1, y + 1, z - 1).lightning);
			
			lightning.Add((voxelColors[0] + voxelColors[5] + voxelColors[7] + voxelColors[8]) / 4);
			lightning.Add((voxelColors[0] + voxelColors[2] + voxelColors[3] + voxelColors[5]) / 4);
			lightning.Add((voxelColors[0] + voxelColors[1] + voxelColors[2] + voxelColors[4]) / 4);
			lightning.Add((voxelColors[0] + voxelColors[4] + voxelColors[6] + voxelColors[7]) / 4);
			
			//normal
			normal.Add(new Vector3(0, 1, 0));
			normal.Add(new Vector3(0, 1, 0));
			normal.Add(new Vector3(0, 1, 0));
			normal.Add(new Vector3(0, 1, 0));
			
			//tangent
			tangent.Add(new Vector4(1,  0,  0, 1));
			tangent.Add(new Vector4(1,  0,  0, 1));
			tangent.Add(new Vector4(1,  0,  0, 1));
			tangent.Add(new Vector4(1,  0,  0, 1));
			
			//UV
			uv.Add(new Vector2(1.0f,  0.0f));
			uv.Add(new Vector2(1.0f,  1.0f));
			uv.Add(new Vector2(0.0f,  1.0f));
			uv.Add(new Vector2(0.0f,  0.0f));
			
			//Atlas texture number
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));;
			
			//triangle
			int submeshID = submesh[voxels[x, y, z].type];
			triangle[submeshID].Add(3 + n);  triangle[submeshID].Add(1 + n);  triangle[submeshID].Add(0 + n);
			triangle[submeshID].Add(3 + n);  triangle[submeshID].Add(2 + n);  triangle[submeshID].Add(1 + n);
		}
		
		// -Z SOUTH
		neighbour = (z != 0 ? voxels[x, y, z - 1] : ZmRef.getVoxel(x, y, ChunkManager.chunkSize - 1));
		if (istransparent[neighbour.type] && neighbour.type != voxels[x, y, z].type)
		{
			int n = vertex.Count;
			//vertex
			vertex.Add(new Vector3((x + 1) * ChunkManager.size,       y * ChunkManager.size,     z * ChunkManager.size));
			vertex.Add(new Vector3((x + 1) * ChunkManager.size, (y + 1) * ChunkManager.size,     z * ChunkManager.size));
			vertex.Add(new Vector3(      x * ChunkManager.size, (y + 1) * ChunkManager.size,     z * ChunkManager.size));
			vertex.Add(new Vector3(      x * ChunkManager.size,       y * ChunkManager.size,     z * ChunkManager.size));
			
			//lightning
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			
			//normal
			normal.Add(new Vector3(0, 0, -1));
			normal.Add(new Vector3(0, 0, -1));
			normal.Add(new Vector3(0, 0, -1));
			normal.Add(new Vector3(0, 0, -1));
			
			//tangent
			tangent.Add(new Vector4(1,  0,  0, 1));
			tangent.Add(new Vector4(1,  0,  0, 1));
			tangent.Add(new Vector4(1,  0,  0, 1));
			tangent.Add(new Vector4(1,  0,  0, 1));
			
			//UV
			uv.Add(new Vector2(1.0f,  0.0f));
			uv.Add(new Vector2(1.0f,  1.0f));
			uv.Add(new Vector2(0.0f,  1.0f));
			uv.Add(new Vector2(0.0f,  0.0f));
			
			//Atlas texture number
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			
			//triangle
			int submeshID = submesh[voxels[x, y, z].type];
			triangle[submeshID].Add(3 + n);  triangle[submeshID].Add(1 + n);  triangle[submeshID].Add(0 + n);
			triangle[submeshID].Add(3 + n);  triangle[submeshID].Add(2 + n);  triangle[submeshID].Add(1 + n);
		}
		
		// +Z NORTH
		neighbour = (z != ChunkManager.chunkSize - 1 ? voxels[x, y, z + 1] : ZpRef.getVoxel(x, y, 0));
		if (istransparent[neighbour.type] && neighbour.type != voxels[x, y, z].type)
		{
			int n = vertex.Count;
			//vertex
			vertex.Add(new Vector3((x + 1) * ChunkManager.size,        y * ChunkManager.size, (z + 1) * ChunkManager.size));
			vertex.Add(new Vector3((x + 1) * ChunkManager.size,  (y + 1) * ChunkManager.size, (z + 1) * ChunkManager.size));
			vertex.Add(new Vector3(      x * ChunkManager.size,  (y + 1) * ChunkManager.size, (z + 1) * ChunkManager.size));
			vertex.Add(new Vector3(      x * ChunkManager.size,        y * ChunkManager.size, (z + 1) * ChunkManager.size));
			
			//lightning
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			lightning.Add(new Color32(0,0,0,0));
			
			//normal
			normal.Add(new Vector3(0, 1, 0));
			normal.Add(new Vector3(0, 1, 0));
			normal.Add(new Vector3(0, 1, 0));
			normal.Add(new Vector3(0, 1, 0));
			
			//tangent
			tangent.Add(new Vector4(-1,  0,  0, 1));
			tangent.Add(new Vector4(-1,  0,  0, 1));
			tangent.Add(new Vector4(-1,  0,  0, 1));
			tangent.Add(new Vector4(-1,  0,  0, 1));
			
			//UV
			uv.Add(new Vector2(1.0f,  0.0f));
			uv.Add(new Vector2(1.0f,  1.0f));
			uv.Add(new Vector2(0.0f,  1.0f));
			uv.Add(new Vector2(0.0f,  0.0f));
			
			//Atlas texture number
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			atlasCoord.Add(new Vector2((byte)(coord >> 4), (byte)(coord & 0xf)));
			
			//triangle
			int submeshID = submesh[voxels[x, y, z].type];
			triangle[submeshID].Add(0 + n);  triangle[submeshID].Add(1 + n);  triangle[submeshID].Add(3 + n);
			triangle[submeshID].Add(1 + n);  triangle[submeshID].Add(2 + n);  triangle[submeshID].Add(3 + n);
		}
	}
	
	#endregion
	
	#region voxel pool
	
	static Stack<Voxel[,,]> voxelsPool = new Stack<Voxel[,,]>();
	
	static Voxel[,,] getVoxelsFromPool()
	{
		return voxelsPool.Count == 0 ? 
				new Voxel[ChunkManager.chunkSize, ChunkManager.chunkSize, ChunkManager.chunkSize] :
				voxelsPool.Pop();
	}
	
	public static void FillPool()
	{
		while (voxelsPool.Count < ChunkManager.poolStart)
			voxelsPool.Push(new Voxel[ChunkManager.chunkSize, ChunkManager.chunkSize, ChunkManager.chunkSize]);
	}
	
	static Voxel[,,] getClearVoxelsFromPool()
	{
		if (voxelsPool.Count == 0)
			return new Voxel[ChunkManager.chunkSize, ChunkManager.chunkSize, ChunkManager.chunkSize];
		
		Voxel[,,] vs = voxelsPool.Pop();
		
		for (int x = 0; x < ChunkManager.chunkSize; x++)
			for (int y = 0; y < ChunkManager.chunkSize; y++)
				for (int z = 0; z < ChunkManager.chunkSize; z++)
				{
					vs[x,y,z].type = 0;
					vs[x,y,z].data = 0;
				}
		return vs;
	}
	
	#endregion
	
	#region lookup tables
	
	static bool[] istransparent =
	{
		true,		// air
		true,		// airLit
		false,		// grass
		false,		// rock
		true,		// water
		false,		// sand
		false		// wood
	};
	
	static int[] submesh =
	{
		-1,			// air - ignore
		-1,			// airLit - ignore
		0,			// grass
		0,			// rock
		1,			// water
		0,			// sand
		0			// wood
	};
	
	static bool[] isSolid =
	{
		false,		// air
		false,		// airLit
		true,		// grass
		true,		// rock
		false,		// water
		true,		// sand
		true		// wood
	};
	
	static bool[] isEmitter =
	{
		false,		// air
		true,		// airLit
		false,		// grass
		false,		// rock
		false,		// water
		false,		// sand
		false		// wood
	};
	
	// R G B SUN
	static short[] EmitterColor =
	{
		0,			// air - ignore
		(short)((0 << 12) | (0 << 8) | (0 << 4) | 15),			// airLit - 0 0 0 15
		0,			// grass - ignore
		0,			// rock - ignore
		0,			// water - ignore
		0,			// sand - ignore
		0			// wood - ignore
	};
	
	static byte[] lookupCoord = 
	{
		255,		// air - ignore
		255,		// airLit - ignore
		1,			// grass
		0,			// rock
		255,		// water - ignore
		2,			// sand
		16			// wood
	};
	#endregion
}
