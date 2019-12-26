using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{

    /*
     * Here is the job that calculate 
     * all cube of a chunk.
     */
    [BurstCompile]
    public struct ChunkJob : IJobParallelFor
    {
        // The size of the chunk.
        [ReadOnly]
        public int size;

        // The amplitude of the noise.
        [ReadOnly]
        public float amplitude;

        // The frequency of the noise.
        [ReadOnly]
        public float frequency;

        // The noise position.
        [ReadOnly]
        public float3 position;

        // Noise threshold
        [ReadOnly]
        public float threshold;

        // The cubes of the chunk to write in.
        [WriteOnly]
        public NativeArray<Cube> cubes;

        public void Execute(int index)
        {
            var pos = to3D(index);

            var isDestroy = HaveCube(pos + position, pos);

            if(isDestroy) return;

            cubes[index] = new Cube() 
            {
                localPos = pos,
                isDestroy  = isDestroy,
                zmin = HaveCube(pos + position + new float3( 0,  0, -1), pos + new float3( 0,  0, -1)),
                xmax = HaveCube(pos + position + new float3( 1,  0,  0), pos + new float3( 1,  0,  0)),
                zmax = HaveCube(pos + position + new float3( 0,  0,  1), pos + new float3( 0,  0,  1)),
                xmin = HaveCube(pos + position + new float3(-1,  0,  0), pos + new float3(-1,  0,  0)),
                ymax = HaveCube(pos + position + new float3( 0,  1,  0), pos + new float3( 0,  1,  0)),
                ymin = HaveCube(pos + position + new float3( 0, -1,  0), pos + new float3( 0, -1,  0)),
            };
        }

        private bool HaveCube(float3 at, float3 localPos)
        {
            if(localPos.x < 0 || localPos.y < 0 || localPos.z < 0 || localPos.x > size -1 || localPos.y > size -1 || localPos.z > size -1) 
                return true;
            else
                return (-at.y + noise.snoise(at * frequency) * amplitude) <= threshold;
        }

        private float3 to3D(int i)
        {
            float x = i % size;
            float y = ( i / size ) % size;
            float z = i / ( size * size );

            return new float3(x, y, z);
        }
    }

    // The chunk size (for all dimension).
    public int size;

    // The frequency of the amplitude.
    public float noiseAmplitude;
    // The frequency of the noise.
    public float noiseFrequency;
    // The position of the 3D noise.
    public Vector3 noisePosition;
    // The noise threshold
    public float noiseThreshold;
    // If the mesh need to be update every frame
    public bool updateEveryFrame;
    // If the mesh collider need to be update after mesh's update
    public bool updateCollider;
    // The speed of noise translation.
    public int noiseSpeed;

    // The mesh of the chunk.
    private Mesh mesh;
    // The mesh filter component of the chunk.
    private MeshFilter mf;
    // The mesh collider component of the chunk.
    private MeshCollider mc;

    // The cubes of the chunk.
    private NativeArray<Cube> cubes;

    private bool isKeyPressed;

    private void Start()
    {
        mesh = new Mesh();
        mf = GetComponent<MeshFilter>();
        mc = GetComponent<MeshCollider>();

        mf.mesh = mesh;
        mc.sharedMesh = mesh;

        cubes = new NativeArray<Cube>(size * size * size, Allocator.Persistent);

        RecalculateCubes();
        RecalculateMesh();
    }

    private void OnDestroy()
    {
        if(cubes.IsCreated)
            cubes.Dispose();
    }

    public void SetUpdateEveryFrame(bool x)
    {
        this.updateEveryFrame = x;
    }

    public void SetUpdateCollider(bool x)
    {
        this.updateCollider = x;

        if(x)
        {
            RecalculateMesh();
        }
    }

    public void SetNoiseAmplitude(float amplitude)
    {
        this.noiseAmplitude = amplitude;

        RecalculateCubes();
        RecalculateMesh();
    }

    public void SetNoiseFrequency(float frequency)
    {
        this.noiseFrequency = frequency;

        RecalculateCubes();
        RecalculateMesh();
    }

    /**
     * Calculate / Recalculate the value of all cubes
     * of this chunk.
     */
    public void RecalculateCubes()
    {
        if(cubes.IsCreated)
            cubes.Dispose();
        
        int cubeAmount = size * size * size;
        cubes = new NativeArray<Cube>(cubeAmount, Allocator.Persistent);

        new ChunkJob()
        {
            size = size,
            amplitude = noiseAmplitude,
            frequency = noiseFrequency,
            position  = noisePosition ,
            threshold = noiseThreshold,
            cubes     = cubes,
        }.Schedule(cubeAmount, SystemInfo.processorCount).Complete();
    }

    /**
     * Calculate / Recalculate the mesh of this chunk.
     */
    public void RecalculateMesh()
    {
        NativeList<Vector3> vert = new NativeList<Vector3>(Allocator.Temp);
        NativeList<int> tris     = new NativeList<int>(Allocator.Temp);
        NativeList<Vector2> uvs  = new NativeList<Vector2>(Allocator.Temp);

        var center = (Vector3)(new float3(size) / 2f) - new Vector3(.5f, .5f, .5f);

        for(int i = 0; i < cubes.Length; i++)
        {
            if(cubes[i].isDestroy) 
                continue;

            var pos = (Vector3)cubes[i].localPos;
            
            for(int f = 0; f < 6; f++)
            {
                if(cubes[i].GetFace(f))
                {
                    int findex = f * 4;
                    int tindex = vert.Length;

                    // Add vertices
                    vert.Add(CubeGeometry.vert[CubeGeometry.cubeTris[findex + 0]] + pos - center);
                    vert.Add(CubeGeometry.vert[CubeGeometry.cubeTris[findex + 1]] + pos - center);
                    vert.Add(CubeGeometry.vert[CubeGeometry.cubeTris[findex + 2]] + pos - center);
                    vert.Add(CubeGeometry.vert[CubeGeometry.cubeTris[findex + 3]] + pos - center);

                    // Add triangles
                    tris.Add(0 + tindex);
                    tris.Add(1 + tindex);
                    tris.Add(2 + tindex);
                    tris.Add(0 + tindex);
                    tris.Add(2 + tindex);
                    tris.Add(3 + tindex);

                    // Add UVs
                    uvs.Add(new Vector2(0, 0));
                    uvs.Add(new Vector2(0, 1));
                    uvs.Add(new Vector2(1, 1));
                    uvs.Add(new Vector2(1, 0));
                }
            }
        }

        // Clear the mesh
        mesh.Clear();

        // Set mesh data
        mesh.SetVertices<Vector3>(vert);
        mesh.SetIndices<int>(tris, 0, tris.Length, MeshTopology.Triangles, 0);
        mesh.SetUVs<Vector2>(0, uvs);

        // Recalculate normals & bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Dispose all NativeList
        vert.Dispose();
        tris.Dispose();
        uvs.Dispose();

        // Update meshCollider
        if(updateCollider)
        {
            mc.sharedMesh = null;
            mc.sharedMesh = mesh;
        }
    }

    private void Update()
    {

        if(Input.GetKey(KeyCode.A))
        {
            noisePosition += Vector3.left;
            isKeyPressed = true;
        }
        
        if(Input.GetKey(KeyCode.D))
        {
            noisePosition += Vector3.right;
            isKeyPressed = true;
        }
        
        if(Input.GetKey(KeyCode.W))
        {
            noisePosition += Vector3.forward;
            isKeyPressed = true;
        }

        if(Input.GetKey(KeyCode.S))
        {
            noisePosition += Vector3.back;
            isKeyPressed = true;
        }

        if(updateEveryFrame || isKeyPressed)
        {
            RecalculateCubes();
            RecalculateMesh();

            if(isKeyPressed)
                isKeyPressed = false;
        }
    }

    private Vector3 to3D(int i)
    {
        float x = i % size;
        float y = ( i / size ) % size;
        float z = i / ( size * size );

        return new Vector3(x, y, z);
    }

}