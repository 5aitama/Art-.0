using Unity.Mathematics;

public struct Cube
{
    public bool isDestroy;
    public bool zmin, xmax, zmax, xmin, ymax, ymin;
    public float3 localPos;
    
    public bool GetFace(int index)
    {
        switch(index)
        {
            case 0: return zmin;
            case 1: return xmax;
            case 2: return zmax;
            case 3: return xmin;
            case 4: return ymax;
            case 5: return ymin;
            default: throw new System.Exception("Can't get face at index {index} because out of bound...");
        }
    }
}