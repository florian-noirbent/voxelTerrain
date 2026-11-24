# Voxel Terrain (2013)

An infinite voxel terrain engine I wrote in Unity during my studies at Epitech,
in the summer and fall of 2013. This repository archives the original project
as it was then. It is not maintained.

This snapshot is the later, cube-based version of the engine described in parts 2
and 3. The marching cubes mode shown in part 1 is not part of it.

The engine and its design are explained in three blog posts:

- [Voxel Engine, Part 1: An Infinite Terrain in Unity](https://titan.nofl.uk/blog/voxel-engine-part-1)
- [Voxel Engine, Part 2: Chunks, Transparency and Colliders](https://titan.nofl.uk/blog/voxel-engine-part-2)
- [Voxel Engine, Part 3: Voxel Data and Lighting](https://titan.nofl.uk/blog/voxel-engine-part-3)

Side experiments from the same project are covered in
[An HLSL Simplex Noise Library](https://titan.nofl.uk/blog/hlsl-simplex-noise-library)
and [A DirectX 11 Particle Engine in Unity](https://titan.nofl.uk/blog/dx11-particle-engine).

## What's inside

| Path | Content |
| --- | --- |
| `Assets/Chunk.cs` | A 32×32×32 chunk: voxel storage (4 bytes per voxel), terrain generation, light propagation, cube mesh building with one submesh per material, and colliders built from merged faces |
| `Assets/ChunkManager.cs` | Streams chunks around the player: chunk pool, neighbor links and the generation pipeline states, with worker threads |
| `Assets/FreeCam.cs`, `PointerCam.cs`, `Spawn.cs`, `PlayerWait.cs`, `GameManager.cs` | Camera, voxel editing pointer, player spawn and scene setup |
| `Assets/material/` | Texture atlas shader (with normal maps and the sun/artificial light channels), water shader, atlas textures |
| `Assets/simplex cs/` | Simplex noise in C# and in HLSL (`simplex.hlsl`), with a compute shader test |
| `Assets/computeShaderTest/` | First DirectCompute test from Unity |
| `Assets/particle/` | GPU particle emitter experiment |
| `Assets/thetrahedra/` | Tetrahedral voxel shape experiment (part 3) |
| `Assets/testVoxelEngine.unity` | Main test scene |
| `build/` | The original Unity Web Player build (`VoxelEngine0.1`) and its page |

## Status and limitations

- **Unity version:** the project was made with Unity 4.0.1f2 (Pro, for compute
  shaders and image effects). Current Unity versions cannot open it directly.
- **Asset references:** Unity 4 stored asset IDs in the generated `Library`
  folder, which is not archived here, and scenes use Unity 4's binary format.
  Opening the scenes requires re-linking their scripts and materials. The C#
  and shader sources are the useful part of this archive.
- **Dependencies not included:** Unity's Standard Assets (Character Controller,
  Pro image effects, skybox and water textures) were part of the original
  project and are omitted. `PlayerWait.cs` depends on the Character Controller's
  `CharacterMotor`. `Chunk.cs` imports `UnityEditor`, so it must be cleaned up
  before a player build in recent Unity versions.
- **Web build:** `build/VoxelEngine0.1.unity3d` needs the Unity Web Player
  browser plugin, which no current browser supports. It is kept for the record.

## Third-party code

- `Assets/FreeCam.cs` is based on a free fly-camera script shared in the Unity
  community at the time.
- `Assets/particle/FreeMove.cs` is adapted from Unity's `MouseLook` script.

Everything else was written by me.
