using UnityEngine;

public static class CubeGeometry
{
    public static Vector3[] vert = new Vector3[8]
    {
        new Vector3(-.5f, -.5f, -.5f),
        new Vector3(-.5f,  .5f, -.5f),
        new Vector3( .5f,  .5f, -.5f),
        new Vector3( .5f, -.5f, -.5f),

        new Vector3(-.5f, -.5f,  .5f),
        new Vector3(-.5f,  .5f,  .5f),
        new Vector3( .5f,  .5f,  .5f),
        new Vector3( .5f, -.5f,  .5f),
    };

    public static readonly int[] cubeTris = new int[]
    {
        0, 1, 2, 3,
        3, 2, 6, 7,
        7, 6, 5, 4,
        4, 5, 1, 0,
        1, 5, 6, 2,
        4, 0, 3, 7,
    };
}
